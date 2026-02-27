using System.Collections;
using UnityEngine;

/// <summary>
/// Spike Pad - polls for nearby player/enemies every frame instead of
/// relying on trigger callbacks. Works regardless of whether the
/// player collider is a trigger or not.
///
/// Place on SpikePlate.
/// Drag the "Spikes" child into spikesRoot in the Inspector.
/// The two BoxColliders on SpikePlate are fine as-is for physics/visuals.
/// Fix the trigger BoxCollider center Y to ~0 so it sits at plate level.
/// </summary>
public class SpikePad : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform spikesRoot; // drag "Spikes" child here

    [Header("Detection")]
    [SerializeField] Vector3 detectionBoxSize = new Vector3(3f, 1f, 3f); // match your plate size
    [SerializeField] Vector3 detectionBoxOffset = Vector3.zero;           // offset from SpikePlate centre

    [Header("Settings")]
    [SerializeField] float damage = 20f;
    [SerializeField] float spikeRiseHeight = 1f;
    [SerializeField] float spikeRiseSpeed = 6f;
    [SerializeField] float holdDuration = 0.5f;
    [SerializeField] float cooldown = 3f;

    Vector3 hiddenPos;
    Vector3 raisedPos;
    bool isAnimating = false;
    bool onCooldown = false;

    void Start()
    {
        if (spikesRoot == null)
            spikesRoot = transform.Find("Spikes");

        if (spikesRoot == null)
        {
            Debug.LogError("[SpikePad] Spikes child not found on " + gameObject.name);
            return;
        }

        hiddenPos = spikesRoot.localPosition;
        raisedPos = hiddenPos + Vector3.up * spikeRiseHeight;

        Debug.Log("[SpikePad] Ready. hiddenPos=" + hiddenPos + " raisedPos=" + raisedPos);
    }

    void Update()
    {
        if (isAnimating || onCooldown || spikesRoot == null) return;

        // Poll for player or enemy overlapping the plate area
        Vector3 worldCenter = transform.position + transform.TransformDirection(detectionBoxOffset);
        Collider[] hits = Physics.OverlapBox(worldCenter, detectionBoxSize * 0.5f, transform.rotation);

        foreach (Collider hit in hits)
        {
            bool isPlayer = hit.GetComponent<PlayerController>() != null
                         || hit.GetComponentInParent<PlayerController>() != null;
            bool isEnemy = hit.GetComponent<EnemyController>() != null
                         || hit.GetComponentInParent<EnemyController>() != null;

            if (isPlayer || isEnemy)
            {
                Debug.Log("[SpikePad] Triggered by: " + hit.gameObject.name);
                StartCoroutine(SpikeRoutine());
                return;
            }
        }
    }

    IEnumerator SpikeRoutine()
    {
        isAnimating = true;
        onCooldown = true;

        // Rise
        float t = 0f;
        while (t < 1f)
        {
            t = Mathf.MoveTowards(t, 1f, Time.deltaTime * spikeRiseSpeed);
            spikesRoot.localPosition = Vector3.Lerp(hiddenPos, raisedPos, t);
            yield return null;
        }
        spikesRoot.localPosition = raisedPos;

        // Damage everything in the detection area
        Vector3 worldCenter = transform.position + transform.TransformDirection(detectionBoxOffset);
        Collider[] hits = Physics.OverlapBox(worldCenter, detectionBoxSize * 0.5f, transform.rotation);
        foreach (Collider hit in hits)
        {
            if (hit.transform.IsChildOf(transform)) continue;

            Health h = hit.GetComponent<Health>() ?? hit.GetComponentInParent<Health>();
            if (h != null)
            {
                Debug.Log("[SpikePad] Damaging: " + hit.gameObject.name);
                h.TakeDamage(damage);
            }

            EnemyController ec = hit.GetComponent<EnemyController>() ?? hit.GetComponentInParent<EnemyController>();
            if (ec != null) ec.PlayHurtAudio();
        }

        // Hold
        yield return new WaitForSeconds(holdDuration);

        // Retract
        t = 0f;
        while (t < 1f)
        {
            t = Mathf.MoveTowards(t, 1f, Time.deltaTime * spikeRiseSpeed * 0.5f);
            spikesRoot.localPosition = Vector3.Lerp(raisedPos, hiddenPos, t);
            yield return null;
        }
        spikesRoot.localPosition = hiddenPos;
        isAnimating = false;

        yield return new WaitForSeconds(cooldown);
        onCooldown = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.35f);
        Vector3 worldCenter = transform.position + transform.TransformDirection(detectionBoxOffset);
        Gizmos.matrix = Matrix4x4.TRS(worldCenter, transform.rotation, Vector3.one);
        Gizmos.DrawCube(Vector3.zero, detectionBoxSize);
    }
}