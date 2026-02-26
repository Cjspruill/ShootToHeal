using UnityEngine;

/// <summary>
/// Attach to the power-up/power-down orb prefab.
/// Positive = green sphere, Negative = red sphere.
/// On trigger with player → tells PowerUpManager to apply a random effect.
/// </summary>
public class PowerUpOrb : MonoBehaviour
{
    public enum OrbType { Positive, Negative }

    [SerializeField] public OrbType orbType = OrbType.Positive;
    [SerializeField] GameObject audioPrefab;
    [SerializeField] float lifetime = 15f; // auto-destroy if uncollected

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerController pc = other.GetComponent<PlayerController>();
        if (pc == null) return;

        if (PowerUpManager.Instance != null)
        {
            if (orbType == OrbType.Positive)
                PowerUpManager.Instance.ApplyRandomPositiveEffect(pc);
            else
                PowerUpManager.Instance.ApplyRandomNegativeEffect(pc);
        }

        PlayAudio();
        Destroy(gameObject);
    }

    void PlayAudio()
    {
        if (audioPrefab == null) return;
        GameObject obj = Instantiate(audioPrefab, transform.position, Quaternion.identity);
        AudioSource src = obj.GetComponent<AudioSource>();
        if (src != null) { src.Play(); Destroy(obj, src.clip.length); }
    }
}
