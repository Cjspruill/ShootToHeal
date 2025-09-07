using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class Health : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI healthText;
    [SerializeField] Slider healthBarSlider;
    [SerializeField] float health;
    [SerializeField] float maxHealth;

    [SerializeField] float damageTime;
    [SerializeField] float damageTimer;

    [SerializeField] bool canDamage;

    [SerializeField] MeshRenderer meshRenderer;
    [SerializeField] Color origColor;
    [SerializeField] Color damageFlashColor = Color.white;
    [SerializeField] public bool inDamageFlash; 
    [SerializeField] float damageFlashTime = .25f;
    [SerializeField] EnemyTutorial enemyTutorial;
    public float GetHealth { get => health; set => health = value; }
    public float GetMaxHealth { get => maxHealth; set => maxHealth = value; }



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        origColor = meshRenderer.material.color;
        enemyTutorial = GetComponent<EnemyTutorial>();

        if (GetComponent<PlayerController>())
        {
            healthText = GameObject.FindGameObjectWithTag("HealthText").GetComponent<TextMeshProUGUI>();

        if(TutorialManager.Instance != null && TutorialManager.Instance.isTutorial)        
            GetMaxHealth = 1000;
        }

        

        ResetHealth();

        if(healthBarSlider != null)
        {
        healthBarSlider.maxValue = GetMaxHealth;
        healthBarSlider.value = GetHealth;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!canDamage)
        {
            damageTimer += Time.deltaTime;

            if(damageTimer >= damageTime)
            {
                canDamage = true;
                damageTimer = 0;
            }
        }

        if (healthText)
            healthText.text = "Health: " + GetRoundedHealth() + "/ " + GetRoundedMaxHealth();
        

        if (GameManager.Instance.showHealthBars)
        {
            if (healthBarSlider != null)
            {
            healthBarSlider.gameObject.SetActive(true);
            healthBarSlider.value = GetHealth;
            }
        }
    }

    public void TakeDamage(float damage)
    {
        if (!canDamage) return;

        health -= damage;
        canDamage = false;
        damageTimer = 0;
        StartCoroutine(DamageFlash());

        if (health <= 0)
        {
            EnemyController enemyController = GetComponent<EnemyController>();
            PlayerController playerController = GetComponent<PlayerController>();

            if (enemyController)
            {
                // --- ENEMY DEATH ---
                GameManager.Instance.EnemyDestroyed();
                enemyController.DropXpOrb();

                float cashOrbsToDrop = Random.Range(1, 6);
                for (int i = 0; i < cashOrbsToDrop; i++)
                    enemyController.DropCashOrb();

                if (enemyTutorial)
                {
                    if (TutorialManager.Instance != null && TutorialManager.Instance.isTutorial)
                        enemyTutorial.TutorialDeath();
                }

                Destroy(gameObject); // ✅ enemy still gets destroyed
            }
            else if (playerController)
            {
                Debug.Log("Player reached 0 HP. Prompting revive ad...");

                // Pause immediately when player "dies"
                GameManager.Instance.PauseGame();

                if (LevelPlayAds.Instance != null && LevelPlayAds.Instance.CanRevive())
                {
                    LevelPlayAds ads = LevelPlayAds.Instance;
                    ads.adsPanel.SetActive(true);
                    ads.adsHeaderText.text = $"Watch an ad to revive with 50 health? ({ads.maxRevives - ads.reviveCount} left)";

                    ads.yesButton.onClick.RemoveAllListeners();
                    ads.noButton.onClick.RemoveAllListeners();

                    // YES = revive via ad
                    ads.yesButton.onClick.AddListener(() =>
                    {
                        if (ads.IsHealthAdReady())
                        {
                            ads.OnAnyAdClosed += () =>
                            {
                                Debug.Log("Revive ad finished. Restoring player health.");
                                health = 50f;
                                ads.IncrementReviveCount();
                                ads.adsPanel.SetActive(false);

                                // ✅ Resume game after revive
                                GameManager.Instance.ResumeGame();
                            };

                            ads.ShowHealthAd();
                        }
                        else
                        {
                            Debug.LogWarning("Health ad not ready. Ending game instead.");
                            ads.LoadHealthAd();
                            ads.adsPanel.SetActive(false);

                            // Resume briefly so GameOver flow can run
                            GameManager.Instance.ResumeGame();
                            GameManager.Instance.GameOver();
                            Destroy(gameObject);
                        }
                    });

                    // NO = normal death
                    ads.noButton.onClick.AddListener(() =>
                    {
                        ads.adsPanel.SetActive(false);

                        // Resume briefly so GameOver flow can run
                        GameManager.Instance.ResumeGame();
                        GameManager.Instance.GameOver();
                        Destroy(gameObject);
                    });
                }
                else
                {
                    Debug.Log("No revives left or LevelPlayAds missing. Game over.");
                    GameManager.Instance.ResumeGame();
                    GameManager.Instance.GameOver();
                    Destroy(gameObject);
                }
            }
        }
    }

    public void GiveHealth(float value)
    {
        health += value;

        if (health >= GetMaxHealth)
        {
            health = GetMaxHealth;
        }
    }

    void ResetHealth() 
    {
        health = GetMaxHealth;
    }

    public int GetRoundedHealth()
    {
        int roundedHealth = Mathf.RoundToInt(health);
        return roundedHealth;
    }
    public int GetRoundedMaxHealth()
    {
        int roundedMaxHealth = Mathf.RoundToInt(GetMaxHealth);
        return roundedMaxHealth;
    }

    IEnumerator DamageFlash()
    {
        meshRenderer.material.color = damageFlashColor;
        inDamageFlash = true;
        yield return new WaitForSeconds(damageFlashTime);
        inDamageFlash = false;
        meshRenderer.material.color = origColor;
    }
}
