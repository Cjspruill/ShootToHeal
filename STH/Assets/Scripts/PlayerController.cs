using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;


public class PlayerController : MonoBehaviour
{
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] Transform barrelOut;
    [SerializeField] Transform target;
    [SerializeField] Health health;
    [SerializeField] CinemachineCamera cam;
    [SerializeField] CinemachineFollow camFollow;
    [SerializeField] AudioSource audioSource;

    [SerializeField] float cameraViewDistance;
    [SerializeField] float moveSpeed = 10f;
    [SerializeField] float bulletForce = 100f;
    [SerializeField] float enemyDetectionRange = 50f;

    [Header("Stats")]
    [SerializeField] float maxHealth;
    [SerializeField] float bulletDamage;
    [SerializeField] float fireRate;
    [SerializeField] float xp;
    [SerializeField] float sprintTime;
    [SerializeField] float sprintMultiplier;

    InputSystem_Actions playerInput;

    [SerializeField] float fireRateTimer;
    [SerializeField] float sprintTimer;
    [SerializeField] private float sprintCooldown;
    public float sprintCooldownTimer;
    public bool isSprinting;

    public float GetMaxHealth { get => maxHealth; set => maxHealth = value; }
    public float GetCameraViewDistance { get => cameraViewDistance; set => cameraViewDistance = value; }
    public float GetMoveSpeed { get => moveSpeed; set => moveSpeed = value; }
    public float GetEnemyDetectionRange { get => enemyDetectionRange; set => enemyDetectionRange = value; }
    public float GetBulletDamage { get => bulletDamage; set => bulletDamage = value; }
    public float GetFireRate { get => fireRate; set => fireRate = value; }
    public float GetXp { get => xp; set => xp = value; }

    public void OnEnable()
    {
        playerInput = new InputSystem_Actions();
        playerInput.Player.Enable();
       // playerInput.Player.Attack.performed += OnAttackPerformed;
    }

    public void OnDisable()
    {
      //  playerInput.Player.Attack.performed -= OnAttackPerformed;
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
        Vector2 moveInput = playerInput.Player.Move.ReadValue<Vector2>();
        Vector3 moveDir = new Vector3(moveInput.x, 0, moveInput.y);

        bool sprintInput = playerInput.Player.Sprint.inProgress;

        // Handle sprinting
        if (sprintInput && sprintCooldownTimer <= 0f)
        {
            isSprinting = true;
            sprintTimer += Time.deltaTime;

            if (sprintTimer > sprintTime)
            {
                // Max sprint reached, start cooldown
                isSprinting = false;
                sprintCooldownTimer = sprintCooldown;
            }
        }
        else
        {
            // Not sprinting, reduce cooldown
            isSprinting = false;

            if (sprintTimer > 0f)
            {
                // Reset sprint timer if player released sprint early
                sprintTimer = 0f;
                sprintCooldownTimer = sprintCooldown; // start cooldown
            }

            if (sprintCooldownTimer > 0f)
            {
                sprintCooldownTimer -= Time.deltaTime;
            }
        }

        // Apply movement
        float currentSpeed = GetMoveSpeed * (isSprinting ? sprintMultiplier : 1f);
        transform.Translate(moveDir * currentSpeed * Time.deltaTime, Space.World);

        // --- Facing Target ---
        if (target != null)
        {
            Vector3 direction = target.position - transform.position;
            direction.y = 0; // keep only horizontal rotation
            if (direction.sqrMagnitude > 0.01f) // avoid jitter
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 10f * Time.deltaTime);
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
}