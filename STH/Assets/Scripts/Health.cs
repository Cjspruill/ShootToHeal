using UnityEngine;

public class Health : MonoBehaviour
{
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

    }

    public void TakeDamage(float damage)
    {
        if (!canDamage) return;

        health -= damage;
        canDamage = false;
        damageTimer = 0;

        if (health <= 0)
        {
            Destroy(gameObject);
            //Drop XP
            Debug.Log("Dead AF");
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
}
