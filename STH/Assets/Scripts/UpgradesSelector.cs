using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// ─────────────────────────────────────────────
//  Weapon definition — configure in Inspector
// ─────────────────────────────────────────────
[System.Serializable]
public class WeaponData
{
    public string weaponName;
    public UpgradesSelector.WeaponType weaponType;
    public float price;
    [HideInInspector] public bool purchased;
    [HideInInspector] public Button button;
    [HideInInspector] public TextMeshProUGUI priceText;
}

// ─────────────────────────────────────────────
//  Bot definition — configure in Inspector
// ─────────────────────────────────────────────
[System.Serializable]
public class BotData
{
    public string botName;
    public UpgradesSelector.BotType botType;
    public float price;
    [HideInInspector] public bool purchased;
    [HideInInspector] public Button button;
    [HideInInspector] public TextMeshProUGUI priceText;
}

public class UpgradesSelector : MonoBehaviour
{
    // ── Stat ranges ───────────────────────────────────────────────────
    float enemyDetectionRangeToGive;
    [SerializeField] float enemyDetectionRangeToGiveMin, enemyDetectionRangeToGiveMax, enemyDetectionRangeToGiveCap;
    float viewRangeToGive;
    [SerializeField] float viewRangeToGiveMin, viewRangeToGiveMax, viewRangeToGiveCap;
    float moveSpeedToGive;
    [SerializeField] float moveSpeedToGiveMin, moveSpeedToGiveMax, moveSpeedToGiveCap;
    float sprintTimeToGive;
    [SerializeField] float sprintTimeToGiveMin, sprintTimeToGiveMax, sprintTimeToGiveCap;
    float sprintMultiplierToGive;
    [SerializeField] float sprintMultiplierToGiveMin, sprintMultiplierToGiveMax, sprintMultiplierToGiveCap;
    float sprintCooldownToGive;
    [SerializeField] float sprintCooldownToGiveMin, sprintCooldownToGiveMax, sprintCooldownToGiveCap;
    float rotationSpeedToGive;
    [SerializeField] float rotationSpeedToGiveMin, rotationSpeedToGiveMax, rotationSpeedToGiveCap;
    float shootToHealToGive;
    [SerializeField] float shootToHealToGiveMin, shootToHealToGiveMax, shootToHealToGiveCap;
    float healthToGive;
    [SerializeField] float healthToGiveMin, healthToGiveMax, healthToGiveCap;
    float bulletDamageToGive;
    [SerializeField] float bulletDamageToGiveMin, bulletDamageToGiveMax, bulletDamageToGiveCap;
    float fireRateToGive;
    [SerializeField] float fireRateToGiveMin, fireRateToGiveMax, fireRateToGiveCap;
    float flamethrowerDurationToGive;
    [SerializeField] float flameThrowerDurationMin, flameThrowerDurationMax, flameThrowerDurationCap;
    float bulletKnockbackToGive;
    [SerializeField] float bulletKnockbackToGiveMin, bulletKnockbackToGiveMax, bulletKnockbackToGiveCap;

    // Bot stats
    float botSprintDurationToGive;
    [SerializeField] float botSprintDurationToGiveMin, botSprintDurationToGiveMax, botSprintDurationToGiveCap;
    float botMoveSpeedToGive;
    [SerializeField] float botMoveSpeedToGiveMin, botMoveSpeedToGiveMax, botMoveSpeedToGiveCap;
    float botSprintSpeedToGive;
    [SerializeField] float botSprintSpeedToGiveMin, botSprintSpeedToGiveMax, botSprintSpeedToGiveCap;
    float botFireRateToGive;
    [SerializeField] float botFireRateToGiveMin, botFireRateToGiveMax, botFireRateToGiveCap;
    float botDamageToGive;
    [SerializeField] float botDamageToGiveMin, botDamageToGiveMax, botDamageToGiveCap;

    // ── References ────────────────────────────────────────────────────
    [SerializeField] PlayerController playerController;
    [SerializeField] AIHelperBot aiHelperBot;

    [SerializeField] Button[] upgradeButtons;
    [SerializeField] Button showXpButton;
    [SerializeField] Button showLevelButton;
    [SerializeField] Button showSprintButton;
    [SerializeField] Button showHealthBarsButton;
    [SerializeField] Button showTargetReticleButton;
    [SerializeField] Button showMiniMapButton;

