using UnityEngine;

/// <summary>
/// Per-level roadside greenery: L1 dry, L2 medium, L3 lush.
/// </summary>
public static class RunnerFoliageProfile
{
    public readonly struct Settings
    {
        public readonly float RoadCactusEnd;
        public readonly float RoadRockEnd;
        public readonly float RoadBushEnd;
        public readonly float RoadTreeEnd;

        public readonly float ShoulderGrassPatchEnd;
        public readonly float ShoulderBushEnd;
        public readonly float ShoulderRockEnd;
        public readonly float ShoulderSingleGrassEnd;

        public readonly float SideTreeCountScale;
        public readonly float GrassPatchCountScale;
        public readonly float DamGrassCountScale;
        public readonly int MaxGrassBladesPerPatch;
        public readonly float ThornBushBias;
        public readonly Color PlantTint;

        public Settings(
            float roadCactusEnd, float roadRockEnd, float roadBushEnd, float roadTreeEnd,
            float shoulderGrassPatchEnd, float shoulderBushEnd, float shoulderRockEnd, float shoulderSingleGrassEnd,
            float sideTreeCountScale, float grassPatchCountScale, float damGrassCountScale,
            int maxGrassBladesPerPatch, float thornBushBias, Color plantTint)
        {
            RoadCactusEnd = roadCactusEnd;
            RoadRockEnd = roadRockEnd;
            RoadBushEnd = roadBushEnd;
            RoadTreeEnd = roadTreeEnd;
            ShoulderGrassPatchEnd = shoulderGrassPatchEnd;
            ShoulderBushEnd = shoulderBushEnd;
            ShoulderRockEnd = shoulderRockEnd;
            ShoulderSingleGrassEnd = shoulderSingleGrassEnd;
            SideTreeCountScale = sideTreeCountScale;
            GrassPatchCountScale = grassPatchCountScale;
            DamGrassCountScale = damGrassCountScale;
            MaxGrassBladesPerPatch = maxGrassBladesPerPatch;
            ThornBushBias = thornBushBias;
            PlantTint = plantTint;
        }
    }

    static readonly Settings Dry = new(
        roadCactusEnd: 0.52f, roadRockEnd: 0.78f, roadBushEnd: 0.84f, roadTreeEnd: 0.87f,
        shoulderGrassPatchEnd: 0.18f, shoulderBushEnd: 0.34f, shoulderRockEnd: 0.74f, shoulderSingleGrassEnd: 0.79f,
        sideTreeCountScale: 0.35f, grassPatchCountScale: 0.22f, damGrassCountScale: 0.30f,
        maxGrassBladesPerPatch: 2, thornBushBias: 0.82f,
        plantTint: new Color(1.12f, 0.68f, 0.54f));

    static readonly Settings Medium = new(
        roadCactusEnd: 0.24f, roadRockEnd: 0.44f, roadBushEnd: 0.68f, roadTreeEnd: 0.90f,
        shoulderGrassPatchEnd: 0.50f, shoulderBushEnd: 0.76f, shoulderRockEnd: 0.86f, shoulderSingleGrassEnd: 0.96f,
        sideTreeCountScale: 1.05f, grassPatchCountScale: 1.08f, damGrassCountScale: 1.12f,
        maxGrassBladesPerPatch: 5, thornBushBias: 0.28f,
        plantTint: new Color(0.94f, 1.06f, 0.86f));

    static readonly Settings Lush = new(
        roadCactusEnd: 0.08f, roadRockEnd: 0.20f, roadBushEnd: 0.60f, roadTreeEnd: 0.94f,
        shoulderGrassPatchEnd: 0.62f, shoulderBushEnd: 0.86f, shoulderRockEnd: 0.91f, shoulderSingleGrassEnd: 0.98f,
        sideTreeCountScale: 1.45f, grassPatchCountScale: 1.65f, damGrassCountScale: 1.72f,
        maxGrassBladesPerPatch: 7, thornBushBias: 0.08f,
        plantTint: new Color(0.78f, 1.22f, 0.86f));

    public static Settings Current
    {
        get
        {
            string scene = SceneCatalog.ActiveName;
            if (SceneCatalog.IsLevel3(scene)) return Lush;
            if (SceneCatalog.IsLevel2(scene)) return Medium;
            return Dry;
        }
    }

    public static void ApplyPlantTint(GameObject go)
    {
        if (go == null) return;

        Color tint = Current.PlantTint;
        if (Mathf.Approximately(tint.r, 1f) && Mathf.Approximately(tint.g, 1f) && Mathf.Approximately(tint.b, 1f))
            return;

        Renderer[] renderers = go.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer rend = renderers[i];
            if (rend == null) continue;

            Material[] mats = rend.materials;
            for (int m = 0; m < mats.Length; m++)
            {
                Material mat = mats[m];
                if (mat == null) continue;

                if (mat.HasProperty("_BaseColor"))
                {
                    Color c = mat.GetColor("_BaseColor");
                    c.r *= tint.r;
                    c.g *= tint.g;
                    c.b *= tint.b;
                    mat.SetColor("_BaseColor", c);
                }
                else if (mat.HasProperty("_Color"))
                {
                    Color c = mat.GetColor("_Color");
                    c.r *= tint.r;
                    c.g *= tint.g;
                    c.b *= tint.b;
                    mat.SetColor("_Color", c);
                }
            }
        }
    }
}
