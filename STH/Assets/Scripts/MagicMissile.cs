using Unity.Burst.Intrinsics;
using UnityEngine;

/// <summary>
/// Seeking magic missile. Shares the same damage/heal/knockback contract
/// as Projectile so it works with the existing Health and EnemyController systems.
/// Steering is done by rotating the Rigidbody velocity toward the nearest enemy
/// each frame — no nav mesh required.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class MagicMissile : MonoBehaviour
{


    // ── Set by StaffWeapon on spawn ───────────────────────────────────
    [HideInInspector] public float damage;
    [HideInInspector] public float shootToHeal;
    [HideInInspector] public float bulletPushback;
    [HideInInspector] public PlayerController playerController;

    // ── Tuning ────────────────────────────────────────────────────────
    [SerializeField] float speed = 18f;           // travel speed
    [SerializeField] float turnSpeed = 220f;      // degrees per second toward target
    [SerializeField] float seekRange = 60f;       // how far it looks for enemies
    [SerializeField] float lifetime = 6f;         // self-destruct timer

    [SerializeField] GameObject explosionPrefab;  // reuse your existing explosion

    Rigidbody rb;
    Transform target;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.linearDamping = 0f;
        Destroy(gameObject, lifetime);
    }

    void Start()
    {
        // Give the missile its initial velocity so it doesn't spawn stationary
        rb.linearVelocity = transform.forward * speed;
        FindTarget();
    }

    void FixedUpdate()
    {
        // Re-acquire if target dies or moves out of range
        if (target == null || Vector3.Distance(transform.position, target.position) > seekRange)
            FindTarget();

        if (target != null)
            SteerToward(target.position);

        // Keep constant speed
        rb.linearVelocity = rb.linearVelocity.normalized * speed;
    }

    void FindTarget()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, seekRange);
        float closest = Mathf.Infinity;
        target = null;

        foreach (Collider hit in hits)
        {
            if (hit.GetComponent<EnemyController>() != null)
            {
                float dist = Vector3.Distance(transform.position, hit.transform.position);
                if (dist < closest)
                {
                    closest = dist;
                    target = hit.transform;
                }
            }
        }
    }

    void SteerToward(Vector3 targetPos)
    {
        Vector3 desiredDir = (targetPos - transform.position).normalized;
        Vector3 newDir = Vector3.RotateTowards(
            rb.linearVelocity.normalized,
            desiredDir,
            turnSpeed * Mathf.Deg2Rad * Time.fixedDeltaTime,
            0f
        );
        rb.linearVelocity = newDir * speed;

        // Rotate the visual to match travel direction
        if (rb.linearVelocity.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(rb.linearVelocity);
    }

    void OnCollisionEnter(Collision other)
    {
        if (other.transform.root.GetComponentInChildren<MagicMissile>() != null) return;
        // Don't collide with other missiles, bots, or the player
        if (other.gameObject.GetComponent<MagicMissile>() != null) return;
        if (other.gameObject.GetComponent<Projectile>() != null) return;
        if (other.gameObject.GetComponent<AIHelperBot>() != null) return;
        if (other.gameObject.GetComponent<PlayerController>() != null) return;

        Health health = other.gameObject.GetComponent<Health>();
        EnemyController enemyController = other.gameObject.GetComponent<EnemyController>();
        Rigidbody targetRb = other.gameObject.GetComponent<Rigidbody>();

        if (health != null)
            health.TakeDamage(damage);

        if (playerController != null)
            playerController.GetComponent<Health>().GiveHealth(shootToHeal);

        if (enemyController != null)
            enemyController.PlayHurtAudio();

        // Knockback — same formula as Projectile
        if (targetRb != null && enemyController != null && bulletPushback > 0)
        {
            float pushDur;
            if (bulletPushback < 10)
                pushDur = 0.05f + (0.10f * bulletPushback);
            else
                pushDur = 1.05f;

            enemyController.KnockBack(transform.forward, bulletPushback, pushDur);
        }

        if (explosionPrefab != null)
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }

    void SpawnMissile(float damage, float shootToHeal, float bulletKnockback, float yawOffset)
    {
        Quaternion spawnRot = playerController.staffWeapon.staffTip.rotation * Quaternion.Euler(0f, yawOffset, 0f);
        GameObject obj = Instantiate(playerController.staffWeapon.magicMissilePrefab, playerController.staffWeapon.staffTip.position, spawnRot);

        MagicMissile missile = obj.GetComponent<MagicMissile>();
        missile.damage = damage;
        missile.shootToHeal = shootToHeal;
        missile.bulletPushback = bulletKnockback;
        missile.playerController = playerController;

        // Ignore collision with the player specifically
        Physics.IgnoreCollision(
            obj.GetComponent<Collider>(),
            playerController.GetComponent<Collider>()
        );
    }
}