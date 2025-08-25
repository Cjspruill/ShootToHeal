using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField] float moveSpeed = 5f;        // movement speed
    [SerializeField] float stoppingDistance = 2f; // how close before stopping
    [SerializeField] float rotationSpeed = 10f;   // how quickly enemy rotates
    [SerializeField] float separationRadius = 2f; // how close enemies can get to each other
    [SerializeField] float separationForce = 3f;  // strength of separation push


    [SerializeField] float damage;
    Transform target;

    [SerializeField] float amountOfXpToDrop;
    [SerializeField] GameObject xpOrb;

    [SerializeField] GameObject audioPrefab;

    void Start()
    {
        target = FindFirstObjectByType<PlayerController>().transform;
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    void Update()
    {
        if (target == null) return;

        // --- Direction to player ---
        Vector3 toPlayer = target.position - transform.position;
        toPlayer.y = 0;

        float distance = toPlayer.magnitude;

        // --- Separation from other enemies ---
        Vector3 separation = Vector3.zero;
        Collider[] hits = Physics.OverlapSphere(transform.position, separationRadius);
        foreach (var hit in hits)
        {
            if (hit.gameObject != gameObject && hit.GetComponent<EnemyController>() != null)
            {
                Vector3 away = transform.position - hit.transform.position;
                separation += away.normalized / away.magnitude; // stronger push if very close
            }
        }

        // --- Combine movement ---
        Vector3 moveDir = Vector3.zero;
        if (distance > stoppingDistance)
        {
            moveDir = toPlayer.normalized;
        }

        Vector3 finalDir = (moveDir + separation * separationForce).normalized;

        // --- Rotate towards player (not separation) ---
        if (toPlayer != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(toPlayer);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
        }

        // --- Move ---
        if (finalDir != Vector3.zero)
        {
            transform.position += finalDir * moveSpeed * Time.deltaTime;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        PlayerController playerController = collision.gameObject.GetComponent<PlayerController>();

        if (playerController != null)
        {
            Health health = playerController.GetComponent<Health>();

            health.TakeDamage(damage);
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