using UnityEngine;

public class MovingObstacle : MonoBehaviour
{
    /*private Vector3 startPosition;
    public float moveDistance = 2f;//How far it moves
    public float moveSpeed = 1f;//Speed of the movement
    public float delayBeforeMoving = 60f;

    private float timer = 0f;//Tracks the time when gameplay starts
    private bool canMove = false;//Indicates whether the object can move
    private float moveTimer = 0f;

    void Start()
    {
        startPosition = transform.position;//Store the initial position of the objects
    }

    void Update()
    {
        timer += Time.deltaTime;//Increment the timer

        //Check if the object can move and if the timer has reached the delay before moving
        if (!canMove && timer >= delayBeforeMoving)
        {
            canMove = true;//Enable movement after the delay
            moveTimer = 0f;//Reset the move timer when moved
        }

        if (canMove)
        {
            moveTimer += Time.deltaTime;//Increment the move timer
            //Move the player continously forward
            transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
            //Creat a if statement to check the keys that the player is moving on 
            if (Input.GetKey(KeyCode.A))
            {
                transform.Translate(Vector3.left * moveSpeed * Time.deltaTime);
            }
            if (Input.GetKey(KeyCode.D))
            {
                transform.Translate(Vector3.right * moveSpeed * Time.deltaTime);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Handle collision with the player, e.g., reduce health or reset position
            Debug.Log("Player hit by moving obstacle!");
        }
    }*/
}
