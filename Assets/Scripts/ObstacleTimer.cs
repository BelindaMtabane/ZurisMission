using UnityEngine;

public class ObstacleTimer : MonoBehaviour
{
    //Create variables
    public static float timerSet; 
     PlayerMovement playerMovement;
    //Set a random time for the obstacle
    private float breatheTime = 10f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void Start() 
    {
        //Get the PlayerMovement script attached to the player
        playerMovement = GetComponent<PlayerMovement>(); 
        if (playerMovement != null)
        { 
            Debug.Log("PlayerMovement script found on the player."); 
        }
        else 
        {
            Debug.LogError("PlayerMovement script not found on the player. Please attach the PlayerMovement script to the player."); 
        } 
    }
    void Update()
    {
        breatheTime -= Time.deltaTime;
        Debug.Log("Breathe Time: " + breatheTime);

        timerSet = UnityEngine.Random.Range(1f, 3f);
        Debug.Log("Timer Set to: " + timerSet);
        breatheTime = 10f;

    }


}

