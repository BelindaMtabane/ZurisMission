using UnityEngine;
using System.Collections;

/// <summary>
/// Down-pointing arrow (↓) that appears at the obstacle's lane when the
/// Release Obstacle button is pressed. Pulses with colour glow then fades.
/// damageLevel 0 = bright orange  |  1 = vivid red
/// </summary>
public class LaneWarningEffect : MonoBehaviour
{
    // ── Public entry point ─────────────────────────────────────────────────
    public static void Spawn(float laneX, float laneY, float spawnZ, float damageLevel)
    {
        // Arrow dimensions
        const float shaftH  = 3.0f;    // shaft height
        const float shaftW  = 0.22f;   // shaft width
        const float wingLen = 1.4f;    // each wing length
        const float wingW   = 0.22f;   // wing thickness
        const float partD   = 0.14f;   // Z depth of every part

        float tipY    = laneY + 0.4f;          // arrow tip sits just above spawn height
        float arrowZ  = spawnZ + 0.35f;        // slightly behind the obstacle

        // Root sits at the shaft centre — light lives here
        GameObject root = new GameObject("LaneWarningArrow");
        root.transform.position = new Vector3(laneX, tipY + shaftH * 0.5f, arrowZ);

        // ── Shaft (vertical bar) ───────────────────────────────────────────
        GameObject shaft = MakePart(root.transform);
        shaft.transform.localPosition = Vector3.zero;
        shaft.transform.localScale    = new Vector3(shaftW, shaftH, partD);

        // ── Arrowhead wings ────────────────────────────────────────────────
        // Wings are cubes rotated ±45° so their height axis points up-left / up-right.
        // Their centres are placed so their inner ends meet the tip of the shaft.
        //   tipLocalY   = where the shaft tip is in root-local space
        //   wingOffset  = how far each wing centre is from the tip (along each axis)
        float tipLocalY   = -shaftH * 0.5f;
        float wingOffset  = wingLen * 0.5f * 0.7071f;   // = wingLen/2 * sin45

        GameObject lWing = MakePart(root.transform);
        lWing.transform.localPosition = new Vector3(-wingOffset, tipLocalY + wingOffset, 0f);
        lWing.transform.localScale    = new Vector3(wingW, wingLen, partD);
        lWing.transform.localRotation = Quaternion.Euler(0f, 0f,  45f);

        GameObject rWing = MakePart(root.transform);
        rWing.transform.localPosition = new Vector3( wingOffset, tipLocalY + wingOffset, 0f);
        rWing.transform.localScale    = new Vector3(wingW, wingLen, partD);
        rWing.transform.localRotation = Quaternion.Euler(0f, 0f, -45f);

        // ── Base colour (orange → red) ─────────────────────────────────────
        Color baseColor = Color.Lerp(
            new Color(1f, 0.55f, 0f),   // bright orange
            new Color(1f, 0.05f, 0f),   // vivid red
            damageLevel
        );

        foreach (MeshRenderer mr in root.GetComponentsInChildren<MeshRenderer>())
        {
            Material mat = mr.material;
            mat.color = baseColor;
            if (mat.HasProperty("_EmissionColor"))
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", baseColor * 3f);   // HDR → triggers bloom
            }
        }

        // ── Point light ────────────────────────────────────────────────────
        Light glow      = root.AddComponent<Light>();
        glow.type       = LightType.Point;
        glow.color      = baseColor;
        glow.intensity  = 5f;
        glow.range      = 7f;
        glow.renderMode = LightRenderMode.ForcePixel;

        // ── Start animation ────────────────────────────────────────────────
        LaneWarningEffect fx = root.AddComponent<LaneWarningEffect>();
        fx.Init(baseColor, glow, 2.5f);
    }

    // ── Helpers ────────────────────────────────────────────────────────────
    private static GameObject MakePart(Transform parent)
    {
        GameObject part = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Object.Destroy(part.GetComponent<Collider>());
        part.transform.SetParent(parent, false);   // keep local transform clean
        return part;
    }

    private void Init(Color baseColor, Light glow, float duration)
    {
        StartCoroutine(PulseAndFade(baseColor, glow, duration));
    }

    // ── Animation loop ─────────────────────────────────────────────────────
    private IEnumerator PulseAndFade(Color baseColor, Light glow, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float fade  = 1f - (elapsed / duration);               // 1 → 0
            float pulse = 0.65f + 0.35f * Mathf.Sin(elapsed * 9f); // fast strong flicker

            Color current = baseColor * (pulse * fade);

            foreach (MeshRenderer mr in GetComponentsInChildren<MeshRenderer>())
            {
                Material mat = mr.material;
                mat.color = current;
                if (mat.HasProperty("_EmissionColor"))
                    mat.SetColor("_EmissionColor", current * 3f);
            }

            if (glow != null)
                glow.intensity = 5f * pulse * fade;

            yield return null;
        }

        Destroy(gameObject);
    }
}
