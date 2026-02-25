using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
using UnityEngine.UIElements;
using UnityEngine.UI;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] Transform barrelOut;
    [SerializeField] Transform doubleGunBarrelOutLeft;
    [SerializeField] Transform doubleGunBarrelOutRight;
    [SerializeField] public StaffWeapon staffWeapon;
    [SerializeField] Transform target;
    [SerializeField] public Health health;
    [SerializeField] CinemachineCamera cam;
    [SerializeField] CinemachineFollow camFollow;
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip doubleGunAudioClip;
    [SerializeField] AudioClip machineGunAudioClip;
    [SerializeField] AudioClip shotgunAudioClip;
    [SerializeField] UnityEngine.UI.Slider sprintSlider;
    [SerializeField] float bulletForce = 100f;
    [SerializeField] GameObject shadowPrefab;
    [SerializeField] float shadowTime;
    [SerializeField] float shadowTimer;
    [SerializeField] private float shadowEffectTime = 1.0f; // Duration to fade out the shadow

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
    [SerializeField] float flameThrowerDuration;
    [SerializeField] float bulletKnockback;
    InputSystem_Actions playerInput;
    CharacterController characterController;
    [SerializeField] ParticleSystem[] flameThrower;

    [Header("Timers")]
    [SerializeField] float fireRateTimer;
    [SerializeField] float sprintTimer;
    public float sprintCooldownTimer;
    public bool isSprinting;

    [SerializeField] float xp;
    [SerializeField] float cash;

    [Header("Knockback")]
    [SerializeField] float knockbackRecoverySpeed = 5f;
    Vector3 knockbackVelocity = Vector3.zero;
    [SerializeField] private float shotgunSpreadAngle;

    [Header("Gravity")]
    [SerializeField] float gravity = -9.81f;
    Vector3 verticalVelocity;

    [SerializeField] GameObject targetReticleGameObject;
    [SerializeField] TargetReticle targetReticle;

    Vector2 touchStartPos;
    Vector2 touchCurrentPos;
    bool isTouching = false;
    Vector2 externalMoveInput = Vector2.zero;

    [Header("Stuck Detection")]
    [SerializeField] float stuckCheckTime = 5f; // time before damage
    [SerializeField] float stuckVelocityThreshold = 0.1f; // considered "not moving"
    [SerializeField] float stuckTimer = 0f;
    Vector3 lastPosition;
    // Properties
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
    public float GetFlameThrowerDuration { get => flameThrowerDuration; set => flameThrowerDuration = value; }
    public float GetBulletKnockback { get => bulletKnockback; set => bulletKnockback = value; }
    public Transform GetCurrentTarget { get => target; set => target = value; }

    void OnEnable()
    {
        playerInput = new InputSystem_Actions();
        playerInput.Player.Enable();
    }

    void OnDisable()
    {
        playerInput.Player.Disable();
    }

    void Awake()
    {
        health = GetComponent<Health>();
        health.GetMaxHealth = GetMaxHealth;
        camFollow = cam.GetComponent<CinemachineFollow>();
        audioSource = GetComponent<AudioSource>();
        characterController = GetComponent<CharacterController>();
        targetReticle = targetReticleGameObject.GetComponent<TargetReticle>();

        lastPosition = transform.position;
    }

    void Update()
    {
        //CheckIfStuck();
        HandleKnockback();
        HandleMovement();
        HandleFacing();

        // Shooting logic
        EnemySearch();
        if (GameManager.Instance.showTargetReticle)
        {
            PlaceTargetReticle();
        }

        if (GetCurrentTarget != null)
        {
            fireRateTimer += Time.deltaTime;


            if (GameManager.Instance.flamethrowerActive)
            {
                if (fireRateTimer >= flameThrowerDuration * 2)
                {
                    FireFlameThrower();
                    fireRateTimer = 0;
                }
            }
            else
            {
                if (fireRateTimer >= GetFireRate)
                {
                    ShootAtEnemy();
                    fireRateTimer = 0;
                }
            }
        }

        camFollow.FollowOffset = new Vector3(0, cameraViewDistance, 0);

        if (GameManager.Instance.showSprintSlider)
            UpdateSprintSlider();

        HandleTouchInput(); // 👈 add this here
    }
    void HandleTouchInput()
    {
        bool isPC = Application.platform == RuntimePlatform.WindowsPlayer
          || Application.platform == RuntimePlatform.WindowsEditor
          || Application.platform == RuntimePlatform.OSXPlayer
          || Application.platform == RuntimePlatform.LinuxPlayer;

        if (isPC) return; // no touchscreen available

        int activeTouchCount = 0;
        foreach (var touch in Touchscreen.current.touches)
        {
            if (touch.press.isPressed)
                activeTouchCount++;
        }

        // --- Movement ---
        if (activeTouchCount >= 1)
        {
            var primaryTouch = Touchscreen.current.primaryTouch;

            if (!isTouching)
            {
                isTouching = true;
                touchStartPos = primaryTouch.startPosition.ReadValue();
            }

            touchCurrentPos = primaryTouch.position.ReadValue();
            Vector2 dragDelta = touchCurrentPos - touchStartPos;

            // Normalize drag like joystick
            Vector2 moveInput = dragDelta.normalized;
            ApplyTouchMovement(moveInput);
        }
        else
        {
            isTouching = false;
            externalMoveInput = Vector2.zero;
        }

        // --- Sprint ---
        if (activeTouchCount >= 2 && sprintCooldownTimer <= 0f)
        {
            // keep sprinting as long as key is held
            isSprinting = true;
            

            if (shadowTimer >= shadowTime) {
                SpawnShadow();
                shadowTimer = 0;
            }
            sprintTimer += Time.deltaTime;
            shadowTimer += Time.deltaTime;

            // if sprint time is exceeded, force cooldown
            if (sprintTimer >= GetSprintTime)
            {
                isSprinting = false;
                sprintTimer = 0f;
                sprintCooldownTimer = GetSprintCooldown;
            }
        }
        else if (activeTouchCount < 2)
        {
            if (isSprinting)
            {
                // went from sprinting → released early → partial cooldown
                float sprintRatio = sprintTimer / GetSprintTime;
                sprintCooldownTimer = GetSprintCooldown * sprintRatio;
                sprintTimer = 0f;
            }

            isSprinting = false;

        }
        if (sprintCooldownTimer > 0f)
            sprintCooldownTimer -= Time.deltaTime;
    }

    void ApplyTouchMovement(Vector2 moveInput)
    {
        externalMoveInput = moveInput;
    }


    void HandleMovement()
    {
        // 1️⃣ Platform check
        bool isPC = Application.platform == RuntimePlatform.WindowsPlayer
                  || Application.platform == RuntimePlatform.WindowsEditor
                  || Application.platform == RuntimePlatform.OSXPlayer
                  || Application.platform == RuntimePlatform.LinuxPlayer;

        // 2️⃣ Get movement input
        Vector2 moveInput = Vector2.zero;
        if (isPC)
            moveInput = playerInput.Player.Move.ReadValue<Vector2>();
        else if (Touchscreen.current != null && externalMoveInput != Vector2.zero)
            moveInput = externalMoveInput; // Mobile touch input

        Vector3 moveDir = new Vector3(moveInput.x, 0, moveInput.y);

        // 3️⃣ Handle sprint
        float currentSpeed = GetMoveSpeed;

        if (isPC)
        {
            bool sprintInput = playerInput.Player.Sprint.inProgress;

            if (sprintInput && sprintCooldownTimer <= 0f)
            {
                isSprinting = true;
                if (shadowTimer >= shadowTime)
                {
                    SpawnShadow();
                    shadowTimer = 0;
                }
                sprintTimer += Time.deltaTime;
                shadowTimer += Time.deltaTime;

                if (sprintTimer >= GetSprintTime)
                {
                    isSprinting = false;
                    sprintTimer = 0f;
                    sprintCooldownTimer = GetSprintCooldown;
                }
            }
            else if (!sprintInput)
            {
                if (isSprinting)
                {
                    float sprintRatio = sprintTimer / GetSprintTime;
                    sprintCooldownTimer = GetSprintCooldown * sprintRatio;
                    sprintTimer = 0f;
                }
                isSprinting = false;
            }

            if (sprintCooldownTimer > 0f)
                sprintCooldownTimer -= Time.deltaTime;
        }
        // Mobile sprint handled in HandleTouchInput()

        currentSpeed *= isSprinting ? GetSprintMultiplier : 1f;

        // 4️⃣ Gravity
        if (characterController.isGrounded && verticalVelocity.y < 0)
            verticalVelocity.y = -2f;
        verticalVelocity.y += gravity * Time.deltaTime;

        // 5️⃣ Safe knockback
        Vector3 safeKnockback = GetSafeKnockback(knockbackVelocity * Time.deltaTime);

        // 6️⃣ Combine input movement + knockback
        Vector3 totalMove = moveDir * currentSpeed * Time.deltaTime + safeKnockback;

        // 7️⃣ Collision-safe movement
        if (totalMove.magnitude > 0.001f)
        {
            // Cast capsule in the movement direction
            if (Physics.CapsuleCast(
                characterController.transform.position + characterController.center + Vector3.up * -characterController.height / 2,
                characterController.transform.position + characterController.center + Vector3.up * characterController.height / 2,
                characterController.radius,
                totalMove.normalized,
                out RaycastHit hit,
                totalMove.magnitude
            ))
            {
                // Slide along wall
                totalMove = Vector3.ProjectOnPlane(totalMove, hit.normal) * 0.9f;
            }
        }

        // 8️⃣ Apply vertical velocity (gravity)
        totalMove += verticalVelocity * Time.deltaTime;

        // 9️⃣ Move the player
        characterController.Move(totalMove);
    }

    Vector3 GetSafeKnockback(Vector3 knockback)
    {
        if (knockback.magnitude < 0.01f)
            return Vector3.zero;

        Vector3 safeMove = knockback;
        int iterations = 3; // multiple small checks per frame

        for (int i = 0; i < iterations; i++)
        {
            if (Physics.CapsuleCast(
                characterController.transform.position + characterController.center + Vector3.up * -characterController.height / 2,
                characterController.transform.position + characterController.center + Vector3.up * characterController.height / 2,
                characterController.radius,
                safeMove.normalized,
                out RaycastHit hit,
                safeMove.magnitude * Time.deltaTime
            ))
            {
                // Slide along the wall instead of pushing into it
                safeMove = Vector3.ProjectOnPlane(safeMove, hit.normal);

                // Slightly reduce magnitude to avoid sticking
                safeMove *= 0.9f;
            }
            else
            {
                break; // no collision, safe to move
            }
        }

        return safeMove;
    }
    void HandleFacing()
    {
        if (GetCurrentTarget != null && knockbackVelocity == Vector3.zero)
        {
            Vector3 direction = GetCurrentTarget.position - transform.position;
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
    }


    void ShootAtEnemy()
    {
        if (GameManager.Instance.staffActive)
        {
            staffWeapon.FireBurst(GetBulletDamage, GetShootToHeal, GetBulletKnockback);
            return;
        }
        if (GameManager.Instance.doubleGunsActive)
        {
            audioSource.clip = doubleGunAudioClip;
            audioSource.Play();


            GameObject newBulletLeft = Instantiate(bulletPrefab, doubleGunBarrelOutLeft.position, doubleGunBarrelOutLeft.rotation);
            newBulletLeft.GetComponent<Projectile>().damage = GetBulletDamage;
            newBulletLeft.GetComponent<Projectile>().shootToHeal = GetShootToHeal;
            newBulletLeft.GetComponent<Projectile>().bulletPushback = GetBulletKnockback;
            newBulletLeft.GetComponent<Projectile>().playerController = this;
            newBulletLeft.GetComponent<Rigidbody>().AddForce(barrelOut.forward * bulletForce, ForceMode.Impulse);
            Destroy(newBulletLeft, 5f);

            GameObject newBulletRight = Instantiate(bulletPrefab, doubleGunBarrelOutRight.position, doubleGunBarrelOutRight.rotation);
            newBulletRight.GetComponent<Projectile>().damage = GetBulletDamage;
            newBulletRight.GetComponent<Projectile>().shootToHeal = GetShootToHeal;
            newBulletRight.GetComponent<Projectile>().bulletPushback = GetBulletKnockback;
            newBulletRight.GetComponent<Projectile>().playerController = this;
            newBulletRight.GetComponent<Rigidbody>().AddForce(barrelOut.forward * bulletForce, ForceMode.Impulse);
            Destroy(newBulletRight, 5f);
        }
        else if (GameManager.Instance.shotgunActive)
        {
            audioSource.clip = shotgunAudioClip;
            audioSource.Play();

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
                pellet.GetComponent<Projectile>().bulletPushback = GetBulletKnockback;
                pellet.GetComponent<Projectile>().playerController = this;

                // Apply force in the spread direction
                pellet.GetComponent<Rigidbody>().AddForce(spreadRotation * Vector3.forward * bulletForce, ForceMode.Impulse);

                Destroy(pellet, 5f);
            }
        }

        else
        {
            audioSource.clip = machineGunAudioClip;
            audioSource.Play();

            GameObject newBullet = Instantiate(bulletPrefab, barrelOut.position, barrelOut.rotation);
            newBullet.GetComponent<Projectile>().damage = GetBulletDamage;
            newBullet.GetComponent<Projectile>().shootToHeal = GetShootToHeal;
            newBullet.GetComponent<Projectile>().bulletPushback = GetBulletKnockback;
            newBullet.GetComponent<Projectile>().playerController = this;
            newBullet.GetComponent<Rigidbody>().AddForce(barrelOut.forward * bulletForce, ForceMode.Impulse);
            Destroy(newBullet, 5f);
        }
    }

    void FireFlameThrower()
    {
        for (int i = 0; i < flameThrower.Length; i++)
            flameThrower[i].Play();

        flameThrower[0].gameObject.GetComponent<AudioSource>().Play();
        flameThrower[0].gameObject.GetComponent<FlameThrower>().shootToHeal = GetShootToHeal * .045f;
        flameThrower[0].gameObject.GetComponent<FlameThrower>().damage = GetBulletDamage * .045f;

        Invoke("StopFlamethrower", flameThrowerDuration);

    }

    void StopFlamethrower()
    {
        for (int i = 0; i < flameThrower.Length; i++)
            flameThrower[i].Stop();

        flameThrower[0].gameObject.GetComponent<AudioSource>().Stop();
    }

    void EnemySearch()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, GetEnemyDetectionRange);
        float closestDistance = Mathf.Infinity;
        Transform closestEnemy = null;

        foreach (Collider hit in hits)
        {
            EnemyController enemy = hit.GetComponent<EnemyController>();
            if (enemy != null)
            {
                float dist = Vector3.Distance(transform.position, hit.transform.position);
                if (dist < closestDistance)
                {
                    closestDistance = dist;
                    closestEnemy = hit.transform;
                }
            }
        }

        GetCurrentTarget = closestEnemy;
    }

    void UpdateSprintSlider()
    {
        sprintSlider.maxValue = sprintTime;
        sprintSlider.value = isSprinting ? sprintTimer : sprintCooldownTimer;
    }

    public void ApplyKnockback(Vector3 direction, float force)
    {
        direction.y = 0;
        Vector3 newForce = direction.normalized * force;
        knockbackVelocity += newForce;

        float maxKnockback = 20f;
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


    public void UpdateMaxHealth()
    {
        health.GetMaxHealth = GetMaxHealth;
    }

    public void UpdateHealth(float value)
    {
        health.GetHealth += value;

        //Cap health to max health
        if (health.GetHealth > health.GetMaxHealth)
        {
            health.GetHealth = health.GetMaxHealth;
        }
    }

    void PlaceTargetReticle()
    {
        if (GetCurrentTarget != null)
        {
            if (!targetReticleGameObject.activeInHierarchy)
                targetReticleGameObject.SetActive(true);

            if (GetCurrentTarget.gameObject.name.Contains("Grunt"))
                targetReticle.UpdateReticleSize(targetReticle.gruntReticleSize);
            if (GetCurrentTarget.gameObject.name.Contains("Runner"))
                targetReticle.UpdateReticleSize(targetReticle.runnerReticleSize);
            if (GetCurrentTarget.gameObject.name.Contains("Tank"))
                targetReticle.UpdateReticleSize(targetReticle.tankReticleSize);

            Vector3 newPos = new Vector3(GetCurrentTarget.position.x, 0, GetCurrentTarget.position.z);
            targetReticle.transform.position = newPos;
        }
        else
            targetReticleGameObject.SetActive(false);
    }

    void CheckIfStuck()
    {
        // Only run on PC or Mobile
        bool isPC = Application.platform == RuntimePlatform.WindowsPlayer
                  || Application.platform == RuntimePlatform.WindowsEditor
                  || Application.platform == RuntimePlatform.OSXPlayer
                  || Application.platform == RuntimePlatform.LinuxPlayer;

        bool isMobile = Touchscreen.current != null;

        if (!isPC && !isMobile)
            return;

        // 1️⃣ Detect if movement input is being held
        bool isMovingInput = false;

        if (isPC)
        {
            Vector2 pcMoveInput = playerInput.Player.Move.ReadValue<Vector2>();
            if (pcMoveInput.sqrMagnitude > 0.01f)
                isMovingInput = true;
        }
        else if (isMobile)
        {
            if (externalMoveInput.sqrMagnitude > 0.01f)
                isMovingInput = true;
        }

        // 2️⃣ Check velocity (horizontal movement)
        Vector3 displacement = transform.position - lastPosition;

        if (isMovingInput && displacement.magnitude < stuckVelocityThreshold)
        {
            stuckTimer += Time.deltaTime;

            if (stuckTimer >= stuckCheckTime)
            {
                health.TakeDamage(1000f);
                stuckTimer = 0f;
            }
        }
        else
        {
            stuckTimer = 0f;
        }

        lastPosition = transform.position;
    }
    void SpawnShadow() //Spawn Shadow (sprint)
    {
       GameObject newShadow = Instantiate(shadowPrefab, transform.position, Quaternion.identity);
        // Start fading coroutine
        StartCoroutine(FadeShadow(newShadow));
    }
    private IEnumerator FadeShadow(GameObject shadow)
    {
        MeshRenderer renderer = shadow.GetComponent<MeshRenderer>();

        if (renderer == null)
        {
            Debug.LogWarning("No MeshRenderer found on shadow object.");
            yield break;
        }

        Material mat = renderer.material; // This creates a unique instance of the material
        Color originalColor = mat.color;

        float elapsed = 0f;

        while (elapsed < shadowEffectTime)
        {
            float alpha = Mathf.Lerp(1f, 0f, elapsed / shadowEffectTime);
            mat.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Set final alpha to 0
        mat.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);

        // Optionally destroy the shadow after fading out
        Destroy(shadow);
    }
}