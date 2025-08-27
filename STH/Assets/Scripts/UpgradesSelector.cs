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
    float viewRangeToGive;
    [SerializeField] float viewRangeToGiveMin;
    [SerializeField] float viewRangeToGiveMax;
    float moveSpeedToGive;
    [SerializeField] float moveSpeedToGiveMin;
    [SerializeField] float moveSpeedToGiveMax;
    float enemyDetectionRangeToGive;
    [SerializeField] float enemyDetectionRangeToGiveMin;
    [SerializeField] float enemyDetectionRangeToGiveMax;
    float bulletDamageToGive;
    [SerializeField] float bulletDamageToGiveMin;
    [SerializeField] float bulletDamageToGiveMax;
    float fireRateToGive;
    [SerializeField] float fireRateToGiveMin;
    [SerializeField] float fireRateToGiveMax;
    float sprintTimeToGive;
    [SerializeField] float sprintTimeToGiveMin;
    [SerializeField] float sprintTimeToGiveMax;
    float sprintCooldownToGive;
    [SerializeField] float sprintCooldownToGiveMin;
    [SerializeField] float sprintCooldownToGiveMax;
    float sprintMultiplierToGive;
    [SerializeField] float sprintMultiplierToGiveMin;
    [SerializeField] float sprintMultiplierToGiveMax;
    float rotationSpeedToGive;
    [SerializeField] float rotationSpeedToGiveMin;
    [SerializeField] float rotationSpeedToGiveMax;
    float shootToHealToGive;
    [SerializeField] float shootToHealToGiveMin;
    [SerializeField] float shootToHealToGiveMax;
    float flamethrowerDurationToGive;
    [SerializeField] float flameThrowerDurationMin;
    [SerializeField] float flameThrowerDurationMax;

    [SerializeField] PlayerController playerController;

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

    [SerializeField] GameObject upgradesPanel;

    [SerializeField] float doubleGunsPrice = 5;
    [SerializeField] float machineGunPrice = 5;
    [SerializeField] float shotgunPrice = 10;
    [SerializeField] float flamethrowerPrice = 10;

    [SerializeField] bool doubleGunsPurchased;
    [SerializeField] bool machineGunPurchased;
    [SerializeField] bool shotgunPurchased;
    [SerializeField] bool flamethrowerPurchased;


    [SerializeField] TextMeshProUGUI doubleGunPriceText;
    [SerializeField] TextMeshProUGUI machineGunPriceText;
    [SerializeField] TextMeshProUGUI shotgunPriceText;
    [SerializeField] TextMeshProUGUI flameThrowerPriceText;

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
        GameManager.OnLevelEnd += RandomizeUpgrades;
    }

    private void OnDisable()
    {
        GameManager.OnLevelEnd -= RandomizeUpgrades;
    }

    void RandomizeUpgrades()
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

        float healthRounded = Mathf.Round(healthToGive * 100f) / 100f;
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

        List<(Action action, string name)> allUpgrades = new List<(Action, string)>
{
    (GiveHealthToPlayer, "+" + healthRounded + " Health"),
    (GiveViewRangeToPlayer, "+" + viewRangeRounded + " View Range"),
    (GiveMoveSpeedToPlayer, "+" + moveSpeedRounded + " Move Speed"),
    (GiveEnemyDetectionRangeToPlayer, "+" + enemyDetectionRangeRounded + " Enemy Detection"),
    (GiveBulletDamageToPlayer, "+" + bulletDamageRounded + " Bullet Damage"),
    (GiveFireRateToPlayer, "+" + fireRateRounded + " Fire Rate"),
    (GiveSprintTimeToPlayer, "+" + sprintTimeRounded + " Sprint Time"),
    (GiveSprintMultiplierToPlayer, "+" + sprintMultiplierRounded + " Sprint Multiplier"),
    (GiveRotationSpeedToPlayer, "+" + rotationSpeedRounded + " Rotation Speed"),
    (GiveShootToHealToPlayer, "+" + shootToHealRounded + " Shoot To Heal")
};


        // ✅ Only add SprintCooldown upgrade if player still has cooldown > 0
        if (playerController.GetSprintCooldown > 0)        
            allUpgrades.Add((GiveSprintCooldownToPlayer, "+" + sprintCooldownRounded + " Sprint Cooldown"));
        
        // ✅ Only add Flamethrower Duration if purchased
        if (flamethrowerPurchased)        
            allUpgrades.Add((GiveFlamethrowerDurationToPlayer, "+" + flamethrowerDurationRounded + " Flamethrower Duration"));
        

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

        doubleGunsButton.gameObject.SetActive(!GameManager.Instance.doubleGunsActive);
        machineGunButton.gameObject.SetActive(!GameManager.Instance.machineGunActive);
        shotgunButton.gameObject.SetActive(!GameManager.Instance.shotgunActive);
        flamethrowerButton.gameObject.SetActive(!GameManager.Instance.flamethrowerActive);
    }

    // === Upgrade Handlers ===
    void GiveHealthToPlayer()
    {
        playerController.GetMaxHealth += healthToGive;
        playerController.UpdateHealth(healthToGive);
        playerController.UpdateMaxHealth();
        StartLevel();
    }

    void GiveViewRangeToPlayer() { playerController.GetCameraViewDistance += viewRangeToGive; StartLevel(); }
    void GiveMoveSpeedToPlayer() { playerController.GetMoveSpeed += moveSpeedToGive; StartLevel(); }
    void GiveEnemyDetectionRangeToPlayer() { playerController.GetEnemyDetectionRange += enemyDetectionRangeToGive; StartLevel(); }
    void GiveBulletDamageToPlayer() { playerController.GetBulletDamage += bulletDamageToGive; StartLevel(); }
    void GiveFireRateToPlayer() { playerController.GetFireRate -= fireRateToGive; StartLevel(); }
    void GiveSprintTimeToPlayer() { playerController.GetSprintTime += sprintTimeToGive; StartLevel(); }
    void GiveSprintCooldownToPlayer() { playerController.GetSprintCooldown -= sprintCooldownToGive; StartLevel(); }
    void GiveSprintMultiplierToPlayer() { playerController.GetSprintMultiplier += sprintMultiplierToGive; StartLevel(); }
    void GiveRotationSpeedToPlayer() { playerController.GetRotationSpeed += rotationSpeedToGive; StartLevel(); }
    void GiveShootToHealToPlayer() { playerController.GetShootToHeal += shootToHealToGive; StartLevel(); }
    void GiveFlamethrowerDurationToPlayer() { playerController.GetFlameThrowerDuration += flamethrowerDurationToGive; StartLevel(); }

    // === Info Buttons ===
    public void ClickShowSprintButton() { showSprintButton.gameObject.SetActive(false); GameManager.Instance.showSprintSlider = true; StartLevel(); }
    public void ClickShowXpButton() { showXpButton.gameObject.SetActive(false); GameManager.Instance.showXP = true; StartLevel(); }
    public void ClickShowLevelButton() { showLevelButton.gameObject.SetActive(false); StartLevel(); }
    public void ClickShowHealthBarsButton() { showHealthBarsButton.gameObject.SetActive(false); GameManager.Instance.showHealthBars = true; StartLevel(); }
    public void ClickShowTargetReticleButton() { showTargetReticleButton.gameObject.SetActive(false); GameManager.Instance.showTargetReticle = true; StartLevel(); }
    public void ClickShowMiniMapButton() { showMiniMapButton.gameObject.SetActive(false); StartLevel(); }
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
}
