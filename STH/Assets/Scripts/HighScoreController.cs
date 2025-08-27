using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public class HighScoreEntry
{
    public string playerName;
    public int score;

    public HighScoreEntry(string name, int score)
    {
        this.playerName = name;
        this.score = score;
    }
}

public class HighScoreController : MonoBehaviour
{
    public static HighScoreController Instance { get; private set; }

    [Header("Setup")]
    [SerializeField] private GameObject highScoreHolderPrefab; // Prefab with TMP_Text refs
    [SerializeField] private Transform highScoreParent; // Where to spawn the UI
    [SerializeField] private int maxHighScores = 11;

    [Header("UI Input")]
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private GameObject nameEntryPanel; // Panel to toggle

    private List<HighScoreEntry> highScores = new List<HighScoreEntry>();

    private void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadHighScores();
        DisplayHighScores();
    }

    /// <summary>
    /// Adds a new score to the table and saves it.
    /// </summary>
    public void AddNewScore(string playerName, int score)
    {
        HighScoreEntry newEntry = new HighScoreEntry(playerName, score);
        highScores.Add(newEntry);

        // Sort by score descending
        highScores.Sort((a, b) => b.score.CompareTo(a.score));

        // Keep only top maxHighScores
        if (highScores.Count > maxHighScores)
            highScores.RemoveRange(maxHighScores, highScores.Count - maxHighScores);

        SaveHighScores();
        RefreshDisplay();
    }

    /// <summary>
    /// Called from the UI button to submit a new score.
    /// </summary>
    public void AddScoreFromUI()
    {
        string name = nameInputField != null && !string.IsNullOrWhiteSpace(nameInputField.text)
            ? nameInputField.text
            : "AAA";

        int score = 0;

        score = GameManager.Instance.GetScore();

        AddNewScore(name, score);
    }

    private void LoadHighScores()
    {
        highScores.Clear();

        // Load existing scores
        for (int i = 0; i < maxHighScores; i++)
        {
            string name = PlayerPrefs.GetString($"HighScoreName{i}", "AAA");
            int score = PlayerPrefs.GetInt($"HighScoreValue{i}", 0);
            highScores.Add(new HighScoreEntry(name, score));
        }

        // Check if current game score beats any existing score
        bool isNewHighScore = false;
        if (GameManager.Instance != null)
        {
            int currentScore = GameManager.Instance.GetScore();
            foreach (var entry in highScores)
            {
                if (currentScore > entry.score)
                {
                    isNewHighScore = true;
                    break;
                }
            }
        }

        // Toggle the name entry panel based on if it's a high score
        if (nameEntryPanel != null)
            nameEntryPanel.SetActive(isNewHighScore);
    }

    private void SaveHighScores()
    {
        for (int i = 0; i < highScores.Count; i++)
        {
            PlayerPrefs.SetString($"HighScoreName{i}", highScores[i].playerName);
            PlayerPrefs.SetInt($"HighScoreValue{i}", highScores[i].score);
        }

        PlayerPrefs.Save();
    }

    private void DisplayHighScores()
    {
        foreach (Transform child in highScoreParent)
            Destroy(child.gameObject);

        for (int i = 0; i < highScores.Count; i++)
        {
            GameObject holder = Instantiate(highScoreHolderPrefab, highScoreParent);
            TMP_Text[] texts = holder.GetComponentsInChildren<TMP_Text>();

            foreach (TMP_Text text in texts)
            {
                if (text.name == "NameText")
                    text.text = highScores[i].playerName;
                else if (text.name == "ScoreText")
                    text.text = highScores[i].score.ToString();
            }
        }
    }

    private void RefreshDisplay()
    {
        DisplayHighScores();
    }
}
