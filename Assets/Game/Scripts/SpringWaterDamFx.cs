using UnityEngine;

/// <summary>
/// Spring dam VFX — NamuFX Stylized Water Effect Pack when available, procedural fallback otherwise.
/// </summary>
public class SpringWaterDamFx : MonoBehaviour
{
    [SerializeField] float sizeScale = 1f;
    [SerializeField] float basinDepth = 2.75f;
    [SerializeField] float basinWidth = 3.65f;
    [SerializeField] float rangeRevealDistance = 65f;
    [SerializeField] float springSproutDistance = 40f;

    bool built;
    Transform player;
    Transform springFxRoot;
    Transform rangeMarker;
    float springScale;
    float playerSearchTimer;

    public void Configure(float scale, float width, float depth)
    {
        sizeScale = scale;
        basinWidth = width;
        basinDepth = depth;
        BuildIfNeeded();
    }

    void Start() => BuildIfNeeded();

    void Update()
    {
        if (!built) BuildIfNeeded();
        FindPlayerWhenNeeded();

        if (player == null)
        {
            SetProximityState(false, false);
            return;
        }

        Vector3 offset = player.position - transform.position;
        offset.y = 0f;
        float distance = offset.magnitude;
        bool showRange = distance <= rangeRevealDistance;
        bool sprout = distance <= springSproutDistance;
        SetProximityState(showRange, sprout);
    }

    void BuildIfNeeded()
    {
        if (built) return;
        built = true;

        GameObject fxRoot = new GameObject("ProximitySpringEffects");
        fxRoot.transform.SetParent(transform, false);
        springFxRoot = fxRoot.transform;

        if (NamuWaterFxLibrary.IsAvailable)
            BuildNamuEffects();
        else
            BuildFallbackEffects();

        BuildVisibleSpringJet();
        BuildRangeMarker();
        springScale = 0f;
        springFxRoot.localScale = Vector3.zero;
        springFxRoot.gameObject.SetActive(false);
        rangeMarker.gameObject.SetActive(false);
    }

    void BuildNamuEffects()
    {
        var root = new GameObject("NamuFx");
        root.transform.SetParent(springFxRoot, false);

        float s = Mathf.Max(sizeScale, 0.35f);
        Vector3 spring = new Vector3(0f, RunnerVisualScale.F(0.14f), RunnerVisualScale.F(0.02f));
        Vector3 spill = new Vector3(0f, RunnerVisualScale.F(0.05f), basinDepth * 0.34f);
        float fxScale = 0.55f + s * 0.35f;

        NamuWaterFxLibrary.Spawn(root.transform, "Bubbles_Vertical_Loop", spring, Vector3.zero, fxScale * 1.1f);
        NamuWaterFxLibrary.Spawn(root.transform, "Water_Beam", spring + Vector3.up * 0.04f, new Vector3(-12f, 0f, 0f), fxScale * 0.85f);
        NamuWaterFxLibrary.Spawn(root.transform, "Hit_02", spring + Vector3.up * RunnerVisualScale.F(0.22f), Vector3.zero, fxScale * 1.25f);
        NamuWaterFxLibrary.Spawn(root.transform, "Bubbles_Burst", spring + Vector3.up * 0.02f, Vector3.zero, fxScale * 0.9f);

        GameObject spillFx = NamuWaterFxLibrary.Spawn(root.transform, "Water_Splash_Multiple", spill, new Vector3(-90f, 0f, 0f), fxScale * 1.15f);
        if (spillFx != null)
        {
            Vector3 spillEnd = spill + new Vector3(0f, 0f, basinDepth * 0.28f);
            NamuWaterFxLibrary.Spawn(root.transform, "Water_Impact", spillEnd, new Vector3(-90f, 0f, 0f), fxScale * 0.95f);
        }
    }

