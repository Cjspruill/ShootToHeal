using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

[System.Serializable]
public class TutorialPage
{
    public string header;
    [TextArea] public string explanation;
    public bool requiresCondition;      // If true, wait for condition before continuing
    public bool pauseGame;              // Should the game be paused on this step?
    public bool showPanel = true;       // Should the panel be visible on this step?
    public bool showNextButton = true;  // Should the Next button be shown?
}

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private TextMeshProUGUI headerText;
    [SerializeField] private TextMeshProUGUI explanationText;
    [SerializeField] private Button backButton;
    [SerializeField] private Button nextButton;
    [SerializeField] public bool isTutorial;

    [Header("Pages")]
    [SerializeField] private List<TutorialPage> tutorialPages = new List<TutorialPage>();

    private int currentPageIndex = 0;
    private bool canContinue = false;
    private bool conditionMet = false;

    public List<TutorialPage> GetTutorialPages { get => tutorialPages; set => tutorialPages = value; }
    public int GetCurrentPageIndex { get => currentPageIndex; set => currentPageIndex = value; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (!isTutorial) return;

        gameObject.transform.SetParent(GameObject.FindGameObjectWithTag("TutorialSlot").transform);
        tutorialPanel.SetActive(false);

        backButton.onClick.AddListener(OnBack);
        nextButton.onClick.AddListener(OnNext);

        StartCoroutine(RunTutorial());
    }

    private IEnumerator RunTutorial()
    {
        yield return new WaitForSeconds(1);
        tutorialPanel.SetActive(true);
        Time.timeScale = 0f;

        currentPageIndex = 0;

        while (currentPageIndex < tutorialPages.Count)
        {
            TutorialPage page = tutorialPages[currentPageIndex];

            // Reset flags per page
            canContinue = false;
            conditionMet = false;

            // Show the page (updates text, buttons, panel visibility)
            ShowPage(currentPageIndex);

            // 1️⃣ Wait for requirement if needed
            if (page.requiresCondition)
            {
                yield return new WaitUntil(() => conditionMet);
            }

            // 2️⃣ Handle advancement
            if (page.showPanel)
            {
                // Visible page → show Next button and wait for click
                if (page.showNextButton)
                    nextButton.gameObject.SetActive(true);

                yield return new WaitUntil(() => canContinue);
            }
            else
            {
                // Hidden page → auto-advance after requirement is met
                canContinue = true;
                yield return new WaitUntil(() => canContinue);
            }

            // Advance to next page
            currentPageIndex++;
        }

        EndTutorial();
    }

    private void ShowPage(int index)
    {
        if (index >= tutorialPages.Count) return;

        currentPageIndex = index;
        TutorialPage page = tutorialPages[currentPageIndex];

        // Panel visibility
        tutorialPanel.SetActive(page.showPanel);

        if (page.showPanel)
        {
            headerText.text = page.header;
            explanationText.text = page.explanation;
        }

        // Pause game
        Time.timeScale = page.pauseGame ? 0f : 1f;

        // Back button
        backButton.gameObject.SetActive(currentPageIndex > 0 && page.showPanel);

        // Next button logic: visible only if allowed
        nextButton.gameObject.SetActive(page.showPanel && (!page.requiresCondition || conditionMet) && page.showNextButton);
    }

    private void OnBack()
    {
        if (tutorialPages.Count == 0) return;

        currentPageIndex = Mathf.Clamp(currentPageIndex - 1, 0, tutorialPages.Count - 1);
        ShowPage(currentPageIndex);
    }

    private void OnNext()
    {
        canContinue = true;
        nextButton.gameObject.SetActive(false);
    }
    /// <summary>
    /// Call this from another script when the player has met the condition.
    /// Example: TutorialManager.Instance.CompleteStep();
    /// </summary>
    public void CompleteStep()
    {
        TutorialPage page = tutorialPages[currentPageIndex];

        // Mark the requirement as met
        conditionMet = true;

        if (page.showPanel)
        {
            // Visible pages → unlock Next button
            if (page.showNextButton)
                nextButton.gameObject.SetActive(true);
        }
        else
        {
            // Hidden pages → auto-advance
            canContinue = true;
        }
    }


    private void EndTutorial()
    {
        tutorialPanel.SetActive(false);
        Time.timeScale = 1f;
        Debug.Log("Tutorial Finished!");
    }
}
