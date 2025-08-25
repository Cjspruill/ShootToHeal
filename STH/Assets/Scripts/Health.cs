using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI healthText;
    [SerializeField] Slider healthBarSlider;
    [SerializeField] float health;
    [SerializeField] float maxHealth;

    [SerializeField] float damageTime;
    [SerializeField] float damageTimer;

    [SerializeField] bool canDamage;

    public float GetHealth { get => health; set => health = value; }
    public float GetMaxHealth { get => maxHealth; set => maxHealth = value; }

   

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
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

        if (health <= 0)
        {
            EnemyController enemyController = GetComponent<EnemyController>();
            if (enemyController)
            {
                GameManager.Instance.EnemyDestroyed();
                enemyController.DropXpOrb();
            }
            Destroy(gameObject);
            //Drop XP
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
}
