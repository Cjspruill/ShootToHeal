using System.Collections;
using UnityEngine;

/// <summary>
/// Attach to the staff GameObject.
/// PlayerController calls FireBurst() instead of spawning bullets directly.
/// Fires 3 magic missiles in quick succession from the staff tip.
/// </summary>
public class StaffWeapon : MonoBehaviour
{
    [SerializeField] public GameObject magicMissilePrefab;  // prefab with MagicMissile + Rigidbody
    [SerializeField] public Transform staffTip;             // spawn point at the top of the staff
    [SerializeField] float burstInterval = 0.08f;    // seconds between each missile in the burst
    [SerializeField] float spreadAngle = 8f;         // slight fan spread so they don't overlap

    // Set by PlayerController before firing
    [HideInInspector] public PlayerController playerController;

    public void FireBurst(float damage, float shootToHeal, float bulletKnockback)
    {
        StartCoroutine(BurstRoutine(damage, shootToHeal, bulletKnockback));
    }

    IEnumerator BurstRoutine(float damage, float shootToHeal, float bulletKnockback)
    {
        // Spread offsets: left, center, right
        float[] offsets = { -spreadAngle, 0f, spreadAngle };

        for (int i = 0; i < 3; i++)
        {
            SpawnMissile(damage, shootToHeal, bulletKnockback, offsets[i]);

            if (i < 2) // no wait after the last one
                yield return new WaitForSeconds(burstInterval);
        }
    }

    void SpawnMissile(float damage, float shootToHeal, float bulletKnockback, float yawOffset)
    {
        Quaternion spawnRot = staffTip.rotation * Quaternion.Euler(0f, yawOffset, 0f);
        GameObject obj = Instantiate(magicMissilePrefab, staffTip.position, spawnRot);

        MagicMissile missile = obj.GetComponent<MagicMissile>();
        missile.damage = damage;
        missile.shootToHeal = shootToHeal;
        missile.bulletPushback = bulletKnockback;
        missile.playerController = playerController;
    }
}