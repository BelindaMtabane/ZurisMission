using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class PlayerMovement : MonoBehaviour
{
    void Awake()
    {
        if (GetComponent<PlayerController>() != null || FindFirstObjectByType<PlayerController>() != null)
        {
            enabled = false;
            Debug.Log("[PlayerMovement] Disabled because PlayerController is active.");
        }
    }

    //Create variables
    public float[] lanePositions = { -8f, -4f, 0f, 4f, 8f };
    private int currentLane = 2;
    public float playerSpeed = 25f;
    public float playerJumpPower = 10f;
    public float playerSpeedBoost = 10f;
    public float playerSlowDown = 3f;
    public bool isGrounded;
    public Rigidbody rb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Check if rigidbody is attached to the player, if not add one
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        else
        {
            Debug.Log("Rigidbody already attached to the player.");
        }
        if (SceneManager.GetActiveScene().name == "Level2")
            InvokeRepeating("RandomLaneSwitch", 2f, 2f);
    }

    // Update is called once per frame
    void Update()
    {
        //Call the player movement and jumping mechanics in the main method
        if (isGrounded)

            MovementMECH();

        if (isGrounded && Input.GetKeyDown(KeyCode.Space))

            JumpMECH();

        //UpdateUI();
    }
    //Create a method that functions the PLayer Jumping Mechanic
    public void JumpMECH()
    {
        //Check if the player is grounded and the space key is pressed to apply a force to the player
        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            
            //Apply force to the player to make them jump
            rb.AddForce(Vector3.up * playerJumpPower, ForceMode.Impulse);
            isGrounded = false;
        }
    }
    public void MovementMECH()
    {
        if (isGrounded)
        {
            Vector3 forwardMVMT = transform.forward * playerSpeed * Time.deltaTime;

            if (SceneManager.GetActiveScene().name == "Level2")
            {
                // AI: snap to randomly chosen lane
                Vector3 targetPosition = new Vector3(lanePositions[currentLane], transform.position.y, transform.position.z);
                transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * playerSpeed);
            }
            else
            {
                // Player: A/D keys for left/right
                if (Input.GetKey(KeyCode.A))
                    transform.Translate(Vector3.left * playerSpeed * Time.deltaTime * 2);
                if (Input.GetKey(KeyCode.D))
                    transform.Translate(Vector3.right * playerSpeed * Time.deltaTime * 2);
            }

            rb.MovePosition(rb.position + forwardMVMT);
        }
    }
    void RandomLaneSwitch()
    {
        currentLane = Random.Range(0, 4);
    }
    public void SlowMECH()
    {
        //Set player speed to the slow down variable
        playerSpeed = playerSlowDown;
    }

    public void SpeedBoostMECH()
    {
        //Set player speed to the speed boost variable
        playerSpeed = playerSpeedBoost;
    }


    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            return;
        }

        if (collision.gameObject.GetComponent<PitObstacle>() != null)
        {
            Physics.IgnoreCollision(GetComponent<Collider>(), collision.collider);
            PushThroughPit();
            return;
        }

        isGrounded = false;
    }

    // Keep pushing through if physics contact lingers for multiple frames
    public void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.GetComponent<PitObstacle>() != null)
            PushThroughPit();
    }

    void PushThroughPit()
    {
        // Restore forward velocity so the Rigidbody isn't stuck against the pit surface
        rb.linearVelocity = new Vector3(
            transform.forward.x * playerSpeed,
            rb.linearVelocity.y,
            transform.forward.z * playerSpeed
        );
    }
}
