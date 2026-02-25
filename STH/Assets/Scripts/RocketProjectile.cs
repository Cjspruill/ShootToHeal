using UnityEngine;

public class RocketProjectile : MonoBehaviour
{
    [SerializeField] public float damage;
    [SerializeField] public float shootToHeal;
    [SerializeField] public PlayerController playerController;
    [SerializeField] GameObject explosionPrefab;

    [SerializeField] public LayerMask damageableLayers;
    [SerializeField] public float explosionRadius = 5f;
    [SerializeField] public float explosionPower = 10f;

    public void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.GetComponent<RocketProjectile>() || other.gameObject.GetComponent<AIHelperBot>()) return;

        other.gameObject.GetComponent<Health>()?.TakeDamage(damage);

        playerController?.GetComponent<Health>()?.GiveHealth(shootToHeal);

        other.gameObject.GetComponent<EnemyController>()?.PlayHurtAudio();

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