    [SerializeField] GameObject upgradesPanel;
    [SerializeField] GameObject aiBotToActivate;

    // ── Dynamic weapon buttons ─────────────────────────────────────────
    [Header("Dynamic Weapon Buttons")]
    [SerializeField] List<WeaponData> weapons;
    [SerializeField] Button weaponButtonPrefab;
    [SerializeField] Transform weaponButtonContainer;

    // ── Dynamic bot buttons ────────────────────────────────────────────
    [Header("Dynamic Bot Buttons")]
    [SerializeField] List<BotData> bots;
    [SerializeField] Button botButtonPrefab;
    [SerializeField] Transform botButtonContainer;

    // Runtime bot state
    bool aiBotChosen;

    public enum WeaponType
    {
        None,
        DoubleGuns,
        MachineGun,
        Shotgun,
        Flamethrower
    }

    public enum BotType
    {
        None,
        MeleeBot,
        RangedBot
    }

    // ── Lifecycle ─────────────────────────────────────────────────────
    private void OnEnable() => GameManager.OnLevelEnd += HandleLevelEnd;
    private void OnDisable() => GameManager.OnLevelEnd -= HandleLevelEnd;


    #region GameData
    public const float MAX_LEVEL = 15f;

    public TextAsset gameData; // Assets/Resources/gameData.json

    [System.Serializable]
    public class BotGameData
    {
        float sprintDuration;
        float moveSpeed;
        float sprintSpeed;
        float fireRate;
        float damage;
    }

    [System.Serializable]
    public class GameData
    {
        float enemyDetectionRange;
        float viewRange;
        float moveSpeed;
        float sprintTime;
        float sprintMultiplier;
        float sprintCooldown;
        float rotationSpeed;
        float health;
        float bulletDamage;
        float fireRate;
        float flameThrowerDuration;
        float bulletKnockback;
        BotGameData bot;
    }

    static float CalcMax(float num)
    {
        return num / MAX_LEVEL;
    }

    static float CalcMin(float num)
    {
        return CalcMax(num) / 2f;
    }

    void LoadGameData()
    {
        GameData data = JsonUtility.FromJson<GameData>(gameData.text);

        enemyDetectionRangeToGiveCap = data?.enemyDetectionRange ?? 0f;
        enemyDetectionRangeToGiveMin = CalcMin(enemyDetectionRangeToGiveCap);
        enemyDetectionRangeToGiveMax = CalcMax(enemyDetectionRangeToGiveCap);

        viewRangeToGiveCap = data?.viewRange ?? 0f;
        viewRangeToGiveMin = CalcMin(viewRangeToGiveCap);
        viewRangeToGiveMax = CalcMax(viewRangeToGiveCap);

        moveSpeedToGiveCap = data?.moveSpeed ?? 0f;
        moveSpeedToGiveMin = CalcMin(moveSpeedToGiveCap);
        moveSpeedToGiveMax = CalcMax(moveSpeedToGiveCap);

        sprintTimeToGiveCap = data?.sprintTime ?? 0f;
        sprintTimeToGiveMin = CalcMin(sprintTimeToGiveCap);
        sprintTimeToGiveMax = CalcMax(sprintTimeToGiveCap);

        sprintMultiplierToGiveCap = data?.sprintMultiplier ?? 0f;
        sprintMultiplierToGiveMin = CalcMin(sprintMultiplierToGiveCap);
        sprintMultiplierToGiveMax = CalcMax(sprintMultiplierToGiveCap);

        sprintCooldownToGiveCap = data?.sprintCooldown ?? 0f;
        sprintCooldownToGiveMin = CalcMin(sprintCooldownToGiveCap);
        sprintCooldownToGiveMax = CalcMax(sprintCooldownToGiveCap);

        rotationSpeedToGiveCap = data?.rotationSpeed ?? 0f;
        rotationSpeedToGiveMin = CalcMin(rotationSpeedToGiveCap);
        rotationSpeedToGiveMax = CalcMax(rotationSpeedToGiveCap);

        healthToGiveCap = data?.health ?? 0f;
        healthToGiveMin = CalcMin(healthToGiveCap);
        healthToGiveMax = CalcMax(healthToGiveCap);

        bulletDamageToGiveCap = data?.bulletDamage ?? 0f;
        bulletDamageToGiveMin = CalcMin(bulletDamageToGiveCap);
        bulletDamageToGiveMax = CalcMax(bulletDamageToGiveCap);

        fireRateToGiveCap = data?.fireRate ?? 0f;
        fireRateToGiveMin = CalcMin(fireRateToGiveCap);
        fireRateToGiveMax = CalcMax(fireRateToGiveCap);

        flameThrowerDurationCap = data?.flameThrowerDuration ?? 0f;
        flameThrowerDurationMin = CalcMin(flameThrowerDurationCap);
        flameThrowerDurationMax = CalcMax(flameThrowerDurationCap);

        bulletKnockbackToGiveCap = data?.bulletKnockback ?? 0f;
        bulletKnockbackToGiveMin = CalcMin(bulletKnockbackToGiveCap);
        bulletKnockbackToGiveMax = CalcMax(bulletKnockbackToGiveCap);

        botSprintDurationToGiveCap = data?.bot?.sprintDuration ?? 0f;
        botSprintDurationToGiveMin = CalcMin(botSprintDurationToGiveCap);
        botSprintDurationToGiveMax = CalcMax(botSprintDurationToGiveCap);

        botMoveSpeedToGiveCap = data?.bot?.moveSpeed ?? 0f;
        botMoveSpeedToGiveMin = CalcMin(botMoveSpeedToGiveCap);
        botMoveSpeedToGiveMax = CalcMax(botMoveSpeedToGiveCap);

        botSprintSpeedToGiveCap = data?.bot?.sprintSpeed ?? 0f;
        botSprintSpeedToGiveMin = CalcMin(botSprintSpeedToGiveCap);
        botSprintSpeedToGiveMax = CalcMax(botSprintSpeedToGiveCap);

        botFireRateToGiveCap = data?.bot?.fireRate ?? 0f;
        botFireRateToGiveMin = CalcMin(botFireRateToGiveCap);
        botFireRateToGiveMax = CalcMax(botFireRateToGiveCap);

        botDamageToGiveCap = data?.bot?.damage ?? 0f;
        botDamageToGiveMin = CalcMin(botDamageToGiveCap);
        botDamageToGiveMax = CalcMax(botDamageToGiveCap);
    }
    #endregion

