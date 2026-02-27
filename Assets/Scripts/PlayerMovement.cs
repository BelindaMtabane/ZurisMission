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
    public float villageWaterLevel = 0f;
    public float agilityLevel = 100f;
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

            if (Input.GetKeyDown(KeyCode.P))
            {
                SlowMECH();
            }
            else if (Input.GetKeyDown(KeyCode.O))
            {
                SpeedBoostMECH();
            }

            if (Input.GetKeyUp(KeyCode.P) || Input.GetKeyUp(KeyCode.O))
            {
                WaterAgilityManager();
            }

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

    void WaterAgilityManager()
    {
        float waterLeveldecrease = UnityEngine.Random.Range(2f, 10f);
        //Check if player is fast or slow or normal
        if (playerSpeed == playerSpeedBoost)
        {
            //Decrease water level and increase player speed
            waterLevel -= waterLeveldecrease;
            agilityLevel += 5f;
            Debug.Log("FAST, water level decreased by " + waterLeveldecrease + " and agility level is" + agilityLevel);
            
        }
        else if (playerSpeed == playerSlowDown)
        {
            waterLevel += waterLeveldecrease;
            agilityLevel -= 8f;
            Debug.Log("SLOW, water level decreased by " + waterLeveldecrease + " and agility level is" + agilityLevel);
        }
        AgiliyLimit();
        WaterLimit();


    }

    void AgiliyLimit()
    {
        //Check if agility is lower than 40 and decrease water by 1 if the 40 increases but if it decreases increase water level    }
        if (agilityLevel < 40f)
        {
            waterLevel -= 1f;
            Debug.Log("Agility is low to carry the water, water level decreases gradually");
        }
        else if (agilityLevel > 40f)
        {
            waterLevel += 2f;
            Debug.Log("Agility is increasing, water level increases gradually");
        }
    }
    void WaterLimit()
    {
        //Check if water level is lower than 50 and increases agility  by 2 
        if (waterLevel <+ 50f)
        {
            agilityLevel += 3f;
            playerSpeed = 4f;
            Debug.Log("Water level is low, agility level decreases gradually");
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
        /*else if (collision.gameObject.CompareTag("Water"))
        {
            waterLevel += 7f;
            agilityLevel -= 5f;

        }
        else if (collision.gameObject.CompareTag("VillageTank"))
        {
            villageWaterLevel += waterLevel;
            waterLevel -= 0f;
        }
        else if (collision.gameObject.CompareTag("Obstacle"))
        {
            waterLevel -= 5f;
            agilityLevel -= 3f;
        }
        else if (collision.gameObject.CompareTag("Food"))
        {
            agilityLevel += 7f;
        }
        else if (collision.gameObject.CompareTag("Drink"))
        {
            waterLevel += 10f;
            agilityLevel += 6f;
        }
        else if (collision.gameObject.CompareTag("Animal"))
        {
            waterLevel -= 12f;
            agilityLevel -= 6f;
        }
        else if (collision.gameObject.CompareTag("People"))
        {
            waterLevel -= 4f;
            agilityLevel += 2f;
        }*/
        else
        {
            //Set if Player did not contact the ground / is in the air
            isGrounded = false;
        }
    }
}
