using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
public class MainMenu : MonoBehaviour
{

    [Header("Level Data")]
    [SerializeField] private int levelIndex = 0;
    [SerializeField] private Sprite[] levelSprites; // assign in inspector
    [SerializeField] private string[] levelSceneNames; // optional: scene names per level

    [Header("UI References")]
    [SerializeField] private Image levelImage;       // UI Image that shows the level preview
    [SerializeField] private TextMeshProUGUI levelText;

    private void Start()
    {
        levelImage.sprite = levelSprites[levelIndex];
        levelText.text = levelSceneNames[levelIndex];
    }

    public void LoadLevel(string level)
    {
        SceneManager.LoadScene(level);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void ChangeLevel(int direction)
    {
        // Move index
        levelIndex += direction;

        // Wrap around
        if (levelIndex < 0)
            levelIndex = levelSprites.Length - 1;
        else if (levelIndex >= levelSprites.Length)
            levelIndex = 0;

        // Update UI
        levelImage.sprite = levelSprites[levelIndex];
        levelText.text =  levelSceneNames[levelIndex];
    }

    public void LoadCurrentSelectedLevel()
    {
        SceneManager.LoadScene(levelSceneNames[levelIndex]);
    }
}
