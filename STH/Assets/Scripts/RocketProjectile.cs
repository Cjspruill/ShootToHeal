using UnityEngine;

public class RocketProjectile : MonoBehaviour
{
    [SerializeField] public float damage;
    [SerializeField] public float shootToHeal;
    [SerializeField] public float bulletPushback;
    [SerializeField] public PlayerController playerController;
    [SerializeField] GameObject explosionPrefab;

    [SerializeField] public LayerMask damageableLayers;
    [SerializeField] public float explosionRadius;
    [SerializeField] public float explosionPower;

    public void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.GetComponent<RocketProjectile>() || other.gameObject.GetComponent<AIHelperBot>()) return;


        other.gameObject.GetComponent<Health>()?.TakeDamage(damage);

        playerController?.GetComponent<Health>()?.GiveHealth(shootToHeal);

        EnemyController enemyController = other.gameObject.GetComponent<EnemyController>();
        enemyController?.PlayHurtAudio();

        float newPushBackDur = 0f;
        if (bulletPushback <= 0f)
        {
            newPushBackDur = 0f; // no pushback
        }
        else if (bulletPushback < 10f)
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
        if (other.gameObject.GetComponent<Rigidbody>() && enemyController && bulletPushback > 0f)
            enemyController.KnockBack(transform.forward, bulletPushback, newPushBackDur);

        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius, damageableLayers);
        foreach (Collider collider in colliders)
        {
            if (collider.TryGetComponent<Rigidbody>(out Rigidbody rb))
                rb.AddExplosionForce(explosionPower, transform.position, explosionRadius);
        }

        Instantiate(explosionPrefab, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}
