using UnityEngine;

/// <summary>
/// Lightweight procedural motion for the primitive-built creature and villager prefabs,
/// which have no rigs or animation clips.
/// Walk swings children named "Leg*" by distance travelled, Bob squashes "Body",
/// Work swings "WorkArm".
/// </summary>
public sealed class CreatedPropMotion : MonoBehaviour
{
    public enum Mode { Bob, Walk, Work }

    public Mode mode = Mode.Bob;
    public float speed = 2f;
    public float amount = 1f;

    Transform[] legs;
    Quaternion[] legRest;
    Transform tail;
    Quaternion tailRest;
    Transform body;
    Vector3 bodyRestScale;
    Transform workArm;
    Quaternion workArmRest;
    Vector3 lastPosition;
    float phase;

    void Awake()
    {
        var found = new System.Collections.Generic.List<Transform>();
        foreach (Transform t in GetComponentsInChildren<Transform>(true))
        {
            if (t.name.StartsWith("Leg")) found.Add(t);
            else if (t.name == "Tail") tail = t;
            else if (t.name == "Body" && body == null) body = t;
            else if (t.name == "WorkArm") workArm = t;
        }

        legs = found.ToArray();
        legRest = new Quaternion[legs.Length];
        for (int i = 0; i < legs.Length; i++) legRest[i] = legs[i].localRotation;
        if (tail != null) tailRest = tail.localRotation;
        if (body != null) bodyRestScale = body.localScale;
        if (workArm != null) workArmRest = workArm.localRotation;
        lastPosition = transform.position;
        phase = Mathf.Abs(GetInstanceID() % 628) / 100f;
    }

    void Update()
    {
        switch (mode)
        {
            case Mode.Walk: UpdateWalk(); break;
            case Mode.Work: UpdateWork(); break;
            default: UpdateBob(); break;
        }
    }

    void UpdateWalk()
    {
        Vector3 delta = transform.position - lastPosition;
        delta.y = 0f;
        lastPosition = transform.position;
        float stride = delta.magnitude / Mathf.Max(0.01f, transform.lossyScale.y);
        phase += stride * speed + Time.deltaTime * 2f;

        float moving = Mathf.Clamp01(stride / Mathf.Max(0.0001f, Time.deltaTime) * 0.5f);
        for (int i = 0; i < legs.Length; i++)
        {
            string n = legs[i].name;
            bool firstPair = n.EndsWith("FL") || n.EndsWith("BR") || n == "Leg_L";
            float swing = Mathf.Sin(phase + (firstPair ? 0f : Mathf.PI)) * 32f * amount * moving;
            legs[i].localRotation = legRest[i] * Quaternion.Euler(swing, 0f, 0f);
        }

        if (tail != null)
            tail.localRotation = tailRest * Quaternion.Euler(0f, Mathf.Sin(Time.time * 7f) * 18f * amount, 0f);
    }

    void UpdateWork()
    {
        if (workArm == null) return;
        float swing = Mathf.Sin(Time.time * speed * Mathf.PI + phase) * 38f * amount;
        workArm.localRotation = workArmRest * Quaternion.Euler(swing, 0f, 0f);
    }

    void UpdateBob()
    {
        if (body == null) return;
        float s = Mathf.Sin(Time.time * speed * Mathf.PI + phase) * 0.07f * amount;
        body.localScale = new Vector3(bodyRestScale.x * (1f - s * 0.5f), bodyRestScale.y * (1f + s), bodyRestScale.z * (1f - s * 0.5f));
    }
}
