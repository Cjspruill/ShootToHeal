using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public class HighScoreEntry
{
    string playerName;
    int score;
    int level;
    int enemiesDefeated;

    public HighScoreEntry(string playerName, int level, int enemiesDefeated, int score)
    {
        this.playerName = playerName;
        this.level = level;
        this.enemiesDefeated = enemiesDefeated;
        this.score = score;
    }
}

public class HighScoreController : MonoBehaviour
{
    public static HighScoreController Instance { get; private set; }

    [Header("Setup")]
    [SerializeField] private GameObject highScoreHolderPrefab; // Prefab with TMP_Text refs
    [SerializeField] private Transform highScoreParent; // Where to spawn the UI
    [SerializeField] private readonly int maxHighScores = 11;

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
    public void AddNewScore(string playerName,int level, int enemiesDefeated, int score)
    {
        HighScoreEntry newEntry = new HighScoreEntry(playerName, level, enemiesDefeated, score);
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
        int level = 0;
        int enemiesDefeated = 0;

        score = GameManager.Instance.GetScore();
        level = GameManager.Instance.GetLevel();
        enemiesDefeated = GameManager.Instance.GetEnemiesDefeated();

        AddNewScore(name, level, enemiesDefeated, score);
    }

    private void LoadHighScores()
    {
        highScores.Clear();

        // Load existing scores
        for (int i = 0; i < maxHighScores; i++)
        {
            string name = PlayerPrefs.GetString($"HighScoreName{i}", "AAA");
            int level = PlayerPrefs.GetInt($"LevelValue{i}", 1);
            int enemiesDefeated = PlayerPrefs.GetInt($"EnemiesDefeatedValue{i}");
            int score = PlayerPrefs.GetInt($"HighScoreValue{i}");
            highScores.Add(new HighScoreEntry(name, level, enemiesDefeated, score));
        }

        // Check if current game score beats any existing score
        if (GameManager.Instance != null)
        {
            bool isNewHighScore = highScores.Any(entry => GameManager.Instance.GetScore() > entry.score);

            // Toggle the name entry panel based on if it's a high score
            if (isNewHighScore) {
                nameEntryPanel?.SetActive(isNewHighScore);
            }
        }
    }

    private void SaveHighScores()
    {
        for (int i = 0; i < highScores.Count; i++)
        {
            PlayerPrefs.SetString($"HighScoreName{i}", highScores[i].playerName);
            PlayerPrefs.SetInt($"LevelValue{i}", highScores[i].level);
            PlayerPrefs.SetInt($"EnemiesDefeatedValue{i}", highScores[i].enemiesDefeated);
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
                else if (text.name == "LevelText")
                    text.text = highScores[i].level.ToString();
                else if (text.name == "EnemiesDefeatedText")
                    text.text = highScores[i].enemiesDefeated.ToString();
                else if (text.name == "ScoreText")
                    text.text = highScores[i].score.ToString();
            }
        }
    }

    private void RefreshDisplay()
    {
        DisplayHighScores();
    }

    public void ShowHighScores()
    {
        highScores.Clear();

        // Load existing scores
        for (int i = 0; i < maxHighScores; i++)
        {
            string name = PlayerPrefs.GetString($"HighScoreName{i}", "AAA");
            int level = PlayerPrefs.GetInt($"LevelValue{i}", 1);
            int enemiesDefeated = PlayerPrefs.GetInt($"EnemiesDefeatedValue{i}");
            int score = PlayerPrefs.GetInt($"HighScoreValue{i}");

            highScores.Add(new HighScoreEntry(name, level, enemiesDefeated, score));
        }

        DisplayHighScores();
    }
}