    private void Start()
    {
        LoadGameData();

        playerController = FindFirstObjectByType<PlayerController>();
        aiHelperBot = FindFirstObjectByType<AIHelperBot>();
        aiBotToActivate = aiHelperBot.gameObject;
        aiBotToActivate.SetActive(false);
    }

    // ── Weapon helpers ────────────────────────────────────────────────
    WeaponData GetWeaponData(WeaponType type) => weapons.Find(w => w.weaponType == type);

    bool IsWeaponActive(WeaponType type)
    {
        switch (type)
        {
            case WeaponType.DoubleGuns: return GameManager.Instance.doubleGunsActive;
            case WeaponType.MachineGun: return GameManager.Instance.machineGunActive;
            case WeaponType.Shotgun: return GameManager.Instance.shotgunActive;
            case WeaponType.Flamethrower: return GameManager.Instance.flamethrowerActive;
            default: return false;
        }
    }

    bool IsFlamethrowerPurchased()
    {
        var data = GetWeaponData(WeaponType.Flamethrower);
        return data != null && data.purchased;
    }

    // ── Bot helpers ───────────────────────────────────────────────────
    bool IsBotActive(BotType type)
    {
        switch (type)
        {
            case BotType.MeleeBot: return GameManager.Instance.aiMeleeBotActive;
            case BotType.RangedBot: return GameManager.Instance.aiRangedBotActive;
            default: return false;
        }
    }

    // ── Dynamic button builders ───────────────────────────────────────
    void BuildWeaponButtons()
    {
        foreach (Transform child in weaponButtonContainer)
            Destroy(child.gameObject);

        foreach (WeaponData weapon in weapons)
        {
            Button btn = Instantiate(weaponButtonPrefab, weaponButtonContainer);
            weapon.button = btn;

            TextMeshProUGUI[] tmps = btn.GetComponentsInChildren<TextMeshProUGUI>();
            if (tmps.Length >= 1) tmps[0].text = weapon.weaponName;
            if (tmps.Length >= 2)
            {
                weapon.priceText = tmps[1];
                weapon.priceText.text = weapon.purchased ? "" : "$ " + weapon.price;
            }

            btn.gameObject.SetActive(!IsWeaponActive(weapon.weaponType));

            WeaponData captured = weapon;
            btn.onClick.AddListener(() => BuyWeapon(captured));
        }
    }

