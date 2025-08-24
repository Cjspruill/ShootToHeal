using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradesSelector : MonoBehaviour
{
    [SerializeField] float healthToGive;
    [SerializeField] float viewRangeToGive;
    [SerializeField] float moveSpeedToGive;
    [SerializeField] float enemyDetectionRangeToGive;
    [SerializeField] float bulletDamageToGive;
    [SerializeField] float fireRateToGive;
    [SerializeField] float sprintTimeToGive;
    [SerializeField] float sprintCooldownToGive;
    [SerializeField] float sprintMultiplierToGive;
    [SerializeField] float rotationSpeedToGive;
    [SerializeField] float shootToHealToGive;


    [SerializeField] PlayerController playerController;

    [SerializeField] Button[] upgradeButtons;

    [SerializeField] GameObject upgradesPanel;


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
        // list of upgrades with their names using concatenation
        List<(Action action, string name)> allUpgrades = new List<(Action, string)>
    {
        (GiveHealthToPlayer, "+" + healthToGive + " Health"),
        (GiveViewRangeToPlayer, "+" + viewRangeToGive + " View Range"),
        (GiveMoveSpeedToPlayer, "+" + moveSpeedToGive + " Move Speed"),
        (GiveEnemyDetectionRangeToPlayer, "+" + enemyDetectionRangeToGive + " Enemy Detection"),
        (GiveBulletDamageToPlayer, "+" + bulletDamageToGive + " Bullet Damage"),
        (GiveFireRateToPlayer, "+" + fireRateToGive + " Fire Rate"),
        (GiveSprintTimeToPlayer, "+" + sprintTimeToGive + "Sprint Time"),
        (GiveSprintCooldownToPlayer, "-" + sprintCooldownToGive + "Sprint Cooldown"),
        (GiveSprintMultiplierToPlayer, "+" + sprintMultiplierToGive + "Sprint Multiplier"),
        (GiveRotationSpeedToPlayer, "+" + rotationSpeedToGive + "Rotation Speed"),
        (GiveShootToHealToPlayer, "+" + shootToHealToGive + "Shoot To Heal")
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
    }
    void GiveHealthToPlayer()
    {
        playerController.GetMaxHealth += healthToGive;
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

    void StartLevel()
    {
        GameManager.Instance.StartLevel();
        upgradesPanel.SetActive(false);
    }
}
