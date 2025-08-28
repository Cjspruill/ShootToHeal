using UnityEngine;

public class AIHelperBotProjectile : MonoBehaviour
{
    [SerializeField] public float damage;
    [SerializeField] GameObject explosionPrefab;

    public void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.GetComponent<AIHelperBotProjectile>() || other.gameObject.GetComponent<PlayerController>()) return;

        Health health = other.gameObject.GetComponent<Health>();
        EnemyController enemyController = other.gameObject.GetComponent<EnemyController>();

        if (health != null)
            health.TakeDamage(damage);

        if (enemyController)
            enemyController.PlayHurtAudio();

        Instantiate(explosionPrefab, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}
