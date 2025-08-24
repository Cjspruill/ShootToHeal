using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.GraphicsBuffer;

public class PlayerController : MonoBehaviour
{
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] Transform barrelOut;
    [SerializeField] Transform target;
    [SerializeField] Health health;

    [SerializeField] float distance;
    [SerializeField] float moveSpeed = 10f;
    [SerializeField] float bulletForce = 100f;
    [SerializeField] float enemyDetectionRange = 50f;

    [Header("Stats")]
    [SerializeField] float maxHealth;
    [SerializeField] float bulletDamage;
    [SerializeField] float fireRate;


    InputSystem_Actions playerInput;

    [SerializeField] float fireRateTimer;

    public void OnEnable()
    {
        playerInput = new InputSystem_Actions();
        playerInput.Player.Enable();
        playerInput.Player.Attack.performed += OnAttackPerformed;
    }

    public void OnDisable()
    {
        playerInput.Player.Attack.performed -= OnAttackPerformed;
        playerInput.Player.Disable();
    }

    void Start()
    {
        health = GetComponent<Health>();
        health.GetMaxHealth = maxHealth;
    }

    void Update()
    {
        // --- Movement ---
        Vector2 moveInput = playerInput.Player.Move.ReadValue<Vector2>();
        Vector3 moveDir = new Vector3(moveInput.x, 0, moveInput.y);

        // Move in world space (ignores player rotation)
        transform.Translate(moveDir * moveSpeed * Time.deltaTime, Space.World);

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

            if(fireRateTimer >= fireRate)
            {
                ShootAtEnemy();
                fireRateTimer = 0;
            } 
        }

    }

    public void OnAttackPerformed(InputAction.CallbackContext context)
    {
        //Add bool check for allowing player enabled shooting?
        //Instantiate bullet here
        GameObject newBullet = Instantiate(bulletPrefab, barrelOut.position, barrelOut.rotation);
        newBullet.GetComponent<Projectile>().damage = bulletDamage;
        newBullet.GetComponent<Rigidbody>().AddForce(barrelOut.forward * bulletForce, ForceMode.Impulse);
        Destroy(newBullet, 5f);
    }

    void ShootAtEnemy()
    {
        //Instantiate bullet here
        GameObject newBullet = Instantiate(bulletPrefab, barrelOut.position, barrelOut.rotation);
        newBullet.GetComponent<Projectile>().damage = bulletDamage;
        newBullet.GetComponent<Rigidbody>().AddForce(barrelOut.forward * bulletForce, ForceMode.Impulse);
        Destroy(newBullet, 5f);
    }
    void EnemySearch()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, enemyDetectionRange);

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