    void BuildBotButtons()
    {
        foreach (Transform child in botButtonContainer)
            Destroy(child.gameObject);

        foreach (BotData bot in bots)
        {
            Button btn = Instantiate(botButtonPrefab, botButtonContainer);
            bot.button = btn;

            TextMeshProUGUI[] tmps = btn.GetComponentsInChildren<TextMeshProUGUI>();
            if (tmps.Length >= 1) tmps[0].text = bot.botName;
            if (tmps.Length >= 2)
            {
                bot.priceText = tmps[1];
                bot.priceText.text = bot.purchased ? "" : "$ " + bot.price;
            }

            // Hide only the bot that was actually chosen/activated
            bool isChosen = aiBotChosen && IsBotActive(bot.botType);
            btn.gameObject.SetActive(!isChosen);

            BotData captured = bot;
            btn.onClick.AddListener(() => BuyBot(captured));
        }
    }

    // ── Upgrade randomisation ─────────────────────────────────────────
    void RandomizeUpgrades()
    {
        if (TutorialManager.Instance != null && TutorialManager.Instance.isTutorial)
        {
            upgradesPanel.SetActive(true);

            viewRangeToGive = UnityEngine.Random.Range(viewRangeToGiveMin, viewRangeToGiveMax);
            moveSpeedToGive = UnityEngine.Random.Range(moveSpeedToGiveMin, moveSpeedToGiveMax);
            enemyDetectionRangeToGive = UnityEngine.Random.Range(enemyDetectionRangeToGiveMin, enemyDetectionRangeToGiveMax);
            bulletDamageToGive = UnityEngine.Random.Range(bulletDamageToGiveMin, bulletDamageToGiveMax);
            fireRateToGive = UnityEngine.Random.Range(fireRateToGiveMin, fireRateToGiveMax);
            sprintTimeToGive = UnityEngine.Random.Range(sprintTimeToGiveMin, sprintTimeToGiveMax);
            sprintCooldownToGive = UnityEngine.Random.Range(sprintCooldownToGiveMin, sprintCooldownToGiveMax);
            sprintMultiplierToGive = UnityEngine.Random.Range(sprintMultiplierToGiveMin, sprintMultiplierToGiveMax);
            rotationSpeedToGive = UnityEngine.Random.Range(rotationSpeedToGiveMin, rotationSpeedToGiveMax);
            shootToHealToGive = UnityEngine.Random.Range(shootToHealToGiveMin, shootToHealToGiveMax);
            flamethrowerDurationToGive = UnityEngine.Random.Range(flameThrowerDurationMin, flameThrowerDurationMax);
            bulletKnockbackToGive = UnityEngine.Random.Range(bulletKnockbackToGiveMin, bulletKnockbackToGiveMax);

            float viewRangeRounded = Mathf.Round(viewRangeToGive * 100f) / 100f;
            float moveSpeedRounded = Mathf.Round(moveSpeedToGive * 100f) / 100f;
            float enemyDetectionRounded = Mathf.Round(enemyDetectionRangeToGive * 100f) / 100f;
            float bulletDamageRounded = Mathf.Round(bulletDamageToGive * 100f) / 100f;
            float fireRateRounded = Mathf.Round(fireRateToGive * 100f) / 100f;
            float sprintTimeRounded = Mathf.Round(sprintTimeToGive * 100f) / 100f;
            float sprintCooldownRounded = Mathf.Round(sprintCooldownToGive * 100f) / 100f;
            float sprintMultiplierRounded = Mathf.Round(sprintMultiplierToGive * 100f) / 100f;
            float rotationSpeedRounded = Mathf.Round(rotationSpeedToGive * 100f) / 100f;
            float shootToHealRounded = Mathf.Round(shootToHealToGive * 100f) / 100f;
            float bulletKnockbackRounded = Mathf.Round(bulletKnockbackToGive * 100f) / 100f;

            var allUpgrades = new List<(Action action, string name)>
            {
                (GiveViewRangeToPlayer,           "+" + viewRangeRounded        + " View Range"),
                (GiveMoveSpeedToPlayer,           "+" + moveSpeedRounded        + " Move Speed"),
                (GiveEnemyDetectionRangeToPlayer, "+" + enemyDetectionRounded   + " Enemy Detection"),
                (GiveBulletDamageToPlayer,        "+" + bulletDamageRounded     + " Bullet Damage"),
                (GiveFireRateToPlayer,            "+" + fireRateRounded         + " Fire Rate"),
                (GiveSprintTimeToPlayer,          "+" + sprintTimeRounded       + " Sprint Time"),
                (GiveSprintMultiplierToPlayer,    "+" + sprintMultiplierRounded + " Sprint Multiplier"),
                (GiveRotationSpeedToPlayer,       "+" + rotationSpeedRounded    + " Rotation Speed"),
                (GiveShootToHealToPlayer,         "+" + shootToHealRounded      + " Shoot To Heal"),
                (GiveBulletKnockbackToPlayer,     "+" + bulletKnockbackRounded  + " Bullet Knockback"),
            };

            foreach (Button btn in upgradeButtons)
                btn.onClick.RemoveAllListeners();

            for (int i = 0; i < upgradeButtons.Length; i++)
            {
                int randIndex = UnityEngine.Random.Range(0, allUpgrades.Count);
                var chosen = allUpgrades[randIndex];
                upgradeButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = chosen.name;
                Action copy = chosen.action;
                upgradeButtons[i].onClick.AddListener(() => copy());
                allUpgrades.RemoveAt(randIndex);
            }

            showHealthBarsButton.interactable = false;
            showLevelButton.interactable = false;
            showMiniMapButton.interactable = false;
            showSprintButton.interactable = false;
            showTargetReticleButton.interactable = false;
            showXpButton.interactable = false;

            // Build buttons but disable all in tutorial
            BuildWeaponButtons();
            foreach (WeaponData w in weapons)
                if (w.button != null) w.button.interactable = false;

            BuildBotButtons();
            foreach (BotData b in bots)
                if (b.button != null) b.button.interactable = false;
        }
        else
        {
            upgradesPanel.SetActive(true);

            healthToGive = UnityEngine.Random.Range(healthToGiveMin, healthToGiveMax);
            viewRangeToGive = UnityEngine.Random.Range(viewRangeToGiveMin, viewRangeToGiveMax);
            moveSpeedToGive = UnityEngine.Random.Range(moveSpeedToGiveMin, moveSpeedToGiveMax);
            enemyDetectionRangeToGive = UnityEngine.Random.Range(enemyDetectionRangeToGiveMin, enemyDetectionRangeToGiveMax);
            bulletDamageToGive = UnityEngine.Random.Range(bulletDamageToGiveMin, bulletDamageToGiveMax);
            fireRateToGive = UnityEngine.Random.Range(fireRateToGiveMin, fireRateToGiveMax);
            sprintTimeToGive = UnityEngine.Random.Range(sprintTimeToGiveMin, sprintTimeToGiveMax);
            sprintCooldownToGive = UnityEngine.Random.Range(sprintCooldownToGiveMin, sprintCooldownToGiveMax);
            sprintMultiplierToGive = UnityEngine.Random.Range(sprintMultiplierToGiveMin, sprintMultiplierToGiveMax);
            rotationSpeedToGive = UnityEngine.Random.Range(rotationSpeedToGiveMin, rotationSpeedToGiveMax);
            shootToHealToGive = UnityEngine.Random.Range(shootToHealToGiveMin, shootToHealToGiveMax);
            flamethrowerDurationToGive = UnityEngine.Random.Range(flameThrowerDurationMin, flameThrowerDurationMax);
            bulletKnockbackToGive = UnityEngine.Random.Range(bulletKnockbackToGiveMin, bulletKnockbackToGiveMax);

            botMoveSpeedToGive = UnityEngine.Random.Range(botMoveSpeedToGiveMin, botMoveSpeedToGiveMax);
            botDamageToGive = UnityEngine.Random.Range(botDamageToGiveMin, botDamageToGiveMax);
            botFireRateToGive = UnityEngine.Random.Range(botFireRateToGiveMin, botFireRateToGiveMax);
            botSprintSpeedToGive = UnityEngine.Random.Range(botSprintSpeedToGiveMin, botSprintSpeedToGiveMax);
            botSprintDurationToGive = UnityEngine.Random.Range(botSprintDurationToGiveMin, botSprintDurationToGiveMax);

            float healthRounded = Mathf.Round(healthToGive * 100f) / 100f;
            float viewRangeRounded = Mathf.Round(viewRangeToGive * 100f) / 100f;
            float moveSpeedRounded = Mathf.Round(moveSpeedToGive * 100f) / 100f;
            float enemyDetectionRounded = Mathf.Round(enemyDetectionRangeToGive * 100f) / 100f;
            float bulletDamageRounded = Mathf.Round(bulletDamageToGive * 100f) / 100f;
            float fireRateRounded = Mathf.Round(fireRateToGive * 10000f) / 10000f;
            float sprintTimeRounded = Mathf.Round(sprintTimeToGive * 100f) / 100f;
            float sprintCooldownRounded = Mathf.Round(sprintCooldownToGive * 100f) / 100f;
            float sprintMultiplierRounded = Mathf.Round(sprintMultiplierToGive * 100f) / 100f;
            float rotationSpeedRounded = Mathf.Round(rotationSpeedToGive * 100f) / 100f;
            float shootToHealRounded = Mathf.Round(shootToHealToGive * 100f) / 100f;
            float flamethrowerDurRounded = Mathf.Round(flamethrowerDurationToGive * 100f) / 100f;
            float bulletKnockbackRounded = Mathf.Round(bulletKnockbackToGive * 100f) / 100f;

            float botMoveSpeedRounded = MathF.Round(botMoveSpeedToGive * 100f) / 100f;
            float botDamageRounded = MathF.Round(botDamageToGive * 100f) / 100f;
            float botFireRateRounded = MathF.Round(botFireRateToGive * 10000f) / 10000f;
            float botSprintSpeedRounded = MathF.Round(botSprintSpeedToGive * 100f) / 100f;
            float botSprintDurRounded = MathF.Round(botSprintDurationToGive * 100f) / 100f;

            var allUpgrades = new List<(Action action, string name)>();

            if (playerController.GetMaxHealth < healthToGiveCap) allUpgrades.Add((GiveHealthToPlayer, "+" + healthRounded + " Health"));
            if (playerController.GetCameraViewDistance < viewRangeToGiveCap) allUpgrades.Add((GiveViewRangeToPlayer, "+" + viewRangeRounded + " View Range"));
            if (playerController.GetMoveSpeed < moveSpeedToGiveCap) allUpgrades.Add((GiveMoveSpeedToPlayer, "+" + moveSpeedRounded + " Move Speed"));
            if (playerController.GetEnemyDetectionRange < enemyDetectionRangeToGiveCap) allUpgrades.Add((GiveEnemyDetectionRangeToPlayer, "+" + enemyDetectionRounded + " Enemy Detection"));
            if (playerController.GetBulletDamage < bulletDamageToGiveCap) allUpgrades.Add((GiveBulletDamageToPlayer, "+" + bulletDamageRounded + " Bullet Damage"));
            if (playerController.GetFireRate > fireRateToGiveCap) allUpgrades.Add((GiveFireRateToPlayer, "+" + fireRateRounded + " Fire Rate"));
            if (playerController.GetSprintTime < sprintTimeToGiveCap) allUpgrades.Add((GiveSprintTimeToPlayer, "+" + sprintTimeRounded + " Sprint Time"));
            if (playerController.GetSprintMultiplier < sprintMultiplierToGiveCap) allUpgrades.Add((GiveSprintMultiplierToPlayer, "+" + sprintMultiplierRounded + " Sprint Multiplier"));
            if (playerController.GetRotationSpeed < rotationSpeedToGiveCap) allUpgrades.Add((GiveRotationSpeedToPlayer, "+" + rotationSpeedRounded + " Rotation Speed"));
            if (playerController.GetShootToHeal < shootToHealToGiveCap) allUpgrades.Add((GiveShootToHealToPlayer, "+" + shootToHealRounded + " Shoot To Heal"));
            if (playerController.GetBulletKnockback < bulletKnockbackToGiveCap) allUpgrades.Add((GiveBulletKnockbackToPlayer, "+" + bulletKnockbackRounded + " Bullet Knockback"));
            if (playerController.GetSprintCooldown > sprintCooldownToGiveCap) allUpgrades.Add((GiveSprintCooldownToPlayer, "+" + sprintCooldownRounded + " Sprint Cooldown"));
            if (IsFlamethrowerPurchased())
                allUpgrades.Add((GiveFlamethrowerDurationToPlayer, "+" + flamethrowerDurRounded + " Flamethrower Duration"));

            if (GameManager.Instance.aiMeleeBotActive || GameManager.Instance.aiRangedBotActive)
            {
                if (aiHelperBot.GetMoveSpeed < botMoveSpeedToGiveCap) allUpgrades.Add((GiveMoveSpeedToBot, "+" + botMoveSpeedRounded + " Bot Move Speed"));
                if (aiHelperBot.GetDamage < botDamageToGiveCap) allUpgrades.Add((GiveDamageToBot, "+" + botDamageRounded + " Bot Damage"));
                if (aiHelperBot.GetFireRate < botFireRateToGiveCap) allUpgrades.Add((GiveFireRateToBot, "+" + botFireRateRounded + " Bot Fire Rate"));
                if (aiHelperBot.GetSprintSpeed < botSprintSpeedToGiveCap) allUpgrades.Add((GiveSprintSpeedToBot, "+" + botSprintSpeedRounded + " Bot Sprint Speed"));
                if (aiHelperBot.GetSprintDuration < botSprintDurationToGiveCap) allUpgrades.Add((GiveSprintDurationToBot, "+" + botSprintDurRounded + " Bot Sprint Duration"));
            }

            foreach (Button btn in upgradeButtons)
                btn.onClick.RemoveAllListeners();

            for (int i = 0; i < upgradeButtons.Length; i++)
            {
                int randIndex = UnityEngine.Random.Range(0, allUpgrades.Count);
                var chosen = allUpgrades[randIndex];
                upgradeButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = chosen.name;
                Action copy = chosen.action;
                upgradeButtons[i].onClick.AddListener(() => copy());
                allUpgrades.RemoveAt(randIndex);
            }

            BuildWeaponButtons();
            BuildBotButtons();
        }
    }

