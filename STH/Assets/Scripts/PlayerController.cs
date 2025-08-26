using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
using UnityEngine.UIElements;
using UnityEngine.UI;


public class PlayerController : MonoBehaviour
{
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] Transform barrelOut;
    [SerializeField] Transform doubleGunBarrelOutLeft;
    [SerializeField] Transform doubleGunBarrelOutRight;
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
    [SerializeField] float cash;

    [Header("Knockback")]
    [SerializeField] float knockbackRecoverySpeed = 5f; // how fast the knockback wears off
    Vector3 knockbackVelocity = Vector3.zero;
    [SerializeField]private float shotgunSpreadAngle;

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
    public float GetCash { get => cash; set => cash = value; }

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
        // Apply knockback decay
        HandleKnockback();

        // --- Movement Input ---
        Vector2 moveInput = playerInput.Player.Move.ReadValue<Vector2>();
        Vector3 moveDir = new Vector3(moveInput.x, 0, moveInput.y);

        // --- Sprint Input ---
        bool sprintInput = playerInput.Player.Sprint.inProgress;

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
                float sprintRatio = sprintTimer / GetSprintTime;
                sprintCooldownTimer = GetSprintCooldown * sprintRatio;
                sprintTimer = 0f;
            }

            isSprinting = false;

            if (sprintCooldownTimer > 0f)
                sprintCooldownTimer -= Time.deltaTime;
        }

        // --- Apply movement + knockback ---
        float currentSpeed = GetMoveSpeed * (isSprinting ? GetSprintMultiplier : 1f);
        Vector3 finalVelocity = (moveDir * currentSpeed) + knockbackVelocity;
        transform.Translate(finalVelocity * Time.deltaTime, Space.World);

        // --- Facing Target (only if not being knocked back) ---
        if (target != null && knockbackVelocity == Vector3.zero)
        {
            Vector3 direction = target.position - transform.position;
            direction.y = 0;
            if (direction.sqrMagnitude > 0.01f)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    lookRotation,
                    GetRotationSpeed * Time.deltaTime
                );
            }
        }

        // --- Enemy search + shooting ---
        EnemySearch();

        if (target != null)
        {
            fireRateTimer += Time.deltaTime;

            if (fireRateTimer >= GetFireRate)
            {
                ShootAtEnemy();
                fireRateTimer = 0;
            }
        }

        camFollow.FollowOffset = new Vector3(0, cameraViewDistance, 0);

        if (GameManager.Instance.showSprintSlider)
            UpdateSprintSlider();
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

        if (GameManager.Instance.doubleGunsActive)
        {
            GameObject newBulletLeft = Instantiate(bulletPrefab, doubleGunBarrelOutLeft.position, doubleGunBarrelOutLeft.rotation);
            newBulletLeft.GetComponent<Projectile>().damage = GetBulletDamage;
            newBulletLeft.GetComponent<Projectile>().shootToHeal = GetShootToHeal;
            newBulletLeft.GetComponent<Projectile>().playerController = this;
            newBulletLeft.GetComponent<Rigidbody>().AddForce(doubleGunBarrelOutLeft.forward * bulletForce, ForceMode.Impulse);
            Destroy(newBulletLeft, 5f);

            GameObject newBulletRight = Instantiate(bulletPrefab, doubleGunBarrelOutRight.position, doubleGunBarrelOutRight.rotation);
            newBulletRight.GetComponent<Projectile>().damage = GetBulletDamage;
            newBulletRight.GetComponent<Projectile>().shootToHeal = GetShootToHeal;
            newBulletRight.GetComponent<Projectile>().playerController = this;
            newBulletRight.GetComponent<Rigidbody>().AddForce(doubleGunBarrelOutRight.forward * bulletForce, ForceMode.Impulse);
            Destroy(newBulletRight, 5f);
        }
        else if (GameManager.Instance.shotgunActive)
        {
            int pelletCount = Random.Range(3, 8); // fires between 3 and 7 pellets

            for (int i = 0; i < pelletCount; i++)
            {
                // Random angle offset within spread cone
                float angleX = Random.Range(-shotgunSpreadAngle, shotgunSpreadAngle);
                float angleY = Random.Range(-shotgunSpreadAngle, shotgunSpreadAngle);

                // Apply rotation offset
                Quaternion spreadRotation = Quaternion.Euler(barrelOut.eulerAngles.x + angleX,
                                                             barrelOut.eulerAngles.y + angleY,
                                                             barrelOut.eulerAngles.z);

                // Spawn pellet
                GameObject pellet = Instantiate(bulletPrefab, barrelOut.position, spreadRotation);
                pellet.GetComponent<Projectile>().damage = GetBulletDamage;
                pellet.GetComponent<Projectile>().shootToHeal = GetShootToHeal;
                pellet.GetComponent<Projectile>().playerController = this;

                // Apply force in the spread direction
                pellet.GetComponent<Rigidbody>().AddForce(spreadRotation * Vector3.forward * bulletForce, ForceMode.Impulse);

                Destroy(pellet, 5f);
            }
        }
        else
        {
            GameObject newBullet = Instantiate(bulletPrefab, barrelOut.position, barrelOut.rotation);
            newBullet.GetComponent<Projectile>().damage = GetBulletDamage;
            newBullet.GetComponent<Projectile>().shootToHeal = GetShootToHeal;
            newBullet.GetComponent<Projectile>().playerController = this;
            newBullet.GetComponent<Rigidbody>().AddForce(barrelOut.forward * bulletForce, ForceMode.Impulse);
            Destroy(newBullet, 5f);
        }

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
        direction.y = 0;

        Vector3 newForce = direction.normalized * force;
        knockbackVelocity += newForce;

        float maxKnockback = 20f; // clamp so spam-hit doesn't go crazy
        if (knockbackVelocity.magnitude > maxKnockback)
            knockbackVelocity = knockbackVelocity.normalized * maxKnockback;
    }

    void HandleKnockback()
    {
        if (knockbackVelocity.magnitude > 0.01f)
        {
            knockbackVelocity = Vector3.Lerp(
                knockbackVelocity,
                Vector3.zero,
                knockbackRecoverySpeed * Time.deltaTime
            );

            if (knockbackVelocity.magnitude < 0.1f)
                knockbackVelocity = Vector3.zero;
        }
    }

}