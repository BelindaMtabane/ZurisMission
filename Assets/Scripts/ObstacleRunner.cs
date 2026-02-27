using UnityEngine;

public class ObstacleTimer : MonoBehaviour
{
    public GameObject animalONE;
    public GameObject animalTWO;
    public GameObject people;
    public GameObject obstacles;
    public float delayTime;

    void animalsSpeedONE()
    {
        delayTime += Time.deltaTime;

        if (delayTime >= 5)
        {
            animalONE.SetActive(false);
        }
        else
        {
            animalONE.SetActive(true);
            //set objects speed
            int randomSpeedANI1 = Random.Range(1, 5);

            transform.Translate(Vector3.right * randomSpeedANI1 * Time.deltaTime);

            transform.Translate(Vector3.left * randomSpeedANI1 * Time.deltaTime);

            delayTime = 0;
        }

    }
    void animalsSpeedTWO()
    {
        delayTime += Time.deltaTime;

        if (delayTime <= 10)
        {
            animalTWO.SetActive(false);
        }
        else
        {
            animalTWO.SetActive(true);
            //set objects speed
            int randomSpeedANI2 = Random.Range(4, 10);

            transform.Translate(Vector3.left * randomSpeedANI2 * Time.deltaTime);

            transform.Translate(Vector3.right * randomSpeedANI2 * Time.deltaTime);

        }

    }
    void peopleSpeed()
    {
        delayTime += Time.deltaTime;
        if (delayTime <= 15)
        {
            people.SetActive(false);
        }
        else
        {
            people.SetActive(true);
            //set objects speed
            int randomSpeedPEO = Random.Range(1, 5);
            transform.Translate(Vector3.back * randomSpeedPEO * Time.deltaTime);
            delayTime = 0;
        }
    }
    /*void obstaclesSpeed()
    {
        delayTime += Time.deltaTime;
        if (delayTime <= 20)
        {
            obstacles.SetActive(false);
        }
        else
        {
            obstacles.SetActive(true);
            //set objects speed
            int randomSpeedOBS = Random.Range(1, 5);
            transform.Translate(Vector3.left * randomSpeedOBS * Time.deltaTime);
            transform.Translate(Vector3.right * randomSpeedOBS * Time.deltaTime);
        }
    }*/


}