    // ── Upgrade Handlers ──────────────────────────────────────────────
    void GiveHealthToPlayer()
    {
        playerController.GetMaxHealth += healthToGive;
        playerController.UpdateHealth(healthToGive);
        playerController.UpdateMaxHealth();
        StartLevel();
    }

    void GiveViewRangeToPlayer() { playerController.GetCameraViewDistance += viewRangeToGive; StartLevel(); AbilitySelect(); }
    void GiveMoveSpeedToPlayer() { playerController.GetMoveSpeed += moveSpeedToGive; StartLevel(); AbilitySelect(); }
    void GiveEnemyDetectionRangeToPlayer() { playerController.GetEnemyDetectionRange += enemyDetectionRangeToGive; StartLevel(); AbilitySelect(); }
    void GiveBulletDamageToPlayer() { playerController.GetBulletDamage += bulletDamageToGive; StartLevel(); AbilitySelect(); }
    void GiveFireRateToPlayer() { playerController.GetFireRate -= fireRateToGive; StartLevel(); AbilitySelect(); }
    void GiveSprintTimeToPlayer() { playerController.GetSprintTime += sprintTimeToGive; StartLevel(); AbilitySelect(); }
    void GiveSprintCooldownToPlayer() { playerController.GetSprintCooldown -= sprintCooldownToGive; StartLevel(); AbilitySelect(); }
    void GiveSprintMultiplierToPlayer() { playerController.GetSprintMultiplier += sprintMultiplierToGive; StartLevel(); AbilitySelect(); }
    void GiveRotationSpeedToPlayer() { playerController.GetRotationSpeed += rotationSpeedToGive; StartLevel(); AbilitySelect(); }
    void GiveShootToHealToPlayer() { playerController.GetShootToHeal += shootToHealToGive; StartLevel(); AbilitySelect(); }
    void GiveFlamethrowerDurationToPlayer() { playerController.GetFlameThrowerDuration += flamethrowerDurationToGive; StartLevel(); AbilitySelect(); }
    void GiveBulletKnockbackToPlayer() { playerController.GetBulletKnockback += bulletKnockbackToGive; StartLevel(); AbilitySelect(); }

