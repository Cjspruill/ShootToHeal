using UnityEngine;
using TMPro;
public class HowToPlayManager : MonoBehaviour
{
    [SerializeField] private GameObject[] pagePanels; // assign all your panels in order
    private int currentPage = 0;
    [SerializeField] TextMeshProUGUI pageNumberText;

    void Start()
    {
        // Make sure only the first panel is active at start
        for (int i = 0; i < pagePanels.Length; i++)
            pagePanels[i].SetActive(i == currentPage);

        UpdatePageNumber();
    }

    public void ChangePanelLeft()
    {
        pagePanels[currentPage].SetActive(false);

        // Move left, wrap around
        currentPage = (currentPage - 1 + pagePanels.Length) % pagePanels.Length;

        pagePanels[currentPage].SetActive(true);
        UpdatePageNumber();
    }

    public void ChangePanelRight()
    {
        pagePanels[currentPage].SetActive(false);

        // Move right, wrap around
        currentPage = (currentPage + 1) % pagePanels.Length;

        pagePanels[currentPage].SetActive(true);
        UpdatePageNumber();
    }

    private void UpdatePageNumber()
    {
        // Show "Page X / Y" using concatenation
        pageNumberText.text = "Page " + (currentPage + 1) + " / " + pagePanels.Length;
    }
}