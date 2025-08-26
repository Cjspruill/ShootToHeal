using UnityEngine;

public class KillPlane : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Health health = other.GetComponent<Health>();

        if(health != null)
        {
            health.TakeDamage(10000);
        }
        else
            Destroy(other.gameObject);
    }
}
