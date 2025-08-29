using UnityEngine;

public class EnemyTutorial : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TutorialDeath()
    {
        var tutorial = TutorialManager.Instance;
        if (tutorial == null) return;

        int index = tutorial.GetCurrentPageIndex;
        if (index >= tutorial.GetTutorialPages.Count) return;

        var currentPage = tutorial.GetTutorialPages[index];

        switch (currentPage.header)
        {
            case "Enemy Grunt Death":
            case "Runner Enemy Death":
            case "Tank Enemy Death":
                tutorial.CompleteStep();
                GameManager.Instance.tutorialEnemySpawned = false;
                break;
        }

        if (currentPage.header == "Runner Enemy Death")
        {
            PlayerController playerController = FindFirstObjectByType<PlayerController>();
            if (playerController != null)
            {
                playerController.GetBulletDamage += 2;
                playerController.GetFireRate -= 0.25f;
                playerController.GetBulletKnockback += 2.5f;
                GameManager.Instance.showHealthBars = true;
            }
        }
    }
}
