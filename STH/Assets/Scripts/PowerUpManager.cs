using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Singleton that manages all power-up and power-down effects.
/// Place on a persistent GameObject in the scene (e.g. GameManager's object or its own).
/// Wire up the UI notification text in the Inspector.
/// </summary>
public class PowerUpManager : MonoBehaviour
{
    public static PowerUpManager Instance { get; private set; }

    // ── UI ────────────────────────────────────────────────────────────
    [Header("UI")]
    [SerializeField] TextMeshProUGUI notificationText;   // centre-screen pop text
    [SerializeField] float notificationDuration = 3f;

    // ── Effect durations (randomised within range) ────────────────────
    [Header("Positive Effect Durations")]
    [SerializeField] float oneShotKillMinDur  = 5f;
    [SerializeField] float oneShotKillMaxDur  = 10f;
    [SerializeField] float invincibilityDur   = 5f;
    [SerializeField] float speedBoostMinDur   = 5f;
    [SerializeField] float speedBoostMaxDur   = 10f;
    [SerializeField] float freezeShotMinDur   = 5f;
    [SerializeField] float freezeShotMaxDur   = 10f;
    [SerializeField] float timeSlowMinDur     = 3f;
    [SerializeField] float timeSlowMaxDur     = 6f;

    [Header("Negative Effect Durations")]
    [SerializeField] float negEffectMinDur    = 5f;
    [SerializeField] float negEffectMaxDur    = 12f;

    // ── Magnitude ranges ──────────────────────────────────────────────
    [Header("Positive Magnitudes")]
    [SerializeField] float speedBoostMultMin  = 1.3f;
    [SerializeField] float speedBoostMultMax  = 1.8f;

    [Header("Negative Magnitudes")]
    [SerializeField] float fireRatePenaltyMin    = 0.05f;  // added to fireRate (higher = slower)
    [SerializeField] float fireRatePenaltyMax    = 0.25f;
    [SerializeField] float moveSpeedPenaltyMin   = 1f;
    [SerializeField] float moveSpeedPenaltyMax   = 3f;
    [SerializeField] float rotSpeedPenaltyMin    = 20f;
    [SerializeField] float rotSpeedPenaltyMax    = 80f;
    [SerializeField] float enemySpeedBoostMin    = 1f;
    [SerializeField] float enemySpeedBoostMax    = 3f;
    [SerializeField] float viewAnglePenaltyMin   = 5f;
    [SerializeField] float viewAnglePenaltyMax   = 15f;

    // ── State tracking ────────────────────────────────────────────────
    // Public booleans so other scripts (Health, EnemyController) can query them
    [HideInInspector] public bool oneShotKillActive   = false;
    [HideInInspector] public bool invincibleActive    = false;
    [HideInInspector] public bool freezeShotActive    = false;

