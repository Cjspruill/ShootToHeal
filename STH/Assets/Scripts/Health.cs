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

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        origColor = meshRenderer.material.color;
        enemyTutorial = GetComponent<EnemyTutorial>();

        if (GetComponent<PlayerController>())
        {
            healthText = GameObject.FindGameObjectWithTag("HealthText").GetComponent<TextMeshProUGUI>();

            if (TutorialManager.Instance != null && TutorialManager.Instance.isTutorial)
                GetMaxHealth = 1000;
        }

        ResetHealth();

        if (healthBarSlider != null)
        {
            healthBarSlider.maxValue = GetMaxHealth;
            healthBarSlider.value = GetHealth;
        }
    }

    void Update()
    {
        if (!canDamage)
        {
            damageTimer += Time.deltaTime;

            if (damageTimer >= damageTime)
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

                Destroy(gameObject);
            }
            else if (playerController)
            {
                // --- PLAYER DEATH ---
                // Clamp health to 0 so UI doesn't show negative
                health = 0;

                // Delegate entirely to GameManager which owns lives, coins, and UI routing
                GameManager.Instance.PlayerDied();
            }
        }
    }

    public void GiveHealth(float value)
    {
        health += value;

        if (health >= GetMaxHealth)
            health = GetMaxHealth;
    }

    void ResetHealth()
    {
        health = GetMaxHealth;
    }

    public int GetRoundedHealth()
    {
        return Mathf.RoundToInt(health);
    }

    public int GetRoundedMaxHealth()
    {
        return Mathf.RoundToInt(GetMaxHealth);
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