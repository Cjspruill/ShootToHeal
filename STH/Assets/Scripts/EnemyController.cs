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

    // ── Power-Up Drop Settings ────────────────────────────────────────
    [Header("Power-Up Drops")]
    [SerializeField] GameObject positiveOrbPrefab;   // green sphere prefab with PowerUpOrb (Positive)
    [SerializeField] GameObject negativeOrbPrefab;   // red sphere prefab with PowerUpOrb (Negative)

    // Positive drop rate starts at 3-5% and DECREASES over time (max cap 9%)
    private const float POS_RATE_BASE_MIN = 0.03f;
    private const float POS_RATE_BASE_MAX = 0.05f;
    private const float POS_RATE_CAP = 0.09f;

    // Negative drop rate starts at 5-7% and INCREASES over time (max cap 10%)
    private const float NEG_RATE_BASE_MIN = 0.05f;
    private const float NEG_RATE_BASE_MAX = 0.07f;
    private const float NEG_RATE_CAP = 0.10f;

    // How many seconds until rates reach their extremes (default 10 minutes)
    [SerializeField] float dropRateRampTime = 600f;

    // ── Freeze state (set by PowerUpManager freeze shot) ─────────────
    bool isFrozen = false;

    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        meshRenderer = GetComponent<MeshRenderer>();
        health = GetComponent<Health>();
        origColor = meshRenderer.material.color;
        rb = GetComponent<Rigidbody>();
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

        // Don't move or act while frozen
        if (isFrozen) return;

        HandleSprinting();

        // set agent speed based on sprint state
        navMeshAgent.speed = isSprinting ? sprintSpeed : moveSpeed;

        // always move directly to the player
        if (navMeshAgent.enabled)
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

            float force = Random.Range(3f, 6f);
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

    /// <summary>
    /// Rolls drop chances and spawns positive/negative orbs on death.
    /// Called from Health.cs in the enemy death block.
    /// </summary>
    public void DropPowerOrbs()
    {
        float elapsed = Time.timeSinceLevelLoad;
        float t = Mathf.Clamp01(elapsed / dropRateRampTime);

        // Positive rate: starts at 3-5%, DECREASES toward 0 as time goes on (capped at 9%)
        float basePosRate = Random.Range(POS_RATE_BASE_MIN, POS_RATE_BASE_MAX);
        float posRate = Mathf.Lerp(basePosRate, 0f, t);
        posRate = Mathf.Clamp(posRate, 0f, POS_RATE_CAP);

        // Negative rate: starts at 5-7%, INCREASES toward cap as time goes on
        float baseNegRate = Random.Range(NEG_RATE_BASE_MIN, NEG_RATE_BASE_MAX);
        float negRate = Mathf.Lerp(baseNegRate, NEG_RATE_CAP, t);
        negRate = Mathf.Clamp(negRate, NEG_RATE_BASE_MIN, NEG_RATE_CAP);

        if (Random.value < posRate && positiveOrbPrefab != null)
            Instantiate(positiveOrbPrefab, transform.position + Vector3.up, Quaternion.identity);

        if (Random.value < negRate && negativeOrbPrefab != null)
            Instantiate(negativeOrbPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
    }

    /// <summary>
    /// Called by PowerUpManager to temporarily boost or reduce enemy nav speed.
    /// Pass a positive value to speed up, negative to slow/undo.
    /// </summary>
    public void BoostMoveSpeed(float amount)
    {
        moveSpeed += amount;
        sprintSpeed += amount;

        // Keep speeds from going negative
        moveSpeed = Mathf.Max(moveSpeed, 0.5f);
        sprintSpeed = Mathf.Max(sprintSpeed, 0.5f);

        // Immediately apply to the agent if not currently frozen or knockedback
        if (navMeshAgent != null && navMeshAgent.enabled)
            navMeshAgent.speed = isSprinting ? sprintSpeed : moveSpeed;
    }

    /// <summary>
    /// Freezes the enemy in place for a set duration.
    /// Triggered when freezeShotActive is true and the enemy is hit.
    /// </summary>
    public void FreezeForDuration(float duration)
    {
        if (!isFrozen)
            StartCoroutine(FreezeRoutine(duration));
    }

    IEnumerator FreezeRoutine(float duration)
    {
        isFrozen = true;

        if (navMeshAgent != null && navMeshAgent.enabled)
            navMeshAgent.isStopped = true;

        // Flash the enemy blue to signal freeze
        Color frozenColor = Color.blue;
        meshRenderer.material.color = frozenColor;

        yield return new WaitForSeconds(duration);

        isFrozen = false;

        if (navMeshAgent != null && navMeshAgent.enabled)
            navMeshAgent.isStopped = false;

        meshRenderer.material.color = origColor;
    }

    public void KnockBack(Vector3 dir, float force, float duration = 0f)
    {
        StartCoroutine(ApplyKnockback(dir, force, duration));
    }

    IEnumerator ApplyKnockback(Vector3 direction, float force, float duration)
    {
        // Disable NavMeshAgent so it doesn't fight physics
        if (navMeshAgent != null) navMeshAgent.enabled = false;

        rb.isKinematic = false; // ensure knockback works
        rb.linearVelocity = direction.normalized * force;

        yield return new WaitForSeconds(duration);

        rb.linearVelocity = Vector3.zero; // stop sliding
        rb.isKinematic = true;            // go back to navmesh control

        if (navMeshAgent != null)
        {
            navMeshAgent.enabled = true;
            navMeshAgent.nextPosition = transform.position; // sync agent to Rigidbody
        }
    }
}