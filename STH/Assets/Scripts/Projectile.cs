using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] public float damage;
    [SerializeField] public PlayerController playerController;

    public void OnCollisionEnter(Collision other)
    {
        Health health = other.gameObject.GetComponent<Health>();
        EnemyController enemyController = other.gameObject.GetComponent<EnemyController>();

        if (health != null)
            health.TakeDamage(damage);
        

        if (playerController)       
            playerController.GetComponent<Health>().GiveHealth(1);
        

        if (enemyController)
            enemyController.PlayHurtAudio();

        Destroy(gameObject);
    }
}
