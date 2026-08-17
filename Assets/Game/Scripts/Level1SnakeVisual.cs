using UnityEngine;

/// <summary>
/// Arcade snake body bob and red forked tongue flick.
/// </summary>
public class Level1SnakeVisual : MonoBehaviour
{
    [SerializeField] private Transform bodyRoot;
    [SerializeField] private Transform headRoot;
    [SerializeField] private Transform tongueCenter;
    [SerializeField] private Transform tongueLeft;
    [SerializeField] private Transform tongueRight;

    Vector3 bodyBase;
    Vector3 headBase;
    Vector3 tongueCenterBase;
    Vector3 tongueLeftBase;
    Vector3 tongueRightBase;
    float tonguePhase;

    public void Bind(Transform body, Transform head, Transform tongueC, Transform tongueL, Transform tongueR)
    {
        bodyRoot = body;
        headRoot = head;
        tongueCenter = tongueC;
        tongueLeft = tongueL;
        tongueRight = tongueR;
        CacheBases();
    }

    void CacheBases()
    {
        if (bodyRoot != null) bodyBase = bodyRoot.localPosition;
        if (headRoot != null) headBase = headRoot.localPosition;
        if (tongueCenter != null) tongueCenterBase = tongueCenter.localPosition;
        if (tongueLeft != null) tongueLeftBase = tongueLeft.localPosition;
        if (tongueRight != null) tongueRightBase = tongueRight.localPosition;
    }

    void OnEnable()
    {
        CacheBases();
    }

    void Update()
    {
        if (!isActiveAndEnabled) return;

        float t = Time.time;
        if (bodyRoot != null)
        {
            bodyRoot.localPosition = bodyBase + new Vector3(0f, Mathf.Sin(t * 6f) * 0.04f, 0f);
        }

        if (headRoot != null)
        {
            headRoot.localPosition = headBase + new Vector3(Mathf.Sin(t * 4f) * 0.03f, Mathf.Sin(t * 5f) * 0.05f, 0f);
        }

        tonguePhase += Time.deltaTime * 7f;
        float extend = (Mathf.Sin(tonguePhase) + 1f) * 0.5f;
        float fork = extend * 0.22f;

        if (tongueCenter != null)
        {
            tongueCenter.localPosition = tongueCenterBase + new Vector3(0f, 0f, extend * 0.35f);
            tongueCenter.localScale = new Vector3(0.14f, 0.14f, 0.45f + extend * 0.35f);
        }

        if (tongueLeft != null)
        {
            tongueLeft.localPosition = tongueLeftBase + new Vector3(-fork, -fork * 0.35f, extend * 0.42f);
        }

        if (tongueRight != null)
        {
            tongueRight.localPosition = tongueRightBase + new Vector3(fork, -fork * 0.35f, extend * 0.42f);
        }
    }
}
