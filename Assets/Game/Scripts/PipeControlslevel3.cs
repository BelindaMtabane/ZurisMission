using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class PipeControlslevel3 : MonoBehaviour
{
    // ── Inspector-assigned HUD text fields ────────────────────────────────
    public TMP_Text material;
    public TMP_Text healthText;
    public TMP_Text villageProgressText;
    public TMP_Text tank1;
    public TMP_Text tank2;
    public TMP_Text tank3;

    // ── Player ────────────────────────────────────────────────────────────
    private float health = 100f;
    PlayerMovementOG playerMovement;
    private bool hasHit = false;

    // ── Village ───────────────────────────────────────────────────────────
    private float villageLevel;
    private float villageLevelTarget = 96f;

    // ── Tank / pipe progress ──────────────────────────────────────────────
    private int tank1Amount;
    private int tank2Amount;
    private int tank3Amount;

    // ══════════════════════════════════════════════════════════════════════
    // INVENTORY — four material categories
    // ══════════════════════════════════════════════════════════════════════
    public int pipeParts = 10;   // for repairing Pipe Joints
    public int screws    = 10;   // shared fastener for all repairs
    public int tankPaste = 10;   // for sealing Storage Tanks
    public int tapParts  = 10;   // for fixing Water Taps

    // Computed total (for EndLevelDialogue compat + legacy HUD text)
    public int materialLevel => pipeParts + screws + tankPaste + tapParts;

    // Material maximums
    public const int MatMax = 30;

    // ── Repair costs per USE press ─────────────────────────────────────────
    // Pipe Joint:    CostPipeParts + CostScrews
    // Water Tap:     CostTapParts  + CostScrews
    // Storage Tank:  CostTankPaste + CostScrews
    public const int CostPipeParts = 3;
    public const int CostTapParts  = 3;
    public const int CostTankPaste = 3;
    public const int CostScrews    = 1;   // screws consumed by every repair type

    // ── Pipe proximity system ─────────────────────────────────────────────
    public enum PipeZone { None, Pipe1, Pipe2, Pipe3 }
    public PipeZone NearbyPipe { get; private set; } = PipeZone.None;

    // ══════════════════════════════════════════════════════════════════════
    void Start()
    {
        if (playerMovement == null)
            playerMovement = FindFirstObjectByType<PlayerMovementOG>();
        VillageProgress();

        // Push ourselves to InventoryUI so it always has a valid reference,
        // regardless of object-activation state or script-execution order.
        InventoryUI.Instance?.SetPipe(this);
    }

    void Update()
    {
        UpdateUI();
    }

    void UpdateUI()
    {
        material.text          = $"Material: {materialLevel}";
        healthText.text        = $"Health: {health:F0}";
        tank1.text             = $"Tank 1: {tank1Amount}%";
        tank2.text             = $"Tank 2: {tank2Amount}%";
        tank3.text             = $"Tank 3: {tank3Amount}%";
        villageProgressText.text = $"Village Progress: {villageLevel:F0}%";
    }

    // ── Speed ─────────────────────────────────────────────────────────────
    public void SpeedControls(float playerMove)
    {
        playerMovement.playerSpeed = playerMove;
        StopCoroutine("ResetSpeed");
        StartCoroutine(ResetSpeed());
    }

    IEnumerator ResetSpeed()
    {
        yield return new WaitForSeconds(5f);
        playerMovement.playerSpeed = 20f;
    }

    // ── Health ────────────────────────────────────────────────────────────
    public void HealthDecreaseManager()
    {
        float dmg = Random.Range(3f, 15f);
        health = Mathf.Max(health - dmg, 0f);
        Debug.Log($"Health -{dmg:F1}  now {health:F0}");
    }

    public void HealthIncreaseManager()
    {
        if (health < 100f)
        {
            float gain = Random.Range(3f, 15f);
            health = Mathf.Min(health + gain, 100f);
            Debug.Log($"Health +{gain:F1}  now {health:F0}");
        }
    }

    // ── Village progress ──────────────────────────────────────────────────
    void VillageProgress() => villageLevel = 89f;

    public void VillageProgressIncrease()
    {
        float inc = Random.Range(2f, 4f);
        villageLevel = Mathf.Min(villageLevel + inc, villageLevelTarget);
        Debug.Log($"Village +{inc:F1} -> {villageLevel:F1}%");
    }

    public void VillageProgressDecrease()
    {
        float dec = Random.Range(1f, 2f);
        villageLevel = Mathf.Max(villageLevel - dec, 0f);
        Debug.Log($"Village -{dec:F1} -> {villageLevel:F1}%");
    }

    // ── Material collection ───────────────────────────────────────────────
    /// <summary>
    /// Randomly distributes 4-9 material points across the four categories.
    /// </summary>
    public void TankMaterialINC()
    {
        int total = Random.Range(4, 10);
        int gained0 = 0, gained1 = 0, gained2 = 0, gained3 = 0;

        for (int i = 0; i < total; i++)
        {
            switch (Random.Range(0, 4))
            {
                case 0: if (pipeParts < MatMax) { pipeParts++;  gained0++; } break;
                case 1: if (screws    < MatMax) { screws++;     gained1++; } break;
                case 2: if (tankPaste < MatMax) { tankPaste++;  gained2++; } break;
                case 3: if (tapParts  < MatMax) { tapParts++;   gained3++; } break;
            }
        }

        string summary = $"Collected — Pipes:{gained0} Screws:{gained1} Paste:{gained2} Taps:{gained3}";
        GameInfoUI.Post(summary, GameInfoUI.MsgType.Pickup);
        Debug.Log($"TankMaterialINC: {summary}");
    }

    /// <summary>Legacy material drain (deducts from each type evenly).</summary>
    public void TankMaterialDEC()
    {
        pipeParts  = Mathf.Max(0, pipeParts  - 2);
        screws     = Mathf.Max(0, screws     - 2);
        tankPaste  = Mathf.Max(0, tankPaste  - 1);
        tapParts   = Mathf.Max(0, tapParts   - 1);
    }

    // ── Tank progress (called from UseCurrentPipe) ────────────────────────
    public void TankProgressINC1()
    {
        tank1Amount = Mathf.Min(tank1Amount + 25, 100);
        Debug.Log($"Pipe Joint 1: {tank1Amount}%");
    }

    public void TankProgressINC2()
    {
        tank2Amount = Mathf.Min(tank2Amount + 17, 100);
        Debug.Log($"Water Tap: {tank2Amount}%");
    }

    public void TankProgressINC3()
    {
        tank3Amount = Mathf.Min(tank3Amount + 13, 100);
        Debug.Log($"Storage Tank: {tank3Amount}%");
    }

    public void LevelProgress()
    {
        if (villageLevel >= villageLevelTarget)
            Debug.Log("Level 3 Completed — Village at " + villageLevel.ToString("F0") + "%");
        else
            Debug.Log("Level 3 ended — Village at " + villageLevel.ToString("F0") + "%");
    }

    // ── USE button: spend materials to repair the nearby structure ─────────
    /// <summary>
    /// Called by the old-style trigger-only path (uses whatever NearbyPipe is at that moment).
    /// </summary>
    public void UseCurrentPipe() => UseAtZone(NearbyPipe);

    /// <summary>
    /// Called by InventoryUI which latches the zone at approach time and passes it
    /// explicitly — avoids the physics/input timing race where NearbyPipe is already
    /// cleared by OnTriggerExit before the button click event fires.
    /// </summary>
    public void UseAtZone(PipeZone zone)
    {
        if (zone == PipeZone.None) return;

        switch (zone)
        {
            case PipeZone.Pipe1: // Pipe Joint 1
                if (pipeParts < CostPipeParts || screws < CostScrews)
                {
                    GameInfoUI.Post(
                        $"Need {CostPipeParts}x Pipe Parts + {CostScrews}x Screw to repair!",
                        GameInfoUI.MsgType.Obstacle);
                    return;
                }
                pipeParts -= CostPipeParts;
                screws    -= CostScrews;
                TankProgressINC1();
                VillageProgressIncrease();
                GameInfoUI.Post(
                    $"Pipe Joint 1 repaired! Now {tank1Amount}%  |  Pipe Parts left: {pipeParts}",
                    GameInfoUI.MsgType.Win);
                break;

            case PipeZone.Pipe2: // Water Tap
                if (tapParts < CostTapParts || screws < CostScrews)
                {
                    GameInfoUI.Post(
                        $"Need {CostTapParts}x Tap Parts + {CostScrews}x Screw to repair!",
                        GameInfoUI.MsgType.Obstacle);
                    return;
                }
                tapParts -= CostTapParts;
                screws   -= CostScrews;
                TankProgressINC2();
                VillageProgressIncrease();
                GameInfoUI.Post(
                    $"Water Tap repaired! Now {tank2Amount}%  |  Tap Parts left: {tapParts}",
                    GameInfoUI.MsgType.Win);
                break;

            case PipeZone.Pipe3: // Storage Tank
                if (tankPaste < CostTankPaste || screws < CostScrews)
                {
                    GameInfoUI.Post(
                        $"Need {CostTankPaste}x Tank Paste + {CostScrews}x Screw to repair!",
                        GameInfoUI.MsgType.Obstacle);
                    return;
                }
                tankPaste -= CostTankPaste;
                screws    -= CostScrews;
                TankProgressINC3();
                VillageProgressIncrease();
                GameInfoUI.Post(
                    $"Storage Tank sealed! Now {tank3Amount}%  |  Tank Paste left: {tankPaste}",
                    GameInfoUI.MsgType.Win);
                break;
        }
    }

    // ── Read-only accessors ────────────────────────────────────────────────
    public float Health       => health;
    public float VillageLevel => villageLevel;
    public int   Tank1Amount  => tank1Amount;
    public int   Tank2Amount  => tank2Amount;
    public int   Tank3Amount  => tank3Amount;

    // ══════════════════════════════════════════════════════════════════════
    // TRIGGER HANDLING
    // ══════════════════════════════════════════════════════════════════════
    private void OnTriggerEnter(Collider other)
    {
        // ── Pipe proximity zones — no hasHit, just set NearbyPipe ──────────
        if (other.CompareTag("PipeFix1"))
        {
            NearbyPipe = PipeZone.Pipe1;
            GameInfoUI.Post("Near Pipe Joint 1 — open inventory and press USE!", GameInfoUI.MsgType.Interaction);
            return;
        }
        if (other.CompareTag("PipeFix2"))
        {
            NearbyPipe = PipeZone.Pipe2;
            GameInfoUI.Post("Near Water Tap — open inventory and press USE!", GameInfoUI.MsgType.Interaction);
            return;
        }
        if (other.CompareTag("PipeFix3"))
        {
            NearbyPipe = PipeZone.Pipe3;
            GameInfoUI.Post("Near Storage Tank — open inventory and press USE!", GameInfoUI.MsgType.Interaction);
            return;
        }

        // ── All other collisions use hasHit guard ──────────────────────────
        if (hasHit) return;

        if (other.CompareTag("Materials"))
        {
            hasHit = true;
            TankMaterialINC();
            VillageProgressIncrease();
        }
        if (other.CompareTag("FruitPickup"))
        {
            hasHit = true;
            HealthIncreaseManager();
            GameInfoUI.Post("Zuri picked up fruit and feels better!", GameInfoUI.MsgType.Pickup);
        }
        if (other.CompareTag("SpeedBoast"))
        {
            hasHit = true;
            SpeedControls(40f);
            GameInfoUI.Post("Zuri is moving fast! Speed boosted.", GameInfoUI.MsgType.Interaction);
        }
        if (other.CompareTag("SlowDown"))
        {
            hasHit = true;
            SpeedControls(15f);
            GameInfoUI.Post("Zuri has been slowed down!", GameInfoUI.MsgType.Obstacle);
        }
        if (other.CompareTag("Heat&Disease"))
        {
            hasHit = true;
            HealthDecreaseManager();
            VillageProgressDecrease();
            GameInfoUI.Post("Zuri is suffering from heat and disease!", GameInfoUI.MsgType.Obstacle);
        }
        if (other.CompareTag("EndLvl3End"))
        {
            hasHit = true;
            LevelProgress();
            GameInfoUI.Post("Zuri completed the mission! The village is saved!", GameInfoUI.MsgType.Win);
            if (EndLevelDialogue.Instance != null)
                EndLevelDialogue.Instance.ShowForLevel(3);
        }
        if (other.CompareTag("AnimalAttack"))
        {
            hasHit = true;
            HealthDecreaseManager();
            VillageProgressDecrease();
            GameInfoUI.Post("Zuri was attacked by an animal!", GameInfoUI.MsgType.Obstacle);
        }
        if (other.CompareTag("Obstacle"))
        {
            hasHit = true;
            HealthDecreaseManager();
            VillageProgressDecrease();
            GameInfoUI.Post("Zuri hit an obstacle and lost health!", GameInfoUI.MsgType.Obstacle);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("PipeFix1") ||
            other.CompareTag("PipeFix2") ||
            other.CompareTag("PipeFix3"))
        {
            NearbyPipe = PipeZone.None;
        }
        else
        {
            hasHit = false;
        }
    }
}