    void BuildFallbackEffects()
    {
        float s = Mathf.Max(sizeScale, 0.35f);
        Vector3 springOrigin = new Vector3(0f, RunnerVisualScale.F(0.14f), RunnerVisualScale.F(0.02f));
        CreateBubbleSystem("SpringBubbles", springOrigin, s);
        CreateSpillSystem("SpillSpray", new Vector3(0f, RunnerVisualScale.F(0.07f), basinDepth * 0.36f), basinWidth * 0.42f, s);
        CreateMistSystem("SpringMist", springOrigin + Vector3.up * RunnerVisualScale.F(0.18f), s);
    }

    ParticleSystem CreateBubbleSystem(string name, Vector3 localPos, float scale)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(springFxRoot, false);
        go.transform.localPosition = localPos;

        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.loop = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.55f, 1.1f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.35f * scale, 1.2f * scale);
        main.startSize = new ParticleSystem.MinMaxCurve(0.06f * scale, 0.16f * scale);
        main.startColor = new Color(0.65f, 0.92f, 1f, 0.75f);
        main.maxParticles = Mathf.RoundToInt(80 + scale * 40f);
        main.simulationSpace = ParticleSystemSimulationSpace.Local;

        var emission = ps.emission;
        emission.rateOverTime = 18f + scale * 22f;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.22f * scale;
        shape.rotation = new Vector3(-90f, 0f, 0f);

        ApplyParticleMaterial(ps);
        return ps;
    }

    ParticleSystem CreateSpillSystem(string name, Vector3 localPos, float spillWidth, float scale)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(springFxRoot, false);
        go.transform.localPosition = localPos;
        go.transform.localRotation = Quaternion.Euler(-35f, 0f, 0f);

        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.loop = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.35f, 0.75f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(1.5f * scale, 3.5f * scale);
        main.startSize = new ParticleSystem.MinMaxCurve(0.04f * scale, 0.11f * scale);
        main.startColor = new Color(0.75f, 0.95f, 1f, 0.65f);
        main.maxParticles = Mathf.RoundToInt(60 + scale * 30f);
        main.simulationSpace = ParticleSystemSimulationSpace.Local;

        var emission = ps.emission;
        emission.rateOverTime = 14f + scale * 16f;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(spillWidth, 0.05f, 0.12f * scale);

        ApplyParticleMaterial(ps);
        return ps;
    }

    ParticleSystem CreateMistSystem(string name, Vector3 localPos, float scale)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(springFxRoot, false);
        go.transform.localPosition = localPos;

        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.loop = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.8f, 1.6f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.08f * scale, 0.35f * scale);
        main.startSize = new ParticleSystem.MinMaxCurve(0.25f * scale, 0.55f * scale);
        main.startColor = new Color(0.85f, 0.97f, 1f, 0.22f);
        main.maxParticles = Mathf.RoundToInt(24 + scale * 18f);
        main.simulationSpace = ParticleSystemSimulationSpace.Local;

        var emission = ps.emission;
        emission.rateOverTime = 6f + scale * 8f;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.35f * scale;

        ApplyParticleMaterial(ps);
        return ps;
    }

    static void ApplyParticleMaterial(ParticleSystem ps)
    {
        ParticleSystemRenderer r = ps.GetComponent<ParticleSystemRenderer>();
        if (r == null) return;
        Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit")
                        ?? Shader.Find("Particles/Standard Unlit")
                        ?? Shader.Find("Legacy Shaders/Particles/Alpha Blended");
        if (shader == null) return;
        Material mat = new Material(shader);
        mat.color = Color.white;
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", Color.white);
        r.material = mat;
        r.renderMode = ParticleSystemRenderMode.Billboard;
    }

    void FindPlayerWhenNeeded()
    {
        if (player != null) return;
        playerSearchTimer -= Time.deltaTime;
        if (playerSearchTimer > 0f) return;
        playerSearchTimer = 0.5f;

        PlayerController controller = FindFirstObjectByType<PlayerController>();
        if (controller != null)
        {
            player = controller.transform;
            return;
        }

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null) player = playerObject.transform;
    }

    void SetProximityState(bool showRange, bool sprout)
    {
        if (rangeMarker != null)
        {
            rangeMarker.gameObject.SetActive(showRange);
            if (showRange)
            {
                rangeMarker.Rotate(0f, 70f * Time.deltaTime, 0f, Space.Self);
                float pulse = 1f + Mathf.Sin(Time.time * 5f) * 0.06f;
                rangeMarker.localScale = Vector3.one * pulse;
            }
        }

        if (springFxRoot == null) return;
        float target = sprout ? 1f : 0f;
        springScale = Mathf.MoveTowards(springScale, target, Time.deltaTime * 3.8f);
        bool visible = springScale > 0.001f || sprout;
        springFxRoot.gameObject.SetActive(visible);
        if (visible)
        {
            float eased = springScale * springScale * (3f - 2f * springScale);
            springFxRoot.localScale = Vector3.one * Mathf.Max(0.001f, eased);
        }
    }

    void BuildRangeMarker()
    {
        GameObject marker = new GameObject("CollectionRangeMarker");
        marker.transform.SetParent(transform, false);
        marker.transform.localPosition = Vector3.up * RunnerVisualScale.F(0.28f);
        rangeMarker = marker.transform;

        Material waterMaterial = CreateRangeMaterial();
        float radius = Mathf.Max(2.6f, basinWidth * 0.74f);

        LineRenderer ring = marker.AddComponent<LineRenderer>();
        ring.loop = true;
        ring.useWorldSpace = false;
        ring.positionCount = 48;
        ring.startWidth = ring.endWidth = 0.24f;
        ring.sharedMaterial = waterMaterial;
        ring.startColor = ring.endColor = new Color(0.15f, 0.8f, 1f, 0.95f);
        for (int i = 0; i < ring.positionCount; i++)
        {
            float angle = i * Mathf.PI * 2f / ring.positionCount;
            ring.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius));
        }

        for (int i = 0; i < 6; i++)
        {
            float angle = i * Mathf.PI * 2f / 6f;
            GameObject droplet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            droplet.name = "RangeDroplet";
            droplet.transform.SetParent(marker.transform, false);
            droplet.transform.localPosition =
                new Vector3(Mathf.Cos(angle) * radius, 0.10f, Mathf.Sin(angle) * radius);
            droplet.transform.localScale = new Vector3(0.30f, 0.52f, 0.30f);
            Collider collider = droplet.GetComponent<Collider>();
            if (collider != null) Destroy(collider);
            Renderer renderer = droplet.GetComponent<Renderer>();
            if (renderer != null) renderer.sharedMaterial = waterMaterial;
        }
    }

    void BuildVisibleSpringJet()
    {
        Material waterMaterial = CreateRangeMaterial();

        GameObject jet = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        jet.name = "VisibleSpringJet";
        jet.transform.SetParent(springFxRoot, false);
        jet.transform.localPosition = new Vector3(0f, 1.8f, 0f);
        jet.transform.localScale = new Vector3(0.34f, 1.8f, 0.34f);
        Collider jetCollider = jet.GetComponent<Collider>();
        if (jetCollider != null) Destroy(jetCollider);
        Renderer jetRenderer = jet.GetComponent<Renderer>();
        if (jetRenderer != null) jetRenderer.sharedMaterial = waterMaterial;

        GameObject crown = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        crown.name = "SpringSplashCrown";
        crown.transform.SetParent(springFxRoot, false);
        crown.transform.localPosition = new Vector3(0f, 3.65f, 0f);
        crown.transform.localScale = new Vector3(1.15f, 0.28f, 1.15f);
        Collider crownCollider = crown.GetComponent<Collider>();
        if (crownCollider != null) Destroy(crownCollider);
        Renderer crownRenderer = crown.GetComponent<Renderer>();
        if (crownRenderer != null) crownRenderer.sharedMaterial = waterMaterial;
    }

    static Material CreateRangeMaterial()
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit")
                        ?? Shader.Find("Sprites/Default")
                        ?? Shader.Find("Unlit/Color");
        Material material = new Material(shader);
        Color colour = new Color(0.08f, 0.72f, 1f, 0.95f);
        material.color = colour;
        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", colour);
        if (material.HasProperty("_Color")) material.SetColor("_Color", colour);
        return material;
    }
}
