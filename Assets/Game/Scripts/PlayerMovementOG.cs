using UnityEngine;

public class PlayerMovementOG : MonoBehaviour
{
    //Create variables
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

            Vector3 forwardMVMT = transform.forward * playerSpeed * Time.deltaTime;
            isGrounded = false;
        }
    }
    public void MovementMECH()
    {
        //Get player input using the wasd key functionality
        if (isGrounded)
        {
            //Move forward continously
            Vector3 forwardMVMT = transform.forward * playerSpeed * Time.deltaTime;
            //Crouching vector declaration
            Vector3 crouchSCALE = new Vector3(1f, 0.5f, 1f);
            Vector3 playerSCALE = new Vector3(1f, 1f, 1f);

            if (Input.GetKey(KeyCode.A))
            {
                transform.Translate(Vector3.left * playerSpeed * Time.deltaTime * 2);
            }
            if (Input.GetKey(KeyCode.D))
            {
                transform.Translate(Vector3.right * playerSpeed * Time.deltaTime * 2);
            }
            /*if (Input.GetKeyDown(KeyCode.P))
            {
                transform.localScale = crouchSCALE;
                transform.position = new Vector3(transform.position.x, transform.position.y - 0.5f, transform.position.z);
            }
            else if (Input.GetKeyUp(KeyCode.P))
            {
                //Return the player to their original scale
                transform.localScale = playerSCALE;
                transform.position = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);
  }*/

            rb.MovePosition(rb.position + forwardMVMT);
        }
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


    //Create a method that detects when the player collides with the ground and sets isGrounded to true
    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            //Set Ground to be true when Player collides with the ground
            isGrounded = true;
        }
        if (!collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }

    }
}

