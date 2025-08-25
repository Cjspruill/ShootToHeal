using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
using UnityEngine.UIElements;
using UnityEngine.UI;


public class PlayerController : MonoBehaviour
{
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] Transform barrelOut;
    [SerializeField] Transform target;
    [SerializeField] Health health;
    [SerializeField] CinemachineCamera cam;
    [SerializeField] CinemachineFollow camFollow;
    [SerializeField] AudioSource audioSource;
    [SerializeField] UnityEngine.UI.Slider sprintSlider;
    [SerializeField] float bulletForce = 100f;

    [Header("Stats")]
    [SerializeField] float maxHealth;
    [SerializeField] float moveSpeed = 10f;
    [SerializeField] float enemyDetectionRange = 50f;
    [SerializeField] float cameraViewDistance;
    [SerializeField] float bulletDamage;
    [SerializeField] float fireRate;
    [SerializeField] float sprintTime;
    [SerializeField] private float sprintCooldown;
    [SerializeField] float sprintMultiplier;
    [SerializeField] float rotationSpeed;
    [SerializeField] float shootToHeal;

    InputSystem_Actions playerInput;

    [Header("Timers")]
    [SerializeField] float fireRateTimer;
    [SerializeField] float sprintTimer;
    public float sprintCooldownTimer;

    public bool isSprinting;
    [SerializeField] float xp;

    [Header("Knockback")]
    [SerializeField] float knockbackRecoverySpeed = 5f; // how fast the knockback wears off
    Vector3 knockbackVelocity = Vector3.zero;

    public float GetMaxHealth { get => maxHealth; set => maxHealth = value; }
    public float GetCameraViewDistance { get => cameraViewDistance; set => cameraViewDistance = value; }
    public float GetMoveSpeed { get => moveSpeed; set => moveSpeed = value; }
    public float GetEnemyDetectionRange { get => enemyDetectionRange; set => enemyDetectionRange = value; }
    public float GetBulletDamage { get => bulletDamage; set => bulletDamage = value; }
    public float GetFireRate { get => fireRate; set => fireRate = value; }
    public float GetXp { get => xp; set => xp = value; }
    public float GetSprintTime { get => sprintTime; set => sprintTime = value; }
    public float GetSprintCooldown { get => sprintCooldown; set => sprintCooldown = value; }
    public float GetSprintMultiplier { get => sprintMultiplier; set => sprintMultiplier = value; }
    public float GetRotationSpeed { get => rotationSpeed; set => rotationSpeed = value; }
    public float GetShootToHeal { get => shootToHeal; set => shootToHeal = value; }

    public void OnEnable()
    {
        playerInput = new InputSystem_Actions();
        playerInput.Player.Enable();
    }

    public void OnDisable()
    {
        playerInput.Player.Disable();
    }

    void Awake()
    {
        health = GetComponent<Health>();
        health.GetMaxHealth = GetMaxHealth;
        camFollow = cam.GetComponent<CinemachineFollow>();
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        // Apply knockback first
        HandleKnockback();

        Vector2 moveInput = playerInput.Player.Move.ReadValue<Vector2>();
        Vector3 moveDir = new Vector3(moveInput.x, 0, moveInput.y);

        bool sprintInput = playerInput.Player.Sprint.inProgress;

        // Handle sprinting
        if (sprintInput && sprintCooldownTimer <= 0f)
        {
            isSprinting = true;
            sprintTimer += Time.deltaTime;

            if (sprintTimer >= GetSprintTime)
            {
                // Max sprint reached, force full cooldown
                isSprinting = false;
                sprintTimer = 0f;
                sprintCooldownTimer = GetSprintCooldown;
            }
        }
        else
        {
            // Not sprinting
            if (isSprinting)
            {
                // Player just stopped sprinting before max
                float sprintRatio = sprintTimer / GetSprintTime; // % of sprint used
                sprintCooldownTimer = GetSprintCooldown * sprintRatio; // scale cooldown
                sprintTimer = 0f;
            }

            isSprinting = false;

            // Always tick down cooldown
            if (sprintCooldownTimer > 0f)
            {
                sprintCooldownTimer -= Time.deltaTime;
            }
        }

        // Apply movement
        float currentSpeed = GetMoveSpeed * (isSprinting ? GetSprintMultiplier : 1f);
        transform.Translate(moveDir * currentSpeed * Time.deltaTime, Space.World);

        // --- Facing Target ---
        if (target != null)
        {
            Vector3 direction = target.position - transform.position;
            direction.y = 0;

            if (direction.sqrMagnitude > 0.01f)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                // Rotate at a fixed speed in degrees/sec
                transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRotation, GetRotationSpeed * Time.deltaTime);
            }
        }

        EnemySearch();

        if(target != null)
        {
            fireRateTimer += Time.deltaTime;

            if(fireRateTimer >= GetFireRate)
            {
                ShootAtEnemy();
                fireRateTimer = 0;
            } 
        }


        camFollow.FollowOffset = new Vector3(0, cameraViewDistance, 0);

        if (GameManager.Instance.showSprintSlider)
        {
            UpdateSprintSlider();
        }
    }

    public void OnAttackPerformed(InputAction.CallbackContext context)
    {
        //Add bool check for allowing player enabled shooting?
        //Instantiate bullet here
        GameObject newBullet = Instantiate(bulletPrefab, barrelOut.position, barrelOut.rotation);
        newBullet.GetComponent<Projectile>().damage = GetBulletDamage;
        newBullet.GetComponent<Rigidbody>().AddForce(barrelOut.forward * bulletForce, ForceMode.Impulse);
        Destroy(newBullet, 5f);
    }

    void ShootAtEnemy()
    {
        //Instantiate bullet here
        audioSource.Play();
        GameObject newBullet = Instantiate(bulletPrefab, barrelOut.position, barrelOut.rotation);
        newBullet.GetComponent<Projectile>().damage = GetBulletDamage;
        newBullet.GetComponent<Projectile>().shootToHeal = GetShootToHeal;
        newBullet.GetComponent<Projectile>().playerController = this;
        newBullet.GetComponent<Rigidbody>().AddForce(barrelOut.forward * bulletForce, ForceMode.Impulse);
        Destroy(newBullet, 5f);
    }
    void EnemySearch()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, GetEnemyDetectionRange);

        float closestDistance = Mathf.Infinity;
        Transform closestEnemy = null;

        for (int i = 0; i < hits.Length; i++)
        {
            EnemyController enemy = hits[i].GetComponent<EnemyController>();
            if (enemy != null)
            {
                float dist = Vector3.Distance(transform.position, hits[i].transform.position);
                if (dist < closestDistance)
                {
                    closestDistance = dist;
                    closestEnemy = hits[i].transform;
                }
            }
        }

        if (closestEnemy != null)
        {
            target = closestEnemy;
        }
        else
        {
            target = null; // nothing in range
        }
    }

    public void UpdateMaxHealth()
    {
        health.GetMaxHealth = GetMaxHealth;
    }

    public void UpdateHealth(float value)
    {
        health.GetHealth += value;
    }

    void UpdateSprintSlider()
    {
        sprintSlider.maxValue = sprintTime;

        if (isSprinting)
            sprintSlider.value = sprintTimer;
        else
        sprintSlider.value = sprintCooldownTimer;
    }

    public void ApplyKnockback(Vector3 direction, float force)
    {
        // Only horizontal knockback
        direction.y = 0;
        knockbackVelocity += direction.normalized * force;
    }

    void HandleKnockback()
    {
        if (knockbackVelocity.magnitude > 0.01f)
        {
            // Move player by knockback
            transform.position += knockbackVelocity * Time.deltaTime;

            // Gradually reduce knockback over time
            knockbackVelocity = Vector3.Lerp(knockbackVelocity, Vector3.zero, knockbackRecoverySpeed * Time.deltaTime);
        }
    }
}