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


    [SerializeField] PlayerController playerController;

    [SerializeField] Button[] upgradeButtons;
    [SerializeField] Button showXpButton;
    [SerializeField] Button showLevelButton;
    [SerializeField] Button showSprintButton;
    [SerializeField] Button showHealthBarsButton;
    [SerializeField] Button doubleGunsButton;
    [SerializeField] Button shotgunButton;

    [SerializeField] GameObject upgradesPanel;

    [SerializeField] float doubleGunsPrice = 5;
    [SerializeField] float shotgunPrice = 10;


    [SerializeField] TextMeshProUGUI doubleGunPriceText;
    [SerializeField] TextMeshProUGUI shotgunPriceText;

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
        sprintCooldownToGive = UnityEngine.Random.Range(sprintCooldownToGiveMin,sprintCooldownToGiveMax);
        sprintMultiplierToGive = UnityEngine.Random.Range(sprintMultiplierToGiveMin, sprintMultiplierToGiveMax);
        rotationSpeedToGive = UnityEngine.Random.Range(rotationSpeedToGiveMin, rotationSpeedToGiveMax);
        shootToHealToGive = UnityEngine.Random.Range(shootToHealToGiveMin,shootToHealToGiveMax);


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

        // list of upgrades with their names using concatenation
        List<(Action action, string name)> allUpgrades = new List<(Action, string)>
    {
        (GiveHealthToPlayer, "+" + healthRounded + " Health"),
        (GiveViewRangeToPlayer, "+" + viewRangeRounded + " View Range"),
        (GiveMoveSpeedToPlayer, "+" + moveSpeedRounded + " Move Speed"),
        (GiveEnemyDetectionRangeToPlayer, "+" + enemyDetectionRangeRounded + " Enemy Detection"),
        (GiveBulletDamageToPlayer, "+" + bulletDamageRounded + " Bullet Damage"),
        (GiveFireRateToPlayer, "+" + fireRateRounded + " Fire Rate"),
        (GiveSprintTimeToPlayer, "+" + sprintTimeRounded + " Sprint Time"),
        (GiveSprintCooldownToPlayer, "+" + sprintCooldownRounded + " Sprint Cooldown"),
        (GiveSprintMultiplierToPlayer, "+" + sprintMultiplierRounded + " Sprint Multiplier"),
        (GiveRotationSpeedToPlayer, "+" + rotationSpeedRounded + " Rotation Speed"),
        (GiveShootToHealToPlayer, "+" + shootToHealRounded + " Shoot To Heal")
    };

        // clear old listeners
        foreach (Button btn in upgradeButtons)
        {
            btn.onClick.RemoveAllListeners();
        }

        // assign 3 unique upgrades to the buttons
        for (int i = 0; i < upgradeButtons.Length; i++)
        {
            int randIndex = UnityEngine.Random.Range(0, allUpgrades.Count);
            var chosenUpgrade = allUpgrades[randIndex];

            // set button text
            TextMeshProUGUI btnText = upgradeButtons[i].GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null)
                btnText.text = chosenUpgrade.name;

            // capture local copy to avoid closure issues
            Action upgradeCopy = chosenUpgrade.action;
            upgradeButtons[i].onClick.AddListener(() => upgradeCopy());

            // remove from list so it cannot repeat
            allUpgrades.RemoveAt(randIndex);
        }

        doubleGunPriceText.text = "$ " + doubleGunsPrice;
        shotgunPriceText.text = "$ " + shotgunPrice;
    }
    void GiveHealthToPlayer()
    {
        playerController.GetMaxHealth += healthToGive;
        playerController.UpdateHealth(healthToGive);
        playerController.UpdateMaxHealth();
        StartLevel();
    }

    void GiveViewRangeToPlayer()
    {
        playerController.GetCameraViewDistance += viewRangeToGive;
        StartLevel();
    }

    void GiveMoveSpeedToPlayer()
    {
        playerController.GetMoveSpeed += moveSpeedToGive;
        StartLevel();
    }

    void GiveEnemyDetectionRangeToPlayer()
    {
        playerController.GetEnemyDetectionRange += enemyDetectionRangeToGive;
        StartLevel();
    }

    void GiveBulletDamageToPlayer()
    {
        playerController.GetBulletDamage += bulletDamageToGive;
        StartLevel();
    }

    void GiveFireRateToPlayer()
    {
        playerController.GetFireRate -= fireRateToGive;
        StartLevel();
    }

    void GiveSprintTimeToPlayer()
    {
        playerController.GetSprintTime += sprintTimeToGive;
        StartLevel();
    }

    void GiveSprintCooldownToPlayer()
    {
        playerController.GetSprintCooldown -= sprintCooldownToGive;
        StartLevel();
    }
    
    void GiveSprintMultiplierToPlayer()
    {
        playerController.GetSprintMultiplier += sprintMultiplierToGive;
        StartLevel();
    }

    void GiveRotationSpeedToPlayer()
    {
        playerController.GetRotationSpeed += rotationSpeedToGive;
        StartLevel();
    }

    void GiveShootToHealToPlayer()
    {
        playerController.GetShootToHeal += shootToHealToGive;
        StartLevel();
    }

    public void ClickShowSprintButton()
    {
        showSprintButton.gameObject.SetActive(false);
        GameManager.Instance.showSprintSlider = true;
        StartLevel();
    }

    public void ClickShowXpButton()
    {
        showXpButton.gameObject.SetActive(false);
        GameManager.Instance.showXP = true;
        StartLevel();
    }

    public void ClickShowLevelButton()
    {
        showLevelButton.gameObject.SetActive(false);
        StartLevel();
    }

    public void ClickShowHealthBarsButton()
    {
        showHealthBarsButton.gameObject.SetActive(false);
        GameManager.Instance.showHealthBars = true;
        StartLevel();
    }

    public void BuyDoubleGunsButton()
    {
        if (playerController.GetCash < doubleGunsPrice) return;

        playerController.GetCash -= doubleGunsPrice;

        if (!shotgunButton.isActiveAndEnabled)
            shotgunButton.gameObject.SetActive(true);
        if(GameManager.Instance.shotgunActive)
            GameManager.Instance.shotgunActive = false;

        doubleGunsButton.gameObject.SetActive(false);
        GameManager.Instance.doubleGunsActive = true;
        StartLevel();
    }

    public void BuyShotGunButton()
    {
        if (playerController.GetCash < shotgunPrice) return;

        playerController.GetCash -= shotgunPrice;

        if (!doubleGunsButton.isActiveAndEnabled)
            doubleGunsButton.gameObject.SetActive(true);
        if(GameManager.Instance.doubleGunsActive)
            GameManager.Instance.doubleGunsActive = false;

        shotgunButton.gameObject.SetActive(false);
        GameManager.Instance.shotgunActive = true;
        StartLevel();
    }

    void StartLevel()
    {
        GameManager.Instance.StartLevel();
        upgradesPanel.SetActive(false);
    }
}
