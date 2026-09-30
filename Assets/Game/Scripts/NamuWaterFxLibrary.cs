using UnityEngine;

/// <summary>
/// Runtime loads for NamuFX Stylized Water Effect Pack (Resources/WaterFx).
/// </summary>
public static class NamuWaterFxLibrary
{
    const string Folder = "WaterFx/";

    public static bool IsAvailable => LoadPrefab("Bubbles_Vertical_Loop") != null;

    public static GameObject LoadPrefab(string nameWithoutExtension)
    {
        return Resources.Load<GameObject>(Folder + nameWithoutExtension);
    }

    public static Material LoadTrailMaterial()
    {
        return Resources.Load<Material>(Folder + "WaterTrail");
    }

    public static Material LoadGreenWaterMaterial()
    {
        return Resources.Load<Material>(Folder + "GreenWater");
    }

    public static GameObject Spawn(Transform parent, string prefabName, Vector3 localPos, Vector3 localEuler, float uniformScale)
    {
        GameObject prefab = LoadPrefab(prefabName);
        if (prefab == null || parent == null) return null;

        GameObject instance = Object.Instantiate(prefab, parent, false);
        instance.name = prefabName;
        instance.transform.localPosition = localPos;
        instance.transform.localEulerAngles = localEuler;
        instance.transform.localScale = Vector3.one * uniformScale;
        PlayAllParticles(instance.transform, true);
        return instance;
    }

    public static GameObject SpawnOneShotTinted(Transform parent, string prefabName, Vector3 localPos, Vector3 localEuler, float uniformScale, Color tint, float destroyAfterSeconds)
    {
        GameObject prefab = LoadPrefab(prefabName);
        if (prefab == null || parent == null) return null;

        GameObject instance = Object.Instantiate(prefab, parent, false);
        instance.name = prefabName + "_Splash";
        instance.transform.localPosition = localPos;
        instance.transform.localEulerAngles = localEuler;
        instance.transform.localScale = Vector3.one * uniformScale;
        TintParticleSystems(instance, tint);
        PlayAllParticles(instance.transform, false);

        if (destroyAfterSeconds > 0f)
            Object.Destroy(instance, destroyAfterSeconds);

        return instance;
    }

    public static void TintParticleSystems(GameObject root, Color tint)
    {
        if (root == null) return;

        ParticleSystemRenderer[] renderers = root.GetComponentsInChildren<ParticleSystemRenderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            ParticleSystemRenderer r = renderers[i];
            if (r == null || r.sharedMaterial == null) continue;

            Material mat = new Material(r.sharedMaterial);
            Color hdr = new Color(tint.r * 1.35f, tint.g * 1.35f, tint.b * 1.35f, tint.a);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", hdr);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", hdr);
            r.material = mat;
        }
    }

    public static void SpawnSimpleSplash(Transform parent, Vector3 localPos, Color tint)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = "HydrationSplashFallback";
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        go.transform.localScale = Vector3.one * 0.35f;
        Collider col = go.GetComponent<Collider>();
        if (col != null) Object.Destroy(col);
        Renderer r = go.GetComponent<Renderer>();
        if (r != null) r.material.color = tint;
        Object.Destroy(go, 0.35f);
    }

    public static void PlayAllParticles(Transform root, bool forceLoop)
    {
        if (root == null) return;

        ParticleSystem[] systems = root.GetComponentsInChildren<ParticleSystem>(true);
        for (int i = 0; i < systems.Length; i++)
        {
            ParticleSystem ps = systems[i];
            if (ps == null) continue;
            var main = ps.main;
            if (forceLoop && main.duration > 0f && !main.loop)
            {
                main.loop = true;
            }

            ps.gameObject.SetActive(true);
            ps.Play(true);
        }
    }
}
