using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField] float moveSpeed = 5f;        // movement speed
    [SerializeField] float stoppingDistance = 2f; // how close before stopping
    [SerializeField] float rotationSpeed = 10f;   // how quickly enemy rotates

    Transform target;

    public void Start()
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

        // Get direction towards player, ignoring vertical difference
        Vector3 direction = target.position - transform.position;
        direction.y = 0;

        float distance = direction.magnitude;

        // Rotate towards player every frame
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
        }

        // Move if farther than stoppingDistance
        if (distance > stoppingDistance)
        {
            Vector3 moveDir = direction.normalized;
            transform.position += moveDir * moveSpeed * Time.deltaTime;
        }
    }
}
