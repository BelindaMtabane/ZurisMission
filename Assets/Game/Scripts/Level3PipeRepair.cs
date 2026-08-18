using UnityEngine;

public class Level3PipeRepair : MonoBehaviour
{
    public static Level3PipeRepair Instance { get; private set; }

    readonly int[] progress = new int[3];
    readonly GameObject[] tankVisuals = new GameObject[3];
    readonly GameObject[] flowVisuals = new GameObject[3];
    readonly GameObject[] fillVisuals = new GameObject[3];

    public static bool AllTanksRepaired =>
        Instance != null && Instance.progress[0] >= 100 && Instance.progress[1] >= 100 && Instance.progress[2] >= 100;

    public int GetProgress(int tankIndex)
    {
        if (tankIndex < 0 || tankIndex > 2) return 0;
        return progress[tankIndex];
    }

    void Awake()
    {
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void BindTank(int tankIndex, GameObject tankRoot, GameObject flowRoot, GameObject fillRoot)
    {
        if (tankIndex < 0 || tankIndex > 2) return;
        tankVisuals[tankIndex] = tankRoot;
        flowVisuals[tankIndex] = flowRoot;
        fillVisuals[tankIndex] = fillRoot;
        if (flowRoot != null) flowRoot.SetActive(false);
        UpdateTankVisual(tankIndex);
    }

    public bool TryRepair(int tankIndex)
    {
        if (tankIndex < 0 || tankIndex > 2) return false;

        if (progress[tankIndex] >= 100)
        {
            Level3FeedbackUI.Show($"TANK {tankIndex + 1} ALREADY FULL", new Color(0.7f, 0.85f, 0.4f), 1f);
            return false;
        }

        HUDControls hud = FindFirstObjectByType<HUDControls>();
        if (hud == null) return false;

        int cost = Level3Config.RepairMaterialCost(tankIndex);
        if (hud.MaterialLevel < cost)
        {
            Level3FeedbackUI.Show($"NEED {cost} MATERIALS", new Color(1f, 0.45f, 0.2f), 1.6f);
            return false;
        }

        hud.BreakMaterials(cost);
        progress[tankIndex] = Mathf.Min(100, progress[tankIndex] + Level3Config.ProgressPerRepair(tankIndex));
        UpdateTankVisual(tankIndex);
        hud.ApplyLevel3TankProgress(progress[0], progress[1], progress[2]);

        string msg = $"TANK {tankIndex + 1}: {progress[tankIndex]}%";
        if (progress[tankIndex] >= 100)
        {
            msg += " — REPAIRED!";
            if (flowVisuals[tankIndex] != null) flowVisuals[tankIndex].SetActive(true);
        }

        int village = Mathf.RoundToInt(HUDControls.Level3VillageFromTanks(progress[0], progress[1], progress[2]));
        msg += $"  |  VILLAGE {village}%";
        Level3FeedbackUI.Show(msg, new Color(1f, 0.88f, 0.15f), 1.8f);
        FlashRepair(tankIndex);
        return true;
    }

    void UpdateTankVisual(int tankIndex)
    {
        float t = progress[tankIndex] / 100f;
        if (fillVisuals[tankIndex] != null)
        {
            fillVisuals[tankIndex].transform.localScale = new Vector3(2f, 1.2f + t * 1.4f, 2f);
        }

        if (tankVisuals[tankIndex] != null)
        {
            Renderer[] rends = tankVisuals[tankIndex].GetComponentsInChildren<Renderer>();
            Color tint = Color.Lerp(new Color(0.45f, 0.48f, 0.52f), new Color(0.25f, 0.7f, 1f), t);
            for (int i = 0; i < rends.Length; i++)
            {
                if (rends[i] != null) rends[i].material.color = tint;
            }
        }
    }

    void FlashRepair(int tankIndex)
    {
        if (tankVisuals[tankIndex] == null) return;
        Renderer[] rends = tankVisuals[tankIndex].GetComponentsInChildren<Renderer>();
        for (int i = 0; i < rends.Length; i++)
        {
            if (rends[i] != null)
            {
                rends[i].material.color = Level3Primitives.YellowRepair;
            }
        }
    }
}

public class Level3RepairPoint : MonoBehaviour
{
    [SerializeField] int tankIndex;
    [SerializeField] bool isRepaired;
    GameObject fxRoot;
    float nextTryTime;

    public void Setup(int tank)
    {
        tankIndex = tank;
        isRepaired = false;
    }

    public void BindFx(GameObject fx)
    {
        fxRoot = fx;
    }

    void OnTriggerEnter(Collider other) => TryHandle(other);

    void OnTriggerStay(Collider other) => TryHandle(other);

    void OnCollisionEnter(Collision collision)
    {
        if (collision != null) TryHandle(collision.collider);
    }

    void TryHandle(Collider other)
    {
        if (isRepaired || other == null) return;
        if (Time.time < nextTryTime) return;
        if (!IsPlayer(other)) return;
        if (RunStateManager.Instance != null && !RunStateManager.Instance.IsPlaying) return;
        if (Level3PipeRepair.Instance == null) return;

        if (Level3PipeRepair.Instance.TryRepair(tankIndex))
        {
            isRepaired = true;
            Collider col = GetComponent<Collider>();
            if (col != null) col.enabled = false;
            PlayRepairFx();
            GameObject paint = fxRoot != null ? fxRoot : gameObject;
            Renderer[] rends = paint.GetComponentsInChildren<Renderer>();
            for (int i = 0; i < rends.Length; i++)
            {
                if (rends[i] != null) rends[i].material.color = new Color(0.2f, 0.85f, 0.45f);
            }
        }
        else
        {
            nextTryTime = Time.time + 1.1f;
        }
    }

    static bool IsPlayer(Collider other)
    {
        if (other.CompareTag("Player")) return true;
        return other.GetComponentInParent<PlayerController>() != null;
    }

    void PlayRepairFx()
    {
        Transform parent = fxRoot != null ? fxRoot.transform : transform;
        for (int i = 0; i < 8; i++)
        {
            GameObject spark = GameObject.CreatePrimitive(PrimitiveType.Cube);
            spark.name = "Spark";
            spark.transform.SetParent(parent, false);
            spark.transform.localPosition = Random.insideUnitSphere * 0.25f;
            spark.transform.localScale = Vector3.one * 0.12f;
            Renderer r = spark.GetComponent<Renderer>();
            if (r != null) r.material.color = Level3Primitives.YellowRepair;
            Collider c = spark.GetComponent<Collider>();
            if (c != null) Destroy(c);
            Destroy(spark, 0.45f);
        }
    }
}