    // Active coroutine handles so we can cancel/replace
    Coroutine notifCoroutine;
    readonly List<Coroutine> activeEffects = new List<Coroutine>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (notificationText != null)
            notificationText.gameObject.SetActive(false);
    }

    // ─────────────────────────────────────────────────────────────────
    //  PUBLIC API
    // ─────────────────────────────────────────────────────────────────

    public void ApplyRandomPositiveEffect(PlayerController pc)
    {
        int roll = Random.Range(0, 5);
        switch (roll)
        {
            case 0: StartTracked(OneShotKill(pc));  break;
            case 1: StartTracked(Invincibility(pc)); break;
            case 2: StartTracked(SpeedBoost(pc));   break;
            case 3: StartTracked(FreezeShot());     break;
            case 4: StartTracked(TimeSlow());       break;
        }
    }

    public void ApplyRandomNegativeEffect(PlayerController pc)
    {
        int roll = Random.Range(0, 5);
        switch (roll)
        {
            case 0: StartTracked(FireRateDecrease(pc));   break;
            case 1: StartTracked(MoveSpeedDecrease(pc));  break;
            case 2: StartTracked(RotationDecrease(pc));   break;
            case 3: StartTracked(EnemySpeedIncrease());   break;
            case 4: StartTracked(ViewAngleDecrease(pc));  break;
        }
    }

    // ─────────────────────────────────────────────────────────────────
    //  POSITIVE EFFECTS
    // ─────────────────────────────────────────────────────────────────

    IEnumerator OneShotKill(PlayerController pc)
    {
        float dur = Random.Range(oneShotKillMinDur, oneShotKillMaxDur);
        ShowNotification("⚡ ONE SHOT KILL! (" + Mathf.RoundToInt(dur) + "s)", Color.green);
        oneShotKillActive = true;
        yield return new WaitForSecondsRealtime(dur);
        oneShotKillActive = false;
        ShowNotification("One Shot Kill ended", Color.white);
    }

    IEnumerator Invincibility(PlayerController pc)
    {
        ShowNotification("🛡 INVINCIBLE! (" + invincibilityDur + "s)", Color.cyan);
        invincibleActive = true;
        yield return new WaitForSecondsRealtime(invincibilityDur);
        invincibleActive = false;
        ShowNotification("Invincibility ended", Color.white);
    }

    IEnumerator SpeedBoost(PlayerController pc)
    {
        float mult = Random.Range(speedBoostMultMin, speedBoostMultMax);
        float dur  = Random.Range(speedBoostMinDur, speedBoostMaxDur);
        ShowNotification("🚀 SPEED BOOST x" + Mathf.Round(mult * 100f) / 100f + " (" + Mathf.RoundToInt(dur) + "s)", Color.green);

        float original = pc.GetMoveSpeed;
        pc.GetMoveSpeed *= mult;
        yield return new WaitForSecondsRealtime(dur);
        pc.GetMoveSpeed = original;
        ShowNotification("Speed Boost ended", Color.white);
    }

    IEnumerator FreezeShot()
    {
        float dur = Random.Range(freezeShotMinDur, freezeShotMaxDur);
        ShowNotification("❄ FREEZE SHOT! (" + Mathf.RoundToInt(dur) + "s)", Color.cyan);
        freezeShotActive = true;
        yield return new WaitForSecondsRealtime(dur);
        freezeShotActive = false;
        ShowNotification("Freeze Shot ended", Color.white);
    }

    IEnumerator TimeSlow()
    {
        float dur = Random.Range(timeSlowMinDur, timeSlowMaxDur);
        ShowNotification("🕐 TIME SLOW! (" + Mathf.RoundToInt(dur) + "s)", Color.yellow);
        Time.timeScale = 0.5f;
        yield return new WaitForSecondsRealtime(dur);
        Time.timeScale = 1f;
        ShowNotification("Time Slow ended", Color.white);
    }

    // ─────────────────────────────────────────────────────────────────
    //  NEGATIVE EFFECTS
    // ─────────────────────────────────────────────────────────────────

    IEnumerator FireRateDecrease(PlayerController pc)
    {
        float penalty = Random.Range(fireRatePenaltyMin, fireRatePenaltyMax);
        float dur = Random.Range(negEffectMinDur, negEffectMaxDur);
        ShowNotification("⬇ Fire Rate decreased (" + Mathf.RoundToInt(dur) + "s)", Color.red);
        pc.GetFireRate += penalty;
        yield return new WaitForSecondsRealtime(dur);
        pc.GetFireRate -= penalty;
        ShowNotification("Fire Rate restored", Color.white);
    }

    IEnumerator MoveSpeedDecrease(PlayerController pc)
    {
        float penalty = Random.Range(moveSpeedPenaltyMin, moveSpeedPenaltyMax);
        float dur = Random.Range(negEffectMinDur, negEffectMaxDur);
        ShowNotification("⬇ Move Speed decreased (" + Mathf.RoundToInt(dur) + "s)", Color.red);
        pc.GetMoveSpeed -= penalty;
        yield return new WaitForSecondsRealtime(dur);
        pc.GetMoveSpeed += penalty;
        ShowNotification("Move Speed restored", Color.white);
    }

    IEnumerator RotationDecrease(PlayerController pc)
    {
        float penalty = Random.Range(rotSpeedPenaltyMin, rotSpeedPenaltyMax);
        float dur = Random.Range(negEffectMinDur, negEffectMaxDur);
        ShowNotification("⬇ Rotation Speed decreased (" + Mathf.RoundToInt(dur) + "s)", Color.red);
        pc.GetRotationSpeed -= penalty;
        yield return new WaitForSecondsRealtime(dur);
        pc.GetRotationSpeed += penalty;
        ShowNotification("Rotation Speed restored", Color.white);
    }

    IEnumerator EnemySpeedIncrease()
    {
        float boost = Random.Range(enemySpeedBoostMin, enemySpeedBoostMax);
        float dur   = Random.Range(negEffectMinDur, negEffectMaxDur);
        ShowNotification("💀 Enemy Speed INCREASED (" + Mathf.RoundToInt(dur) + "s)", Color.red);

        // Apply to all currently active enemies
        EnemyController[] enemies = FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
        foreach (EnemyController e in enemies)
            e.BoostMoveSpeed(boost);

        yield return new WaitForSecondsRealtime(dur);

        enemies = FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
        foreach (EnemyController e in enemies)
            e.BoostMoveSpeed(-boost);

        ShowNotification("Enemy Speed restored", Color.white);
    }

    IEnumerator ViewAngleDecrease(PlayerController pc)
    {
        float penalty = Random.Range(viewAnglePenaltyMin, viewAnglePenaltyMax);
        float dur = Random.Range(negEffectMinDur, negEffectMaxDur);
        ShowNotification("⬇ View Range decreased (" + Mathf.RoundToInt(dur) + "s)", Color.red);
        pc.GetCameraViewDistance -= penalty;
        yield return new WaitForSecondsRealtime(dur);
        pc.GetCameraViewDistance += penalty;
        ShowNotification("View Range restored", Color.white);
    }

    // ─────────────────────────────────────────────────────────────────
    //  HELPERS
    // ─────────────────────────────────────────────────────────────────

    void StartTracked(IEnumerator routine)
    {
        activeEffects.Add(StartCoroutine(routine));
    }

    void ShowNotification(string message, Color color)
    {
        if (notificationText == null) return;

        if (notifCoroutine != null) StopCoroutine(notifCoroutine);
        notifCoroutine = StartCoroutine(ShowNotifRoutine(message, color));
    }

    IEnumerator ShowNotifRoutine(string message, Color color)
    {
        notificationText.text = message;
        notificationText.color = color;
        notificationText.gameObject.SetActive(true);

        yield return new WaitForSecondsRealtime(notificationDuration);

        notificationText.gameObject.SetActive(false);
    }
}
