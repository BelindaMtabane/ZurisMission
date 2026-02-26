using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    //Create variables
    public static float playerSpeed = 5f;
    public static float playerJumpPower = 5f;
    public static float playerSpeedBoost = 10f;
    public static float playerSlowDown = 3f;
    public static bool isGrounded;
    public float waterLevel = 100f;
    public float agilityLevel = 100f;
    private int pressCounter;
    public Rigidbody rb;
    ObstacleTimer timer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Check if rigidbody is attached to the player, if not add one
        rb = GetComponent<Rigidbody>();
        timer = GetComponent<ObstacleTimer>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        else
        {
            Debug.Log("Rigidbody already attached to the player.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Call the player movement and jumping mechanics in the main method
        if (isGrounded)

            MovementMECH();

        if (isGrounded && Input.GetKeyDown(KeyCode.Space))

            JumpMECH();
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
        //Get player input using the wasd key functionality
        if (isGrounded)
        {
            if (Input.GetKey(KeyCode.W))
            {
                transform.Translate(Vector3.forward * playerSpeed * Time.deltaTime);
            }
            if (Input.GetKey(KeyCode.S))
            {
                transform.Translate(Vector3.back * playerSpeed * Time.deltaTime);
            }
            if (Input.GetKey(KeyCode.A))
            {
                transform.Translate(Vector3.left * playerSpeed * Time.deltaTime);
            }
            if (Input.GetKey(KeyCode.D))
            {
                transform.Translate(Vector3.right * playerSpeed * Time.deltaTime);
            }
            else if (Input.GetKey(KeyCode.P))
            {
                SlowMECH();
            }
            else if (Input.GetKey(KeyCode.O))
            {
                SpeedBoostMECH();
            }
            WaterAgilityManager();
        }
    }
    public void SlowMECH()
    {
        //Set player speed to the slow down variable
        playerSpeed = playerSlowDown;
        WaterAgilityManager();   
    }

    public void SpeedBoostMECH()
    {
        //Set player speed to the speed boost variable
        playerSpeed = playerSpeedBoost;
        WaterAgilityManager();
    }

    void WaterAgilityManager()
    {
        float waterLeveldecrease = UnityEngine.Random.Range(2f, 20f);
        //Check if player is fast or slow or normal
        if (playerSpeed == 5f)
        {
            waterLevel -= waterLeveldecrease;
            agilityLevel += 5f;

        }
        else if (playerSpeed == playerSpeedBoost)
        {
            //Decrease water level and increase player speed
            waterLevel -= 10f;
            agilityLevel += 5f;

        }
        else if (playerSpeed == playerSlowDown)
        {
            waterLevel += waterLeveldecrease;
            agilityLevel -= 8f;
        }
    }

    //Create a method that detects when the player collides with the ground and sets isGrounded to true
    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            //Set Ground to be true when Player collides with the ground
            isGrounded = true;
        }
        else
        {
            //Set if Player did not contact the ground / is in the air
            isGrounded = false;
        }
    }
}
