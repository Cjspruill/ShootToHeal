using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] public float damage;
    [SerializeField] public float shootToHeal;
    [SerializeField] public PlayerController playerController;

    public void OnCollisionEnter(Collision other)
    {
        Health health = other.gameObject.GetComponent<Health>();
        EnemyController enemyController = other.gameObject.GetComponent<EnemyController>();

        if (health != null)
            health.TakeDamage(damage);
        

        if (playerController)       
            playerController.GetComponent<Health>().GiveHealth(shootToHeal);
        

        if (enemyController)
            enemyController.PlayHurtAudio();

        Destroy(gameObject);
    }
}
