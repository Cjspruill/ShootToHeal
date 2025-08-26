using UnityEngine;

public class FlameThrower : MonoBehaviour
{

    [SerializeField] public float damage;
    [SerializeField] public float shootToHeal;
    [SerializeField] AudioSource audioSource;
    [SerializeField] PlayerController playerController;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnParticleCollision(GameObject other)
    {
        Health health = other.GetComponent<Health>();

        if (health != null) 
        {
            health.TakeDamage(damage);
        }

        if (playerController)
            playerController.GetComponent<Health>().GiveHealth(shootToHeal);
    }
}
