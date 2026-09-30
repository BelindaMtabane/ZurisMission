using UnityEngine;

/// <summary>
/// Green NamuFX water at hydration cacti; splashes when the player collects water.
/// </summary>
public class CactusHydrationFx : MonoBehaviour
{
    static readonly Color HydrationGreen = new Color(0.38f, 0.96f, 0.48f, 1f);
    const float UnityPlaneSize = 10f;

    bool idleBuilt;

    void Start()
    {
        BuildIdleVisuals();
    }

    public void PlayHydrationSplash()
    {
        BuildIdleVisuals();

        float s = 0.72f;
        Vector3 splash = new Vector3(0f, RunnerVisualScale.F(0.1f), RunnerVisualScale.F(0.42f));
        if (NamuWaterFxLibrary.IsAvailable)
        {
            NamuWaterFxLibrary.SpawnOneShotTinted(transform, "Water_Splash_A", splash, new Vector3(-28f, 0f, 0f), s, HydrationGreen, 2.4f);
            NamuWaterFxLibrary.SpawnOneShotTinted(transform, "Hit_01", splash + Vector3.up * RunnerVisualScale.F(0.18f), Vector3.zero, s * 1.15f, HydrationGreen, 1.9f);
            NamuWaterFxLibrary.SpawnOneShotTinted(transform, "Bubbles_Burst", splash + Vector3.up * 0.05f, Vector3.zero, s * 0.95f, HydrationGreen, 2f);
            NamuWaterFxLibrary.SpawnOneShotTinted(transform, "Water_Impact", splash + new Vector3(0f, 0f, RunnerVisualScale.F(0.28f)), new Vector3(-90f, 0f, 0f), s * 0.9f, HydrationGreen, 2.2f);
            return;
        }

        NamuWaterFxLibrary.SpawnSimpleSplash(transform, splash, HydrationGreen);
    }

    void BuildIdleVisuals()
    {
        if (idleBuilt || transform.Find("HydrationWater") != null)
        {
            idleBuilt = true;
            return;
        }

        idleBuilt = true;
        GameObject root = new GameObject("HydrationWater");
        root.transform.SetParent(transform, false);
        root.transform.localPosition = Vector3.zero;

        float padW = RunnerVisualScale.F(1.35f);
        float padD = RunnerVisualScale.F(1.05f);
        AddGreenPuddle(root.transform, new Vector3(0f, RunnerVisualScale.F(0.015f), RunnerVisualScale.F(0.15f)), padW, padD);

        if (NamuWaterFxLibrary.IsAvailable)
        {
            Vector3 spring = new Vector3(0f, RunnerVisualScale.F(0.06f), RunnerVisualScale.F(0.12f));
            GameObject bubbles = NamuWaterFxLibrary.Spawn(root.transform, "Bubbles_Vertical_Loop", spring, Vector3.zero, 0.42f);
            NamuWaterFxLibrary.TintParticleSystems(bubbles, HydrationGreen);
        }
    }

    static void AddGreenPuddle(Transform parent, Vector3 localPos, float widthX, float depthZ)
    {
        GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        plane.name = "GreenPuddle";
        plane.transform.SetParent(parent, false);
        plane.transform.localPosition = localPos;
        plane.transform.localRotation = Quaternion.identity;
        plane.transform.localScale = new Vector3(widthX / UnityPlaneSize, 1f, depthZ / UnityPlaneSize);

        Collider col = plane.GetComponent<Collider>();
        if (col != null) col.enabled = false;

        Renderer rend = plane.GetComponent<Renderer>();
        if (rend == null) return;

        Material green = NamuWaterFxLibrary.LoadGreenWaterMaterial();
        if (green != null)
        {
            Material instance = new Material(green);
            if (instance.HasProperty("_Color")) instance.SetColor("_Color", new Color(0.55f, 1f, 0.62f, 0.88f));
            rend.sharedMaterial = instance;
        }
        else
            rend.material.color = new Color(0.25f, 0.82f, 0.35f, 0.75f);

        rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        rend.receiveShadows = false;
    }
}
