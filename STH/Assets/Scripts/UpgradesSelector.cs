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
        (GiveFireRateToPlayer, "+" + fireRateToGive + " Fire Rate")
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
        GameManager.Instance.StartLevel();
        upgradesPanel.SetActive(false);
    }

    void GiveViewRangeToPlayer()
    {
        playerController.GetCameraViewDistance += viewRangeToGive;
        GameManager.Instance.StartLevel();
        upgradesPanel.SetActive(false);
    }

    void GiveMoveSpeedToPlayer()
    {
        playerController.GetMoveSpeed += moveSpeedToGive;
        GameManager.Instance.StartLevel();
        upgradesPanel.SetActive(false);
    }

    void GiveEnemyDetectionRangeToPlayer()
    {
        playerController.GetEnemyDetectionRange += enemyDetectionRangeToGive;
        GameManager.Instance.StartLevel();
        upgradesPanel.SetActive(false);
    }

    void GiveBulletDamageToPlayer()
    {
        playerController.GetBulletDamage += bulletDamageToGive;
        GameManager.Instance.StartLevel();
        upgradesPanel.SetActive(false);
    }

    void GiveFireRateToPlayer()
    {
        playerController.GetFireRate -= fireRateToGive;
        GameManager.Instance.StartLevel();
        upgradesPanel.SetActive(false);
    }
}
