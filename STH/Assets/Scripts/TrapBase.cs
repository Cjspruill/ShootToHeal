using UnityEngine;

/// <summary>
/// Base class for all traps. Handles shared activation logic,
/// cooldown, and who triggered the trap (player vs. enemy).
/// </summary>
public abstract class TrapBase : MonoBehaviour
{
    [Header("Trap Settings")]
    [SerializeField] protected float damage = 20f;
    [SerializeField] protected float activationCooldown = 3f;
    [SerializeField] protected bool canPlayerActivate = true;
    [SerializeField] protected bool canEnemyActivate = true;
    [SerializeField] protected AudioSource audioSource;
    [SerializeField] protected AudioClip activationClip;

    protected float cooldownTimer = 0f;
    protected bool isOnCooldown = false;

    protected virtual void Awake() { }

    protected virtual void Update()
    {
        if (isOnCooldown)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f)
            {
                isOnCooldown = false;
                OnCooldownEnd();
            }
        }
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (isOnCooldown) return;

        bool isPlayer = other.GetComponent<PlayerController>() != null;
        bool isEnemy = other.GetComponent<EnemyController>() != null;

        if (isPlayer && canPlayerActivate)
            Activate(other.gameObject, triggeredByPlayer: true);
        else if (isEnemy && canEnemyActivate)
            Activate(other.gameObject, triggeredByPlayer: false);
    }

    /// <summary>Override in each trap to define what happens on activation.</summary>
    protected abstract void Activate(GameObject trigger, bool triggeredByPlayer);

    protected void StartCooldown()
    {
        isOnCooldown = true;
        cooldownTimer = activationCooldown;
    }

    protected virtual void OnCooldownEnd() { }

    protected void PlayAudio()
    {
        if (audioSource != null && activationClip != null)
            audioSource.PlayOneShot(activationClip);
    }
}