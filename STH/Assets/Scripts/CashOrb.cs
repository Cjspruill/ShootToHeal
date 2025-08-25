using UnityEngine;

public class CashOrb : MonoBehaviour
{

    [SerializeField] public float cashAmountToGive;
    [SerializeField] GameObject audioPrefab;

    private void OnTriggerEnter(Collider other)
    {
        PlayerController playerController = other.GetComponent<PlayerController>();

        if (playerController != null)
        {
            playerController.GetCash += cashAmountToGive;
            PlayAudio();
            Destroy(gameObject);
        }
    }
    public void PlayAudio()
    {
        GameObject newAudioObj = Instantiate(audioPrefab, transform.position, Quaternion.identity);
        AudioSource newAudio = newAudioObj.GetComponent<AudioSource>();
        newAudio.Play();
        Destroy(newAudioObj, newAudio.clip.length);
    }
}
