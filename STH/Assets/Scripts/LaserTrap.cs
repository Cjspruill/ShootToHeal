using System.Collections;
using UnityEngine;

/// <summary>
/// Laser Trap.
/// A pad switch on the ground activates a laser beam that sweeps across
/// the arena, damaging anyone who isn't the trap's activator side.
///
/// Setup in scene:
///  - LaserTrap GameObject: place this script + a trigger collider (the pad)
///  - LaserOrigin: where the laser spawns from
///  - Assign laserBeam LineRenderer with 2 positions
/// </summary>
public class LaserTrap : TrapBase
{
    [Header("Laser Settings")]
    [SerializeField] Transform laserOrigin;          // pivot the laser sweeps from
    [SerializeField] LineRenderer laserBeam;         // LineRenderer for the visual
    [SerializeField] float laserLength = 40f;
    [SerializeField] float sweepAngle  = 180f;       // total arc in degrees
    [SerializeField] float sweepSpeed  = 60f;        // degrees per second
    [SerializeField] float laserDamageInterval = 0.1f; // seconds between damage ticks
    [SerializeField] LayerMask laserHitLayers;

    [Header("Pad Visual")]
    [SerializeField] MeshRenderer padRenderer;
    [SerializeField] Color readyColor   = Color.green;
    [SerializeField] Color activeColor  = Color.red;
    [SerializeField] Color cooldownColor = Color.yellow;

    bool isSweeping = false;
    bool activatedByPlayer;

    void Start()
    {
        if (laserBeam != null) laserBeam.enabled = false;
        SetPadColor(readyColor);
    }

    protected override void Activate(GameObject trigger, bool triggeredByPlayer)
    {
        activatedByPlayer = triggeredByPlayer;
        PlayAudio();
        StartCoroutine(LaserSweepRoutine());
    }

    IEnumerator LaserSweepRoutine()
    {
        StartCooldown();
        isSweeping = true;
        SetPadColor(activeColor);

        if (laserBeam != null) laserBeam.enabled = true;

        // Start angle is -half the sweep
        float halfSweep = sweepAngle * 0.5f;
        float currentAngle = -halfSweep;
        float damageTimer = 0f;

        while (currentAngle < halfSweep)
        {
            currentAngle += sweepSpeed * Time.deltaTime;
            damageTimer  += Time.deltaTime;

            // Rotate the laser direction around the up axis
            Vector3 dir = Quaternion.Euler(0, currentAngle, 0) * laserOrigin.forward;

            // Update LineRenderer
            if (laserBeam != null)
            {
                laserBeam.SetPosition(0, laserOrigin.position);
                laserBeam.SetPosition(1, laserOrigin.position + dir * laserLength);
            }

            // Damage tick
            if (damageTimer >= laserDamageInterval)
            {
                damageTimer = 0f;
                FireLaserDamage(dir);
            }

            yield return null;
        }

        // Done sweeping
        if (laserBeam != null) laserBeam.enabled = false;
        isSweeping = false;
        SetPadColor(cooldownColor);
    }

    void FireLaserDamage(Vector3 dir)
    {
        RaycastHit[] hits = Physics.RaycastAll(laserOrigin.position, dir, laserLength, laserHitLayers);

        foreach (RaycastHit hit in hits)
        {
            // Skip the activating side — the laser punishes the OTHER team
            bool hitIsPlayer = hit.collider.GetComponent<PlayerController>() != null;
            bool hitIsEnemy  = hit.collider.GetComponent<EnemyController>()  != null;

            // If activated by player → spare the player, hurt enemies
            // If activated by enemy  → spare enemies, hurt player
            if (activatedByPlayer && hitIsPlayer) continue;
            if (!activatedByPlayer && hitIsEnemy)  continue;

            Health health = hit.collider.GetComponent<Health>();
            if (health != null)
                health.TakeDamage(damage);

            EnemyController ec = hit.collider.GetComponent<EnemyController>();
            if (ec != null)
                ec.PlayHurtAudio();
        }
    }

    protected override void OnCooldownEnd()
    {
        SetPadColor(readyColor);
    }

    void SetPadColor(Color c)
    {
        if (padRenderer != null)
            padRenderer.material.color = c;
    }

    void OnDrawGizmosSelected()
    {
        if (laserOrigin == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawRay(laserOrigin.position, laserOrigin.forward * laserLength);
    }
}