    void GiveMoveSpeedToBot() { aiHelperBot.GetMoveSpeed += botMoveSpeedToGive; StartLevel(); }
    void GiveDamageToBot() { aiHelperBot.GetDamage += botDamageToGive; StartLevel(); }
    void GiveFireRateToBot() { aiHelperBot.GetFireRate -= botFireRateToGive; StartLevel(); }
    void GiveSprintSpeedToBot() { aiHelperBot.GetSprintSpeed += botSprintSpeedToGive; StartLevel(); }
    void GiveSprintDurationToBot() { aiHelperBot.GetSprintDuration += botSprintDurationToGive; StartLevel(); }

    // ── Info Buttons ──────────────────────────────────────────────────
    public void ClickShowSprintButton() { showSprintButton.gameObject.SetActive(false); GameManager.Instance.showSprintSlider = true; StartLevel(); }
    public void ClickShowXpButton() { showXpButton.gameObject.SetActive(false); GameManager.Instance.showXP = true; StartLevel(); }
    public void ClickShowLevelButton() { showLevelButton.gameObject.SetActive(false); StartLevel(); }
    public void ClickShowHealthBarsButton() { showHealthBarsButton.gameObject.SetActive(false); GameManager.Instance.showHealthBars = true; StartLevel(); }
    public void ClickShowTargetReticleButton() { showTargetReticleButton.gameObject.SetActive(false); GameManager.Instance.showTargetReticle = true; StartLevel(); }
    public void ClickShowMiniMapButton() { showMiniMapButton.gameObject.SetActive(false); StartLevel(); }

