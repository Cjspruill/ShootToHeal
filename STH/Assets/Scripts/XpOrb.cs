using UnityEngine;

public class XpOrb : MonoBehaviour
{
    [SerializeField] public float xpToGive;
    [SerializeField] GameObject audioPrefab;

    private void Start()
    {
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerController playerController = other.GetComponent<PlayerController>();

        if(playerController != null)
        {
            playerController.GetXp += xpToGive;
            PlayAudio();
            gameObject.SetActive(false);

            if (TutorialManager.Instance != null && TutorialManager.Instance.isTutorial)
            {
                var currentPage = TutorialManager.Instance.GetTutorialPages[TutorialManager.Instance.GetCurrentPageIndex];

                if (currentPage.header == "Orb Collect")
                {
                    TutorialManager.Instance.CompleteStep();
                }

            }


            Destroy(gameObject,2f);
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
