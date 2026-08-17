using UnityEngine;

public class Level3PipeRepair : MonoBehaviour
{
    public static Level3PipeRepair Instance { get; private set; }

    [SerializeField] int tank1Cost = 5;
    [SerializeField] int tank2Cost = 10;
    [SerializeField] int tank3Cost = 2;

    readonly bool[] repaired = new bool[3];
    readonly GameObject[] tankVisuals = new GameObject[3];
    readonly GameObject[] flowVisuals = new GameObject[3];

    public static bool AllTanksRepaired =>
        Instance != null && Instance.repaired[0] && Instance.repaired[1] && Instance.repaired[2];

    public void ApplyCosts(int first, int second, int third)
    {
        tank1Cost = first;
        tank2Cost = second;
        tank3Cost = third;
    }
    {
        if (tankIndex == 1) return tank2Cost;
        if (tankIndex == 2) return tank3Cost;
        return tank1Cost;
    }

    public void BindTank(int tankIndex, GameObject tankRoot, GameObject flowRoot)
    {
        if (tankIndex < 0 || tankIndex > 2) return;
        tankVisuals[tankIndex] = tankRoot;
        flowVisuals[tankIndex] = flowRoot;
        if (flowRoot != null) flowRoot.SetActive(false);
    }

    void Awake()
    {
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public bool TryRepair(int tankIndex)
    {
        if (tankIndex < 0 || tankIndex > 2) return false;
        if (repaired[tankIndex])
        {
            Level3FeedbackUI.Show("ALREADY REPAIRED", new Color(0.7f, 0.85f, 0.4f), 1f);
            return true;
        }

        HUDControls hud = FindFirstObjectByType<HUDControls>();
        if (hud == null) return false;

        int cost = CostFor(tankIndex);
        if (hud.MaterialLevel < cost)
        {
            Level3FeedbackUI.Show($"NEED {cost} MATERIALS", new Color(1f, 0.45f, 0.2f), 1.6f);
            return false;
        }

        hud.BreakMaterials(cost);
        repaired[tankIndex] = true;

        if (flowVisuals[tankIndex] != null) flowVisuals[tankIndex].SetActive(true);
        if (tankVisuals[tankIndex] != null)
        {
            Renderer[] rends = tankVisuals[tankIndex].GetComponentsInChildren<Renderer>();
            for (int i = 0; i < rends.Length; i++)
            {
                if (rends[i] != null) rends[i].material.color = Color.Lerp(rends[i].material.color, new Color(0.25f, 0.7f, 1f), 0.55f);
            }
        }

        string flow = tankIndex == 0 ? "SMALL WATER FLOW" : tankIndex == 1 ? "STRONG WATER FLOW" : "FULL WATER SYSTEM";
        Level3FeedbackUI.Show($"TANK {tankIndex + 1} REPAIRED! {flow}", new Color(0.35f, 0.85f, 1f), 2f);
        return true;
    }
}

public class Level3RepairPoint : MonoBehaviour
{
    [SerializeField] int tankIndex;
    bool used;

    public void Setup(int tank)
    {
        tankIndex = tank;
    }

    void OnTriggerEnter(Collider other)
    {
        if (used || !other.CompareTag("Player")) return;
        if (RunStateManager.Instance != null && !RunStateManager.Instance.IsPlaying) return;
        if (Level3PipeRepair.Instance == null) return;

        if (Level3PipeRepair.Instance.TryRepair(tankIndex))
        {
            used = true;
            Renderer[] rends = GetComponentsInChildren<Renderer>();
            for (int i = 0; i < rends.Length; i++)
            {
                if (rends[i] != null) rends[i].material.color = new Color(0.2f, 0.85f, 0.45f);
            }
        }
    }
}
