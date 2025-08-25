using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] public float damage;
    [SerializeField] public float shootToHeal;
    [SerializeField] public PlayerController playerController;
    [SerializeField] GameObject explosionPrefab;

    public void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.GetComponent<Projectile>()) return;

        Health health = other.gameObject.GetComponent<Health>();
        EnemyController enemyController = other.gameObject.GetComponent<EnemyController>();

        if (health != null)
            health.TakeDamage(damage);
        

        if (playerController)       
            playerController.GetComponent<Health>().GiveHealth(shootToHeal);
        

        if (enemyController)
            enemyController.PlayHurtAudio();

        Instantiate(explosionPrefab, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}
