using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class AIHelperBot : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float moveSpeed = 3f;
    [SerializeField] float stoppingDistance = 2f;
    [SerializeField] float followPlayerDistance = 1.5f; // distance to keep when following player

    [Header("Attack")]
    [SerializeField] float damage = 10f;
    [SerializeField] float fireRate = 1f;
    float attackTimer;
    [SerializeField] float knockBackForce = 5f;
    [SerializeField] public bool isMelee = false;
    [SerializeField] public bool isRanged = false;
    [SerializeField] float meleeDistance;
    [SerializeField] GameObject projectilePrefab;
    [SerializeField] Transform firePoint;

    [Header("Sprint")]
    [SerializeField] float sprintSpeed;
    [SerializeField] float sprintDuration = 2f;
    [SerializeField] float minSprintCooldown = 3f;
    [SerializeField] float maxSprintCooldown = 7f;
    [Range(0f, 1f)][SerializeField] float chanceToSprint = 0.5f;
    [SerializeField] Color sprintColor;
    [SerializeField] Color attackColor;
    Color origColor;

    float sprintTimer = 0f;
    float sprintCooldownTimer = 0f;
    bool isSprinting = false;

    [Header("References")]
    [SerializeField] NavMeshAgent navMeshAgent;
    [SerializeField] Health health;
    [SerializeField] MeshRenderer meshRenderer;

    PlayerController playerController;
    Transform targetEnemy;

    public float GetMoveSpeed { get => moveSpeed; set => moveSpeed = value; }
    public float GetDamage { get => damage; set => damage = value; }
    public float GetFireRate { get => fireRate; set => fireRate = value; }
    public float GetSprintSpeed { get => sprintSpeed; set => sprintSpeed = value; }
    public float GetSprintDuration { get => sprintDuration; set => sprintDuration = value; }

    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        health = GetComponent<Health>();
        meshRenderer = GetComponent<MeshRenderer>();
        origColor = meshRenderer.material.color;

        playerController = FindFirstObjectByType<PlayerController>();

        navMeshAgent.speed = GetMoveSpeed;
        navMeshAgent.stoppingDistance = stoppingDistance;

        // random cooldown to stagger sprint starts
        sprintCooldownTimer = Random.Range(minSprintCooldown, maxSprintCooldown);
    }

    void Update()
    {
        if (playerController == null) return;

        // check player’s current target
        targetEnemy = playerController.GetCurrentTarget;

        if (targetEnemy != null)
        {
            // go after enemy
            navMeshAgent.stoppingDistance = stoppingDistance;
            navMeshAgent.SetDestination(targetEnemy.position);

            HandleSprint();
            HandleAttack();
        }
        else
        {
            // follow player if no target
            navMeshAgent.stoppingDistance = followPlayerDistance;
            navMeshAgent.SetDestination(playerController.transform.position);

            // don’t attack when following
            sprintCooldownTimer -= Time.deltaTime; // still allow sprint visuals while following
            if (sprintCooldownTimer <= 0f && Random.value <= chanceToSprint)
            {
                isSprinting = true;
                navMeshAgent.speed = sprintSpeed;
                meshRenderer.material.color = sprintColor;
            }
        }
    }

    void HandleSprint()
    {
        if (isSprinting)
        {
            sprintTimer += Time.deltaTime;
            if (sprintTimer >= GetSprintDuration)
            {
                // end sprint
                isSprinting = false;
                navMeshAgent.speed = GetMoveSpeed;

                if (!health.inDamageFlash)
                    meshRenderer.material.color = origColor;

                sprintTimer = 0f;
                sprintCooldownTimer = Random.Range(minSprintCooldown, maxSprintCooldown);
            }
        }
        else
        {
            sprintCooldownTimer -= Time.deltaTime;
            if (sprintCooldownTimer <= 0f && Random.value <= chanceToSprint)
            {
                // start sprint
                isSprinting = true;
                navMeshAgent.speed = GetMoveSpeed * 2f; // sprint speed
                meshRenderer.material.color = sprintColor;
            }
        }
    }

    void HandleAttack()
    {
        if (targetEnemy == null) return;

        attackTimer -= Time.deltaTime;
        if (attackTimer > 0f) return;

        float dist = Vector3.Distance(transform.position, targetEnemy.position);

        if (isMelee)
        {
            if (dist <= meleeDistance)
            {
                attackTimer = GetFireRate;
                StartCoroutine(AttackFlash());

                // melee hit
                targetEnemy.GetComponent<Health>().TakeDamage(GetDamage);
                EnemyController enemyController = targetEnemy.GetComponent<EnemyController>();
                if (enemyController != null)
                {
                    Vector3 knockDir = (targetEnemy.position - transform.position).normalized;
                    
                    enemyController.KnockBack(knockDir,knockBackForce,.5f);
                }
            }
        }
        else if (isRanged && projectilePrefab && firePoint)
        {
            float detectionRange = playerController.GetEnemyDetectionRange;

            if (dist <= detectionRange)
            {
                attackTimer = GetFireRate;
                StartCoroutine(AttackFlash());

                // ranged attack
                GameObject proj = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
                Rigidbody prb = proj.GetComponent<Rigidbody>();
                if (prb != null)
                {
                    prb.AddForce(firePoint.forward * 50, ForceMode.Impulse);
                }
                AIHelperBotProjectile p = proj.GetComponent<AIHelperBotProjectile>();
                if (p != null) p.damage = GetDamage;
            }
        }
    }

    IEnumerator AttackFlash()
    {
        meshRenderer.material.color = attackColor;
        yield return new WaitForSeconds(0.2f); // how long it flashes
        if (!isSprinting && !health.inDamageFlash)
        {
            meshRenderer.material.color = origColor;
        }
        else if (isSprinting)
        {
            meshRenderer.material.color = sprintColor;
        }
    }
}
