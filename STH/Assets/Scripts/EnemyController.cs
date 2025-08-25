using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    [SerializeField] NavMeshAgent navMeshAgent;

    [SerializeField] float moveSpeed = 5f;            // normal movement speed
    [SerializeField] float sprintSpeed = 10f;         // sprint movement speed
    [SerializeField] float stoppingDistance = 2f;
    [SerializeField] float rotationSpeed = 10f;
    [SerializeField] float separationRadius = 2f;
    [SerializeField] float separationForce = 3f;
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
    [SerializeField] GameObject xpOrb;

    [SerializeField] GameObject audioPrefab;

    [SerializeField] MeshRenderer meshRenderer;
    [SerializeField] Color origColor;
    [SerializeField] Color sprintColor;


    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        meshRenderer = GetComponent<MeshRenderer>();
        origColor = meshRenderer.material.color;

        target = FindFirstObjectByType<PlayerController>().transform;

        // start cooldown timer randomly to stagger sprints
        sprintCooldownTimer = Random.Range(minSprintCooldown, maxSprintCooldown);

        navMeshAgent.speed = moveSpeed;
        navMeshAgent.stoppingDistance = stoppingDistance;
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
        navMeshAgent.SetDestination(target.position);

        // optional: smooth rotation towards velocity
        if (navMeshAgent.velocity.sqrMagnitude > 0.1f)
        {
            Quaternion lookRotation = Quaternion.LookRotation(navMeshAgent.velocity);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
        }

        // color feedback
        meshRenderer.material.color = isSprinting ? sprintColor : origColor;
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

    private void OnCollisionEnter(Collision collision)
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
}