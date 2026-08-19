using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// MainGame: every 6 seconds a snake from the scene crosses in front of the player.
/// </summary>
public class SnakePassDirector : MonoBehaviour
{
    const float Interval = 6f;
    bool goRight = true;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        // Replaced by Level 1 lane snakes.
    }

    void Start()
    {
        enabled = false;
    }

    IEnumerator Loop()
    {
        yield return new WaitForSeconds(4f);
        while (true)
        {
            if (RunStateManager.Instance == null || RunStateManager.Instance.IsPlaying)
            {
                SpawnPass();
            }
            yield return new WaitForSeconds(Interval);
        }
    }

    void SpawnPass()
    {
        Transform player = FindPlayer();
        if (player == null) return;

        GameObject template = GameObject.Find("Snakes");
        if (template == null)
        {
            template = GameObject.Find("Snakes (1)");
        }

        int dir = goRight ? 1 : -1;
        goRight = !goRight;

        float startX = dir > 0 ? -16f : 16f;
        float endX = dir > 0 ? 16f : -16f;
        Vector3 pos = new Vector3(startX, 0.6f, player.position.z + 14f);

        GameObject snake;
        if (template != null)
        {
            snake = Instantiate(template, pos, template.transform.rotation);
            snake.SetActive(true);
        }
        else
        {
            snake = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            snake.GetComponent<Renderer>().material.color = new Color(0.2f, 0.55f, 0.15f);
            snake.transform.position = pos;
            snake.transform.localScale = new Vector3(0.6f, 0.35f, 2.4f);
            snake.transform.rotation = Quaternion.Euler(90f, 90f, 0f);
        }

        snake.name = "SnakePass";
        SnakePassHazard pass = snake.GetComponent<SnakePassHazard>();
        if (pass == null)
        {
            pass = snake.AddComponent<SnakePassHazard>();
        }
        pass.direction = dir;
        pass.endX = endX;
        pass.speed = 16f;

        Debug.Log("[Snake] Crossing " + (dir > 0 ? "left to right" : "right to left"));
    }

    static Transform FindPlayer()
    {
        PlayerController pc = FindFirstObjectByType<PlayerController>();
        if (pc != null) return pc.transform;
        GameObject p = GameObject.Find("Player");
        return p != null ? p.transform : null;
    }
}
