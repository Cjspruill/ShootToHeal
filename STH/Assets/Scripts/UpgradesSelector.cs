using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradesSelector : MonoBehaviour
{
    float healthToGive;
    [SerializeField] float healthToGiveMin;
    [SerializeField] float healthToGiveMax;
    [SerializeField] float healthToGiveCap;
    float viewRangeToGive;
    [SerializeField] float viewRangeToGiveMin;
    [SerializeField] float viewRangeToGiveMax;
    [SerializeField] float viewRangeToGiveCap;
    float moveSpeedToGive;
    [SerializeField] float moveSpeedToGiveMin;
    [SerializeField] float moveSpeedToGiveMax;
    [SerializeField] float moveSpeedToGiveCap;
    float enemyDetectionRangeToGive;
    [SerializeField] float enemyDetectionRangeToGiveMin;
    [SerializeField] float enemyDetectionRangeToGiveMax;
    [SerializeField] float enemyDetectionRangeToGiveCap;
    float bulletDamageToGive;
    [SerializeField] float bulletDamageToGiveMin;
    [SerializeField] float bulletDamageToGiveMax;
    [SerializeField] float bulletDamageToGiveCap;
    float fireRateToGive;
    [SerializeField] float fireRateToGiveMin;
    [SerializeField] float fireRateToGiveMax;
    [SerializeField] float fireRateToGiveCap;
    float sprintTimeToGive;
    [SerializeField] float sprintTimeToGiveMin;
    [SerializeField] float sprintTimeToGiveMax;
    [SerializeField] float sprintTimeToGiveCap;
    float sprintCooldownToGive;
    [SerializeField] float sprintCooldownToGiveMin;
    [SerializeField] float sprintCooldownToGiveMax;
    [SerializeField] float sprintCooldownToGiveCap;
    float sprintMultiplierToGive;
    [SerializeField] float sprintMultiplierToGiveMin;
    [SerializeField] float sprintMultiplierToGiveMax;
    [SerializeField] float sprintMultiplierToGiveCap;
    float rotationSpeedToGive;
    [SerializeField] float rotationSpeedToGiveMin;
    [SerializeField] float rotationSpeedToGiveMax;
    [SerializeField] float rotationSpeedToGiveCap;
    float shootToHealToGive;
    [SerializeField] float shootToHealToGiveMin;
    [SerializeField] float shootToHealToGiveMax;
    [SerializeField] float shootToHealToGiveCap;
    float flamethrowerDurationToGive;
    [SerializeField] float flameThrowerDurationMin;
    [SerializeField] float flameThrowerDurationMax;
    [SerializeField] float flameThrowerDurationCap;
    float bulletKnockbackToGive;
    [SerializeField] float bulletKnockbackToGiveMin;
    [SerializeField] float bulletKnockbackToGiveMax;
    [SerializeField] float bulletKnockbackToGiveCap;

    //Bot
    float botMoveSpeedToGive;
    [SerializeField] float botMoveSpeedToGiveMin;
    [SerializeField] float botMoveSpeedToGiveMax;
    [SerializeField] float botMoveSpeedToGiveCap;
    float botDamageToGive;
    [SerializeField] float botDamageToGiveMin;
    [SerializeField] float botDamageToGiveMax;
    [SerializeField] float botDamageToGiveCap;
    float botFireRateToGive;
    [SerializeField] float botFireRateToGiveMin;
    [SerializeField] float botFireRateToGiveMax;
    [SerializeField] float botFireRateToGiveCap;
    float botSprintSpeedToGive;
    [SerializeField] float botSprintSpeedToGiveMin;
    [SerializeField] float botSprintSpeedToGiveMax;
    [SerializeField] float botSprintSpeedToGiveCap;
    float botSprintDurationToGive;
    [SerializeField] float botSprintDurationToGiveMin;
    [SerializeField] float botSprintDurationToGiveMax;
    [SerializeField] float botSprintDurationToGiveCap;

    [SerializeField] PlayerController playerController;
    [SerializeField] AIHelperBot aiHelperBot;

    [SerializeField] Button[] upgradeButtons;
    [SerializeField] Button showXpButton;
    [SerializeField] Button showLevelButton;
    [SerializeField] Button showSprintButton;
    [SerializeField] Button showHealthBarsButton;
    [SerializeField] Button showTargetReticleButton;
    [SerializeField] Button showMiniMapButton;
    [SerializeField] Button doubleGunsButton;
    [SerializeField] Button machineGunButton;
    [SerializeField] Button shotgunButton;
    [SerializeField] Button flamethrowerButton;
    [SerializeField] Button aiMeleeBotButton;
    [SerializeField] Button aiRangedBotButton;

    [SerializeField] GameObject upgradesPanel;

    [SerializeField] float doubleGunsPrice = 5;
    [SerializeField] float machineGunPrice = 5;
    [SerializeField] float shotgunPrice = 10;
    [SerializeField] float flamethrowerPrice = 10;
    [SerializeField] float aiMeleeBotPrice = 10;
    [SerializeField] float aiRangedBotPrice = 10;


    [SerializeField] bool doubleGunsPurchased;
    [SerializeField] bool machineGunPurchased;
    [SerializeField] bool shotgunPurchased;
    [SerializeField] bool flamethrowerPurchased;
    [SerializeField] bool aiMeleeBotPurchased;
    [SerializeField] bool aiRangedBotPurchased;
    [SerializeField] bool aiBotChosen;


    [SerializeField] TextMeshProUGUI doubleGunPriceText;
    [SerializeField] TextMeshProUGUI machineGunPriceText;
    [SerializeField] TextMeshProUGUI shotgunPriceText;
    [SerializeField] TextMeshProUGUI flameThrowerPriceText;
    [SerializeField] TextMeshProUGUI aiMeleeBotPriceText;
    [SerializeField] TextMeshProUGUI aiRangedBotPriceText;


    [SerializeField] GameObject aiBotToActivate;
    public enum WeaponType
    {
        None,
        DoubleGuns,
        MachineGun,
        Shotgun,
        Flamethrower
    }

    private void OnEnable()
    {
        GameManager.OnLevelEnd += HandleLevelEnd;
    }


    private void OnDisable()
    {
        GameManager.OnLevelEnd -= HandleLevelEnd;

        // Make sure to unsubscribe when destroyed
        if (LevelPlayAds.Instance != null)
        {
            LevelPlayAds.Instance.OnAnyAdClosed -= ShowUpgradesAfterAd;
        }
    }

    private void Start()
    {
        playerController = FindFirstObjectByType<PlayerController>();
        aiHelperBot = FindFirstObjectByType<AIHelperBot>();
        aiBotToActivate = aiHelperBot.gameObject;
        aiBotToActivate.SetActive(false);
    }

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
            float enemyDetectionRangeRounded = Mathf.Round(enemyDetectionRangeToGive * 100f) / 100f;
            float bulletDamageRounded = Mathf.Round(bulletDamageToGive * 100f) / 100f;
            float fireRateRounded = Mathf.Round(fireRateToGive * 100f) / 100f;
            float sprintTimeRounded = Mathf.Round(sprintTimeToGive * 100f) / 100f;
            float sprintCooldownRounded = Mathf.Round(sprintCooldownToGive * 100f) / 100f;
            float sprintMultiplierRounded = Mathf.Round(sprintMultiplierToGive * 100f) / 100f;
            float rotationSpeedRounded = Mathf.Round(rotationSpeedToGive * 100f) / 100f;
            float shootToHealRounded = Mathf.Round(shootToHealToGive * 100f) / 100f;
            float flamethrowerDurationRounded = Mathf.Round(flamethrowerDurationToGive * 100f) / 100f;
            float bulletKnockbackRounded = Mathf.Round(bulletKnockbackToGive * 100f) / 100f;

            List<(Action action, string name)> allUpgrades = new List<(Action, string)>
            { 
    (GiveViewRangeToPlayer, "+" + viewRangeRounded + " View Range"),
    (GiveMoveSpeedToPlayer, "+" + moveSpeedRounded + " Move Speed"),
    (GiveEnemyDetectionRangeToPlayer, "+" + enemyDetectionRangeRounded + " Enemy Detection"),
    (GiveBulletDamageToPlayer, "+" + bulletDamageRounded + " Bullet Damage"),
    (GiveFireRateToPlayer, "+" + fireRateRounded + " Fire Rate"),
    (GiveSprintTimeToPlayer, "+" + sprintTimeRounded + " Sprint Time"),
    (GiveSprintMultiplierToPlayer, "+" + sprintMultiplierRounded + " Sprint Multiplier"),
    (GiveRotationSpeedToPlayer, "+" + rotationSpeedRounded + " Rotation Speed"),
    (GiveShootToHealToPlayer, "+" + shootToHealRounded + " Shoot To Heal"),
    (GiveBulletKnockbackToPlayer, "+" + bulletKnockbackRounded + " Bullet Knockback")
            };

            foreach (Button btn in upgradeButtons)
                btn.onClick.RemoveAllListeners();

            for (int i = 0; i < upgradeButtons.Length; i++)
            {
                int randIndex = UnityEngine.Random.Range(0, allUpgrades.Count);
                var chosenUpgrade = allUpgrades[randIndex];

                TextMeshProUGUI btnText = upgradeButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                if (btnText != null)
                    btnText.text = chosenUpgrade.name;

                Action upgradeCopy = chosenUpgrade.action;
                upgradeButtons[i].onClick.AddListener(() => upgradeCopy());

                allUpgrades.RemoveAt(randIndex);
            }

            doubleGunPriceText.text = doubleGunsPurchased ? "" : "$ " + doubleGunsPrice;
            machineGunPriceText.text = machineGunPurchased ? "" : "$ " + machineGunPrice;
            shotgunPriceText.text = shotgunPurchased ? "" : "$ " + shotgunPrice;
            flameThrowerPriceText.text = flamethrowerPurchased ? "" : "$ " + flamethrowerPrice;
            aiMeleeBotPriceText.text = aiMeleeBotPurchased ? "" : "$ " + aiMeleeBotPrice;
            aiRangedBotPriceText.text = aiRangedBotPurchased ? "" : "$ " + aiRangedBotPrice;

            showHealthBarsButton.interactable = false;
            showLevelButton.interactable = false;
            showMiniMapButton.interactable = false;
            showSprintButton.interactable = false;
            showTargetReticleButton.interactable = false;
            showXpButton.interactable = false;

            doubleGunsButton.interactable = false;
            machineGunButton.interactable = false;
            shotgunButton.interactable = false;
            flamethrowerButton.interactable = false;

            aiMeleeBotButton.interactable = false;
            aiRangedBotButton.interactable = false;
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
            float enemyDetectionRangeRounded = Mathf.Round(enemyDetectionRangeToGive * 100f) / 100f;
            float bulletDamageRounded = Mathf.Round(bulletDamageToGive * 100f) / 100f;
            float fireRateRounded = Mathf.Round(fireRateToGive * 10000f) / 10000f;
            float sprintTimeRounded = Mathf.Round(sprintTimeToGive * 100f) / 100f;
            float sprintCooldownRounded = Mathf.Round(sprintCooldownToGive * 100f) / 100f;
            float sprintMultiplierRounded = Mathf.Round(sprintMultiplierToGive * 100f) / 100f;
            float rotationSpeedRounded = Mathf.Round(rotationSpeedToGive * 100f) / 100f;
            float shootToHealRounded = Mathf.Round(shootToHealToGive * 100f) / 100f;
            float flamethrowerDurationRounded = Mathf.Round(flamethrowerDurationToGive * 100f) / 100f;
            float bulletKnockbackRounded = Mathf.Round(bulletKnockbackToGive * 100f) / 100f;

            float botMoveSpeedRounded = MathF.Round(botMoveSpeedToGive * 100f) / 100f;
            float botDamageRounded = MathF.Round(botDamageToGive * 100f) / 100f;
            float botFireRateRounded = MathF.Round(botFireRateToGive * 10000f) / 10000f;
            float botSprintSpeedRounded = MathF.Round(botSprintSpeedToGive * 100f) / 100f;
            float botSprintDurationRounded = MathF.Round(botSprintDurationToGive * 100f) / 100f;

            List<(Action action, string name)> allUpgrades = new List<(Action, string)> { };

            if (playerController.GetMaxHealth < healthToGiveCap)
                allUpgrades.Add((GiveHealthToPlayer, "+" + healthRounded + " Health"));
            if (playerController.GetCameraViewDistance < viewRangeToGiveCap)
                allUpgrades.Add((GiveViewRangeToPlayer, "+" + viewRangeRounded + " View Range"));
            if (playerController.GetMoveSpeed < moveSpeedToGiveCap)
                allUpgrades.Add((GiveMoveSpeedToPlayer, "+" + moveSpeedRounded + " Move Speed"));
            if (playerController.GetEnemyDetectionRange < enemyDetectionRangeToGiveCap)
                allUpgrades.Add((GiveEnemyDetectionRangeToPlayer, "+" + enemyDetectionRangeRounded + " Enemy Detection"));
            if (playerController.GetBulletDamage < bulletDamageToGiveCap)
                allUpgrades.Add((GiveBulletDamageToPlayer, "+" + bulletDamageRounded + " Bullet Damage"));
            if (playerController.GetFireRate > fireRateToGiveCap)
                allUpgrades.Add((GiveFireRateToPlayer, "+" + fireRateRounded + " Fire Rate"));
            if (playerController.GetSprintTime < sprintTimeToGiveCap)
                allUpgrades.Add((GiveSprintTimeToPlayer, "+" + sprintTimeRounded + " Sprint Time"));
            if (playerController.GetSprintMultiplier < sprintMultiplierToGiveCap)
                allUpgrades.Add((GiveSprintMultiplierToPlayer, "+" + sprintMultiplierRounded + " Sprint Multiplier"));
            if (playerController.GetRotationSpeed < rotationSpeedToGiveCap)
                allUpgrades.Add((GiveRotationSpeedToPlayer, "+" + rotationSpeedRounded + " Rotation Speed"));
            if (playerController.GetShootToHeal < shootToHealToGiveCap)
                allUpgrades.Add((GiveShootToHealToPlayer, "+" + shootToHealRounded + " Shoot To Heal"));
            if (playerController.GetBulletKnockback < bulletKnockbackToGiveCap)
                allUpgrades.Add((GiveBulletKnockbackToPlayer, "+" + bulletKnockbackRounded + " Bullet Knockback"));
            if (playerController.GetSprintCooldown > sprintCooldownToGiveCap)
                allUpgrades.Add((GiveSprintCooldownToPlayer, "+" + sprintCooldownRounded + " Sprint Cooldown"));
            if (flamethrowerPurchased)
                allUpgrades.Add((GiveFlamethrowerDurationToPlayer, "+" + flamethrowerDurationRounded + " Flamethrower Duration"));

            // ✅ Only add bot upgrades if a bot is active
            if (aiMeleeBotPurchased || aiRangedBotPurchased)
            {
                if (aiHelperBot.GetMoveSpeed < botMoveSpeedToGiveCap)
                    allUpgrades.Add((GiveMoveSpeedToBot, "+" + botMoveSpeedRounded + " Bot Move Speed"));
                if (aiHelperBot.GetDamage < botDamageToGiveCap)
                    allUpgrades.Add((GiveDamageToBot, "+" + botDamageRounded + " Bot Damage"));
                if (aiHelperBot.GetFireRate < botFireRateToGiveCap)
                    allUpgrades.Add((GiveFireRateToBot, "+" + botFireRateRounded + " Bot Fire Rate"));
                if (aiHelperBot.GetSprintSpeed < botSprintSpeedToGiveCap)
                    allUpgrades.Add((GiveSprintSpeedToBot, "+" + botSprintSpeedRounded + " Bot Sprint Speed"));
                if (aiHelperBot.GetSprintDuration < botSprintDurationToGiveCap)
                    allUpgrades.Add((GiveSprintDurationToBot, "+" + botSprintDurationRounded + " Bot Sprint Duration"));
            }


            foreach (Button btn in upgradeButtons)
                btn.onClick.RemoveAllListeners();

            for (int i = 0; i < upgradeButtons.Length; i++)
            {
                int randIndex = UnityEngine.Random.Range(0, allUpgrades.Count);
                var chosenUpgrade = allUpgrades[randIndex];

                TextMeshProUGUI btnText = upgradeButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                if (btnText != null)
                    btnText.text = chosenUpgrade.name;

                Action upgradeCopy = chosenUpgrade.action;
                upgradeButtons[i].onClick.AddListener(() => upgradeCopy());

                allUpgrades.RemoveAt(randIndex);
            }

            doubleGunPriceText.text = doubleGunsPurchased ? "" : "$ " + doubleGunsPrice;
            machineGunPriceText.text = machineGunPurchased ? "" : "$ " + machineGunPrice;
            shotgunPriceText.text = shotgunPurchased ? "" : "$ " + shotgunPrice;
            flameThrowerPriceText.text = flamethrowerPurchased ? "" : "$ " + flamethrowerPrice;
            aiMeleeBotPriceText.text = aiMeleeBotPurchased ? "" : "$ " + aiMeleeBotPrice;
            aiRangedBotPriceText.text = aiRangedBotPurchased ? "" : "$ " + aiRangedBotPrice;

            doubleGunsButton.gameObject.SetActive(!GameManager.Instance.doubleGunsActive);
            machineGunButton.gameObject.SetActive(!GameManager.Instance.machineGunActive);
            shotgunButton.gameObject.SetActive(!GameManager.Instance.shotgunActive);
            flamethrowerButton.gameObject.SetActive(!GameManager.Instance.flamethrowerActive);

            aiMeleeBotButton.gameObject.SetActive(!aiBotChosen && !GameManager.Instance.aiMeleeBotActive);
            aiRangedBotButton.gameObject.SetActive(!aiBotChosen && !GameManager.Instance.aiRangedBotActive);
        }
    }

    // === Upgrade Handlers ===
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

    // === Info Buttons ===
    public void ClickShowSprintButton() { showSprintButton.gameObject.SetActive(false); GameManager.Instance.showSprintSlider = true; StartLevel(); }
    public void ClickShowXpButton() { showXpButton.gameObject.SetActive(false); GameManager.Instance.showXP = true; StartLevel(); }
    public void ClickShowLevelButton() { showLevelButton.gameObject.SetActive(false); StartLevel(); }
    public void ClickShowHealthBarsButton() { showHealthBarsButton.gameObject.SetActive(false); GameManager.Instance.showHealthBars = true; StartLevel(); }
    public void ClickShowTargetReticleButton() { showTargetReticleButton.gameObject.SetActive(false); GameManager.Instance.showTargetReticle = true; StartLevel(); }
    public void ClickShowMiniMapButton() { showMiniMapButton.gameObject.SetActive(false); StartLevel(); }
   
    public void ClickAIMeleeBotButton() { aiMeleeBotButton.gameObject.SetActive(false); aiRangedBotButton.gameObject.SetActive(false); aiBotToActivate.gameObject.SetActive(true); GameManager.Instance.aiMeleeBotActive = true; aiBotToActivate.GetComponent<AIHelperBot>().isMelee = true; aiBotChosen = true; StartLevel();}
    public void ClickAIRangedBotButton() { aiRangedBotButton.gameObject.SetActive(false); aiMeleeBotButton.gameObject.SetActive(false); aiBotToActivate.gameObject.SetActive(true); GameManager.Instance.aiRangedBotActive = true; aiBotToActivate.GetComponent<AIHelperBot>().isRanged = true; aiBotChosen = true; StartLevel(); }
    
    // === Weapon Logic ===
    void SwapWeapon(WeaponType newWeapon)
    {
        if (GameManager.Instance.machineGunActive)
            playerController.GetFireRate /= .35f;

        GameManager.Instance.doubleGunsActive = false;
        GameManager.Instance.machineGunActive = false;
        GameManager.Instance.shotgunActive = false;
        GameManager.Instance.flamethrowerActive = false;

        doubleGunsButton.gameObject.SetActive(true);
        machineGunButton.gameObject.SetActive(true);
        shotgunButton.gameObject.SetActive(true);
        flamethrowerButton.gameObject.SetActive(true);

        switch (newWeapon)
        {
            case WeaponType.DoubleGuns:
                GameManager.Instance.doubleGunsActive = true;
                doubleGunsButton.gameObject.SetActive(false);
                break;
            case WeaponType.MachineGun:
                GameManager.Instance.machineGunActive = true;
                playerController.GetFireRate *= .35f;
                machineGunButton.gameObject.SetActive(false);
                break;
            case WeaponType.Shotgun:
                GameManager.Instance.shotgunActive = true;
                shotgunButton.gameObject.SetActive(false);
                break;
            case WeaponType.Flamethrower:
                GameManager.Instance.flamethrowerActive = true;
                flamethrowerButton.gameObject.SetActive(false);
                break;
        }
    }

    void BuyWeapon(WeaponType weaponType, ref bool purchasedFlag, float price, TextMeshProUGUI priceText)
    {
        if (purchasedFlag)
        {
            SwapWeapon(weaponType);
            StartLevel();
            return;
        }

        if (playerController.GetCash < price) return;

        playerController.GetCash -= price;
        purchasedFlag = true;
        priceText.text = "";
        SwapWeapon(weaponType);
        StartLevel();
    }

    public void BuyDoubleGunsButton() => BuyWeapon(WeaponType.DoubleGuns, ref doubleGunsPurchased, doubleGunsPrice, doubleGunPriceText);
    public void BuyMachineGun() => BuyWeapon(WeaponType.MachineGun, ref machineGunPurchased, machineGunPrice, machineGunPriceText);
    public void BuyShotGunButton() => BuyWeapon(WeaponType.Shotgun, ref shotgunPurchased, shotgunPrice, shotgunPriceText);
    public void BuyFlamethrower() => BuyWeapon(WeaponType.Flamethrower, ref flamethrowerPurchased, flamethrowerPrice, flameThrowerPriceText);

    void StartLevel()
    {
        GameManager.Instance.StartLevel();
        upgradesPanel.SetActive(false);
    }

    void AbilitySelect()
    {
        if (TutorialManager.Instance != null && TutorialManager.Instance.isTutorial) 
        {
            var currentPage = TutorialManager.Instance.GetTutorialPages[TutorialManager.Instance.GetCurrentPageIndex];

            if (currentPage.header == "Ability Select")
            {
                TutorialManager.Instance.CompleteStep();
            }
        }
    }

    private void HandleLevelEnd()
    {
        // Check if we should wait for an ad before upgrades
        if (LevelPlayAds.Instance != null && LevelPlayAds.Instance.IsAdPlaying())
        {
            Debug.Log("Ad is currently playing → will show upgrades after ad.");
            LevelPlayAds.Instance.OnAnyAdClosed += ShowUpgradesAfterAd;
        }
        else
        {
            Debug.Log("No ad playing → showing upgrades now.");
            RandomizeUpgrades();
        }
    }

    private void ShowUpgradesAfterAd()
    {
        Debug.Log("Ad finished → showing upgrades.");
        LevelPlayAds.Instance.OnAnyAdClosed -= ShowUpgradesAfterAd;
        RandomizeUpgrades();
    }
}
