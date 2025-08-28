using UnityEngine;
using UnityEngine.AI;
using System.Collections;
public class EnemyController : MonoBehaviour
{
    [SerializeField] NavMeshAgent navMeshAgent;

    [SerializeField] float moveSpeed = 5f;            // normal movement speed
    [SerializeField] float sprintSpeed = 10f;         // sprint movement speed
    [SerializeField] float stoppingDistance = 2f;
    [SerializeField] float rotationSpeed = 10f;
    [SerializeField] float knockbackForce;

    [SerializeField] float sprintDuration = 2f;       // how long the enemy sprints
    [SerializeField] float minSprintCooldown = 3f;    // min wait before next sprint
    [SerializeField] float maxSprintCooldown = 7f;    // max wait before next sprint
    [SerializeField] float chanceToSprint;
    float sprintTimer = 0f;
    float sprintCooldownTimer = 0f;
    bool isSprinting = false;

    [SerializeField] float damage;
    Transform target;

    [SerializeField] float amountOfXpToDrop;
    [SerializeField] float amountOfCashToDropPerOrb;
    [SerializeField] GameObject xpOrb;
    [SerializeField] GameObject cashOrb;

    [SerializeField] GameObject audioPrefab;

    [SerializeField] MeshRenderer meshRenderer;
    [SerializeField] Color origColor;
    [SerializeField] Color sprintColor;
    [SerializeField] Health health;
    [SerializeField] Rigidbody rb;
    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        meshRenderer = GetComponent<MeshRenderer>();
        health = GetComponent<Health>();
        origColor = meshRenderer.material.color;
        rb= GetComponent<Rigidbody>();
        target = FindFirstObjectByType<PlayerController>().transform;

        // start cooldown timer randomly to stagger sprints
        sprintCooldownTimer = Random.Range(minSprintCooldown, maxSprintCooldown);

        navMeshAgent.speed = moveSpeed;
        navMeshAgent.stoppingDistance = stoppingDistance;
        navMeshAgent.updateRotation = false;
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    void Update()
    {
        if (target == null) return;

        HandleSprinting();

        // set agent speed based on sprint state
        navMeshAgent.speed = isSprinting ? sprintSpeed : moveSpeed;

        // always move directly to the player
        if(navMeshAgent.enabled)
        navMeshAgent.SetDestination(target.position);

        // smooth rotation directly towards the player
        Vector3 direction = (target.position - transform.position).normalized;
        direction.y = 0f; // keep upright
        if (direction.sqrMagnitude > 0.001f) // prevent NaN if too close
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
        }

        // color feedback
        if (isSprinting)
        {
            meshRenderer.material.color = sprintColor;
        }
        else if (!health.inDamageFlash)
        {
            meshRenderer.material.color = origColor;
        }
    }

    void HandleSprinting()
    {
        if (isSprinting)
        {
            sprintTimer -= Time.deltaTime;
            if (sprintTimer <= 0f)
            {
                isSprinting = false;
                sprintCooldownTimer = Random.Range(minSprintCooldown, maxSprintCooldown);
            }
        }
        else
        {
            sprintCooldownTimer -= Time.deltaTime;
            if (sprintCooldownTimer <= 0f)
            {
                // Random chance to sprint when cooldown ends
                if (Random.value < chanceToSprint)
                {
                    isSprinting = true;
                    sprintTimer = sprintDuration;
                }
                else
                {
                    sprintCooldownTimer = Random.Range(minSprintCooldown, maxSprintCooldown);
                }
            }
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        PlayerController playerController = collision.gameObject.GetComponent<PlayerController>();

        if (playerController != null)
        {
            // Damage
            Health health = playerController.GetComponent<Health>();
            health.TakeDamage(damage);

            // Knockback
            Vector3 knockbackDir = (collision.transform.position - transform.position).normalized;
            playerController.ApplyKnockback(knockbackDir, knockbackForce);
        }
    }

    public void DropXpOrb()
    {
        GameObject newXpOrb = Instantiate(xpOrb, transform.position, transform.rotation);
        newXpOrb.GetComponent<XpOrb>().xpToGive = amountOfXpToDrop;
    }

    public void PlayHurtAudio()
    {
        GameObject newAudioObj = Instantiate(audioPrefab, transform.position, Quaternion.identity);
        AudioSource newAudio = newAudioObj.GetComponent<AudioSource>();
        newAudio.Play();
        Destroy(newAudioObj, newAudio.clip.length);
    }

    public void DropCashOrb()
    {
        GameObject newCashOrb = Instantiate(cashOrb, transform.position, transform.rotation);
        newCashOrb.GetComponent<CashOrb>().cashAmountToGive = amountOfCashToDropPerOrb;

        Rigidbody rb = newCashOrb.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Random direction in a sphere but mostly outward
            Vector3 randomDirection = new Vector3(
                Random.Range(-1f, 1f),
                Random.Range(0.5f, 1.5f), // bias upward
                Random.Range(-1f, 1f)
            ).normalized;

            float force = Random.Range(3f, 6f); // tweak these values for strength
            rb.AddForce(randomDirection * force, ForceMode.Impulse);

            // Optional: add a little spin
            Vector3 randomTorque = new Vector3(
                Random.Range(-180f, 180f),
                Random.Range(-180f, 180f),
                Random.Range(-180f, 180f)
            );
            rb.AddTorque(randomTorque);
        }
    }

    public void KnockBack(Vector3 dir, float force, float duration)
    {
        StartCoroutine(ApplyKnockback(dir, force, duration));
    }

    IEnumerator ApplyKnockback(Vector3 direction, float force, float duration)
    {
        // Disable NavMeshAgent so it doesn’t fight physics
        if (navMeshAgent != null) navMeshAgent.enabled = false;

        rb.isKinematic = false; // ensure knockback works
        rb.linearVelocity = direction.normalized * force;

        yield return new WaitForSeconds(duration);

        rb.linearVelocity = Vector3.zero; // stop sliding
        rb.isKinematic = true;      // go back to navmesh control

        if (navMeshAgent != null)
        {
            navMeshAgent.enabled = true;
            navMeshAgent.nextPosition = transform.position; // sync agent to Rigidbody
        }
    }
}