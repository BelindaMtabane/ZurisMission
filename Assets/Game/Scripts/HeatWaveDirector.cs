using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// MainGame heat waves: duration 1s, then 2s, 3s, 4s, then 5s and stay at 5s.
/// Each session drains 10 health and 10 player water.
/// </summary>
public class HeatWaveDirector : MonoBehaviour
{
    Image flash;
    int waveLength = 1;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        // Replaced by Level1LayoutDirector heat-wave timer.
    }

    void Start()
    {
        enabled = false;
    }

    System.Collections.IEnumerator Loop()
    {
        yield return new WaitForSeconds(3f);
        while (true)
        {
            if (RunStateManager.Instance != null && !RunStateManager.Instance.IsPlaying)
            {
                yield return null;
                continue;
            }

            yield return StartCoroutine(RunSession(waveLength));
            if (waveLength < 5)
            {
                waveLength++;
            }

            yield return new WaitForSeconds(4f);
        }
    }

    System.Collections.IEnumerator RunSession(int seconds)
    {
        HUDControls hud = FindFirstObjectByType<HUDControls>();
        hud?.ApplyHeatWaveSession();
        Debug.Log($"[HeatWave] Session {seconds}s — health -10, water -10");

        float t = 0f;
        while (t < seconds)
        {
            if (RunStateManager.Instance != null && !RunStateManager.Instance.IsPlaying)
            {
                yield break;
            }

            t += Time.deltaTime;
            if (flash != null)
            {
                float pulse = 0.25f + Mathf.Sin(t * 8f) * 0.12f;
                flash.color = new Color(1f, 0.28f, 0.04f, pulse);
            }
            yield return null;
        }

        if (flash != null)
        {
            flash.color = new Color(1f, 0.28f, 0.04f, 0f);
        }
    }

    void CreateFlash()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;
        GameObject go = new GameObject("HeatWaveFlash");
        go.transform.SetParent(canvas.transform, false);
        flash = go.AddComponent<Image>();
        flash.color = new Color(1f, 0.28f, 0.04f, 0f);
        flash.raycastTarget = false;
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
}
