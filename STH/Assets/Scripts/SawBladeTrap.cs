using System.Collections;
using UnityEngine;

/// <summary>
/// Saw Blade Trap.
/// Triggering the pad launches a wave of saw blades from random
/// off-screen directions. They fly across the arena damaging
/// whoever they hit (both player and enemies).
///
/// Setup:
///  - Place this script on a trigger collider (the pad)
///  - Assign sawBladePrefab (needs SawBladeProjectile.cs)
///  - spawnRadius should be larger than your arena
/// </summary>
public class SawBladeTrap : TrapBase
{
    [Header("Saw Blade Settings")]
    [SerializeField] GameObject sawBladePrefab;
    [SerializeField] int bladeCount = 5;
    [SerializeField] float bladeSpeed = 20f;
    [SerializeField] float spawnRadius = 60f;       // how far off-screen blades spawn
    [SerializeField] float spawnInterval = 0.15f;   // delay between each blade
    [SerializeField] float bladeDamage = 30f;
    [SerializeField] bool damagesBothSides = true;  // saw blades hurt everyone

    protected override void Activate(GameObject trigger, bool triggeredByPlayer)
    {
        PlayAudio();
        StartCoroutine(SpawnBladeWave());
    }

    IEnumerator SpawnBladeWave()
    {
        StartCooldown();

        for (int i = 0; i < bladeCount; i++)
        {
            SpawnBlade();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnBlade()
    {
        if (sawBladePrefab == null) return;

        // Random angle around the arena
        float angle = Random.Range(0f, 360f);
        Vector3 spawnDir = new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad), 0f, Mathf.Cos(angle * Mathf.Deg2Rad));
        Vector3 spawnPos = transform.position + spawnDir * spawnRadius;
        spawnPos.y = Random.Range(0.5f, 2f); // vary height slightly

        // Travel direction is inward (plus slight random offset so blades fan out)
        float spread = Random.Range(-15f, 15f);
        Vector3 travelDir = Quaternion.Euler(0, spread, 0) * (-spawnDir);

        Quaternion rot = Quaternion.LookRotation(travelDir);
        GameObject blade = Instantiate(sawBladePrefab, spawnPos, rot);

        SawBladeProjectile proj = blade.GetComponent<SawBladeProjectile>();
        if (proj != null)
        {
            proj.damage          = bladeDamage;
            proj.speed           = bladeSpeed;
            proj.damagesBothSides = damagesBothSides;
        }

        // Safety destroy if it flies off the other side
        Destroy(blade, spawnRadius * 2f / bladeSpeed + 1f);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
}


/// <summary>
/// Attach to the saw blade prefab. Moves forward, rotates visually,
/// and damages whatever it hits.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class SawBladeProjectile : MonoBehaviour
{
    [HideInInspector] public float damage;
    [HideInInspector] public float speed;
    [HideInInspector] public bool damagesBothSides = true;

    [SerializeField] float spinSpeed = 720f;         // visual spin degrees/sec
    [SerializeField] GameObject hitFX;               // optional sparks prefab

    Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true; // we move it manually for predictable travel
    }

    void Start()
    {
        rb.isKinematic = false;
        rb.linearVelocity = transform.forward * speed;
    }

    void Update()
    {
        // Spin the blade visually
        transform.Rotate(0f, 0f, spinSpeed * Time.deltaTime, Space.Self);
    }

    void OnCollisionEnter(Collision other)
    {
        // Ignore other saw blades
        if (other.gameObject.GetComponent<SawBladeProjectile>() != null) return;

        bool isPlayer = other.gameObject.GetComponent<PlayerController>() != null;
        bool isEnemy  = other.gameObject.GetComponent<EnemyController>()  != null;

        if (!damagesBothSides)
        {
            // If damagesBothSides is false, only hurt enemies
            if (isPlayer) return;
        }

        Health health = other.gameObject.GetComponent<Health>();
        if (health != null)
            health.TakeDamage(damage);

        EnemyController ec = other.gameObject.GetComponent<EnemyController>();
        if (ec != null)
            ec.PlayHurtAudio();

        if (hitFX != null)
            Instantiate(hitFX, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}
