using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Level 1 has three respawn chances for dehydration or empty health.
/// Each respawn returns the player to the start of the current beat.
/// The fourth failure is game over.
/// </summary>
public class Level1Respawn : MonoBehaviour
{
    public const int MaxChances = 3;
    public static int ChancesLeft => chancesLeft;

    static readonly float[] BeatStarts = { 0f, 0.12f, 0.25f, 0.52f, 0.75f };

    static int chancesLeft = MaxChances;
    static float ignoreUntil;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        SceneManager.sceneLoaded += (_, __) => ResetChances();
        ResetChances();
    }

    static void ResetChances()
    {
        chancesLeft = MaxChances;
        ignoreUntil = 0f;
    }

    public static bool TryRespawn(PlayerResources resources, string reason)
    {
        if (!SceneCatalog.IsLevel1(SceneCatalog.ActiveName)) return false;
        if (Time.time < ignoreUntil) return true;
        if (chancesLeft <= 0) return false;

        chancesLeft--;
        ignoreUntil = Time.time + 1.5f;

        if (resources != null)
        {
            if (resources.Health < 40f) resources.SetHealth(40f);
            if (resources.PlayerWater < 40f) resources.SetPlayerWater(40f);
        }

        PlayerController player = Object.FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            float progress = Level1Progress.Normalized(player.transform.position.z);
            float beat = 0f;
            for (int i = 0; i < BeatStarts.Length; i++)
            {
                if (progress + 0.001f >= BeatStarts[i]) beat = BeatStarts[i];
            }

            //player.PlaceOnPath(Level1Progress.WorldZ(beat));
            player.SetInputLocked(false);
        }

        string left = chancesLeft == 1 ? "1 chance left" : $"{chancesLeft} chances left";
        Level1FeedbackUI.Show($"{reason}\nRespawned. {left}", new Color(1f, 0.72f, 0.25f), 2.4f);
        return true;
    }
}