    // ── Weapon Logic ──────────────────────────────────────────────────
    void SwapWeapon(WeaponType newWeapon)
    {
        if (GameManager.Instance.machineGunActive)
            playerController.GetFireRate /= .35f;

        GameManager.Instance.doubleGunsActive = false;
        GameManager.Instance.machineGunActive = false;
        GameManager.Instance.shotgunActive = false;
        GameManager.Instance.flamethrowerActive = false;

        foreach (WeaponData w in weapons)
            if (w.button != null) w.button.gameObject.SetActive(true);

        switch (newWeapon)
        {
            case WeaponType.DoubleGuns:
                GameManager.Instance.doubleGunsActive = true;
                break;
            case WeaponType.MachineGun:
                GameManager.Instance.machineGunActive = true;
                playerController.GetFireRate *= .35f;
                break;
            case WeaponType.Shotgun:
                GameManager.Instance.shotgunActive = true;
                break;
            case WeaponType.Flamethrower:
                GameManager.Instance.flamethrowerActive = true;
                break;
        }

        WeaponData active = GetWeaponData(newWeapon);
        if (active?.button != null)
            active.button.gameObject.SetActive(false);
    }

    void BuyWeapon(WeaponData weapon)
    {
        if (!weapon.purchased)
        {
            if (playerController.GetCash < weapon.price) return;
            playerController.GetCash -= weapon.price;
            weapon.purchased = true;
            if (weapon.priceText != null) weapon.priceText.text = "";
        }

        SwapWeapon(weapon.weaponType);
        StartLevel();
    }

