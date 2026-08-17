using UnityEngine;

public class Level3HorizontalEnemy : MonoBehaviour
{
    public enum EnemyKind { Snake, Warthog }

    const float TriggerDistance = 52f;

    [SerializeField] EnemyKind kind = EnemyKind.Snake;
    [SerializeField] Level3EnemyPace pace = Level3EnemyPace.Slow;
    [SerializeField] bool movingRight = true;
    [SerializeField] float healthDamage = 8f;
    [SerializeField] GameObject warningRoot;
    [SerializeField] GameObject visualRoot;

    enum Phase { Wait, Warning, Cross, Done }
    Phase phase = Phase.Wait;
    float speed;
    float warningTimer = 1.6f;
    bool hit;
    Transform player;
    float leftX;
    float rightX;

    public void Setup(EnemyKind enemyKind, Level3EnemyPace enemyPace, bool goRight, GameObject warning, GameObject visual)
    {
        kind = enemyKind;
        pace = enemyPace;
        movingRight = goRight;
        warningRoot = warning;
        visualRoot = visual;
        speed = kind == EnemyKind.Snake ? Level3EnemySpeeds.Snake(pace) : Level3EnemySpeeds.Warthog(pace);
        healthDamage = kind == EnemyKind.Snake
            ? (pace == Level3EnemyPace.Fast ? 12f : pace == Level3EnemyPace.Medium ? 9f : 7f)
            : (pace == Level3EnemyPace.Fast ? 16f : pace == Level3EnemyPace.Medium ? 12f : 10f);
        warningTimer = pace == Level3EnemyPace.Fast ? 1.2f : pace == Level3EnemyPace.Medium ? 1.6f : 2.1f;

        leftX = LevelLanes.X(0) - 6f;
        rightX = LevelLanes.X(LevelLanes.Count - 1) + 6f;
        Vector3 p = transform.position;
        p.x = movingRight ? leftX : rightX;
        transform.position = p;
        if (warningRoot != null) warningRoot.SetActive(false);
        if (visualRoot != null) visualRoot.SetActive(false);
    }

    void Update()
    {
        if (RunStateManager.Instance != null && !RunStateManager.Instance.IsPlaying) return;
        CachePlayer();
        if (player == null) return;

        Vector3 p = transform.position;
        p.y = Level3Ground.SurfaceY + (kind == EnemyKind.Warthog ? 0.7f : 0.4f);
        transform.position = p;

        switch (phase)
        {
            case Phase.Wait:
                if (player.position.z > transform.position.z - TriggerDistance)
                {
                    phase = Phase.Warning;
                    if (warningRoot != null) warningRoot.SetActive(true);
                    string label = kind == EnemyKind.Snake ? "SNAKE CROSSING!" : "WARTHOG INCOMING!";
                    Level3FeedbackUI.Show(label, new Color(1f, 0.55f, 0.15f), warningTimer);
                }
                break;
            case Phase.Warning:
                PulseWarning();
                warningTimer -= Time.deltaTime;
                if (warningTimer <= 0f)
                {
                    if (warningRoot != null) warningRoot.SetActive(false);
                    if (visualRoot != null) visualRoot.SetActive(true);
                    phase = Phase.Cross;
                }
                break;
            case Phase.Cross:
                float dir = movingRight ? 1f : -1f;
                p = transform.position;
                p.x += dir * speed * Time.deltaTime;
                transform.position = p;
                if ((movingRight && p.x > rightX) || (!movingRight && p.x < leftX))
                {
                    phase = Phase.Done;
                    Destroy(gameObject);
                }
                break;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (hit || phase != Phase.Cross) return;
        if (!other.CompareTag("Player")) return;
        if (RunStateManager.Instance != null && !RunStateManager.Instance.IsPlaying) return;

        PlayerController controller = other.GetComponent<PlayerController>();
        if (controller != null && !controller.IsGrounded)
        {
            Level3FeedbackUI.Show("JUMPED!", new Color(0.4f, 0.95f, 0.45f), 0.8f);
            hit = true;
            return;
        }

        float laneDelta = Mathf.Abs(other.transform.position.x - transform.position.x);
        if (laneDelta > 2.4f)
        {
            Level3FeedbackUI.Show("DODGED!", new Color(0.4f, 0.95f, 0.45f), 0.8f);
            hit = true;
            return;
        }

        hit = true;
        FindFirstObjectByType<HUDControls>()?.ChangeHealth(-healthDamage, kind == EnemyKind.Snake ? "A snake bit you!" : "A warthog hit you!");
        Level3FeedbackUI.Show(kind == EnemyKind.Snake ? "SNAKE!" : "WARTHOG!", new Color(0.9f, 0.25f, 0.15f), 1f);
    }

    void PulseWarning()
    {
        if (warningRoot == null) return;
        warningRoot.transform.localScale = Vector3.one * (1f + Mathf.Sin(Time.time * 8f) * 0.18f);
    }

    void CachePlayer()
    {
        if (player != null) return;
        PlayerController pc = FindFirstObjectByType<PlayerController>();
        if (pc != null) player = pc.transform;
    }
}
