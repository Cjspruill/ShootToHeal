using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] public float damage;
    [SerializeField] public float shootToHeal;
    [SerializeField] public float bulletPushback;
    [SerializeField] public PlayerController playerController;
    [SerializeField] GameObject explosionPrefab;

    public void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.GetComponent<Projectile>() || other.gameObject.GetComponent<AIHelperBot>() || other.gameObject.CompareTag("Player")) return;

        Health health = other.gameObject.GetComponent<Health>();
        EnemyController enemyController = other.gameObject.GetComponent<EnemyController>();
        Rigidbody rigidbody = other.gameObject.GetComponent<Rigidbody>(); 

        if (health != null)
            health.TakeDamage(damage);
        

        if (playerController)       
            playerController.GetComponent<Health>().GiveHealth(shootToHeal);
        

        if (enemyController)
            enemyController.PlayHurtAudio();

        float newPushBackDur = 0f;

        if (bulletPushback <= 0)
        {
            newPushBackDur = 0f; // no pushback
        }
        else if (bulletPushback < 10)
        {
            // starts at 0.15 when bulletPushback = 1
            // increases by 0.1 each integer step
            newPushBackDur = 0.05f + (0.10f * bulletPushback);
        }
        else
        {
            // clamp max at 10 → duration = 1.05f
            newPushBackDur = 1.05f;
        }

        if (rigidbody && enemyController && bulletPushback > 0)
            enemyController.KnockBack(transform.forward, bulletPushback, newPushBackDur);

        Instantiate(explosionPrefab, transform.position, Quaternion.identity);
       

        Destroy(gameObject);
    }
}