    // ── Bot Logic ─────────────────────────────────────────────────────
    void BuyBot(BotData bot)
    {
        if (!bot.purchased)
        {
            if (playerController.GetCash < bot.price) return;
            playerController.GetCash -= bot.price;
            bot.purchased = true;
            if (bot.priceText != null) bot.priceText.text = "";
        }

        // Hide only the chosen bot's button; the other bot's button stays visible
        if (bot.button != null)
            bot.button.gameObject.SetActive(false);

        aiBotToActivate.SetActive(true);
        aiBotChosen = true;

        AIHelperBot helperBot = aiBotToActivate.GetComponent<AIHelperBot>();

        switch (bot.botType)
        {
            case BotType.MeleeBot:
                GameManager.Instance.aiMeleeBotActive = true;
                helperBot.isMelee = true;
                helperBot.isRanged = false;
                break;
            case BotType.RangedBot:
                GameManager.Instance.aiRangedBotActive = true;
                helperBot.isRanged = true;
                helperBot.isMelee = false;
                break;
        }

        StartLevel();
    }

    // ── Internals ─────────────────────────────────────────────────────
    void StartLevel()
    {
        GameManager.Instance.StartLevel();
        upgradesPanel.SetActive(false);
    }

    void AbilitySelect()
    {
        if (TutorialManager.Instance != null && TutorialManager.Instance.isTutorial)
        {
            var page = TutorialManager.Instance.GetTutorialPages[TutorialManager.Instance.GetCurrentPageIndex];
            if (page.header == "Ability Select")
                TutorialManager.Instance.CompleteStep();
        }
    }

    private void HandleLevelEnd() => RandomizeUpgrades();
}