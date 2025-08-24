using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] public float damage;

    public void OnCollisionEnter(Collision other)
    {
        Health health = other.gameObject.GetComponent<Health>();

        if (health != null)
        {
            health.TakeDamage(damage);
        }
    }
}
