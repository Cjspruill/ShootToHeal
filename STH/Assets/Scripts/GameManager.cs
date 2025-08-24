using UnityEngine;
using System.Collections;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Enemy Settings")]
    [SerializeField] private GameObject[] enemiesToSpawn;
    [SerializeField] private BoxCollider spawnArea;
    [SerializeField] private Transform enemyHolder;
    [SerializeField] private TextMeshProUGUI enemiesDefeatedText;
    [SerializeField] private TextMeshProUGUI currentLevelText;

    [Header("Spawn Settings")]
    [SerializeField]
    [Tooltip("Starting interval in seconds.")]
    private float baseSpawnInterval = 3f;   // starting interval (seconds)
    [SerializeField]
    [Tooltip("How much faster per level?")]
    private float spawnIntervalDecrease = 0.2f; // how much faster per level
    [SerializeField] private float minSpawnInterval = 0.5f;  // clamp min
    [SerializeField] private float maxSpawnInterval = 10f;   // clamp max
    [SerializeField] private int baseMaxEnemies = 5;         // starting max active enemies
    [SerializeField] private int maxEnemiesIncrease = 2;     // how many extra per level
    [SerializeField] private int maxEnemiesCap = 50;         // clamp cap
    [SerializeField] private int enemiesRequiredForNextLevel;

    [Header("Level Settings")]
    [SerializeField] private int level = 1;
    [SerializeField] private int enemiesDestroyed;
    [SerializeField] private int totalEnemiesDestroyed;
    [SerializeField] private int enemiesSpawned;

    public delegate void GameEvent();
    public static event GameEvent OnLevelEnd;
    public static event GameEvent OnLevelStart;

    private Coroutine spawnRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        StartLevel();
    }

    private void Update()
    {
        // You can use this to debug current enemy counts
        // Debug.Log($"Active: {enemyHolder.childCount} | Spawned: {enemiesSpawned} | Destroyed: {enemiesDestroyed}");

        enemiesDefeatedText.text = "Enemies Destroyed: " + totalEnemiesDestroyed.ToString();
        currentLevelText.text = "Level: " + level;
    }


    // ------------------- LEVEL CONTROL -------------------

    public void StartLevel()
    {
        level++;
        Time.timeScale = 1;
        OnLevelStart?.Invoke();

        if (spawnRoutine != null)
            StopCoroutine(spawnRoutine);

        spawnRoutine = StartCoroutine(SpawnLoop());
    }

    public void EndLevel()
    {
        Time.timeScale = 0;
        OnLevelEnd?.Invoke();

        if (spawnRoutine != null)
            StopCoroutine(spawnRoutine);
    }

    // ------------------- SPAWNING -------------------

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            int maxEnemies = GetMaxEnemiesForLevel();
            float interval = GetSpawnIntervalForLevel();

            if (enemyHolder.childCount < maxEnemies)
            {
                SpawnEnemy();
            }

            yield return new WaitForSeconds(interval);
        }
    }

    private void SpawnEnemy()
    {
        if (enemiesToSpawn.Length == 0 || spawnArea == null) return;

        int randEnemyToSpawn = Random.Range(0, enemiesToSpawn.Length);
        float randXValue = Random.Range(spawnArea.bounds.min.x, spawnArea.bounds.max.x);
        float randZValue = Random.Range(spawnArea.bounds.min.z, spawnArea.bounds.max.z);

        Vector3 spawnLocation = new Vector3(randXValue, 1.75f, randZValue);

        Instantiate(enemiesToSpawn[randEnemyToSpawn], spawnLocation, Quaternion.identity, enemyHolder);

        enemiesSpawned++;
    }

    // ------------------- ENEMY TRACKING -------------------

    public void EnemyDestroyed()
    {
        totalEnemiesDestroyed++;
        enemiesDestroyed++;

        if (enemiesDestroyed >= GetRequiredKillsForLevel())
        {
            enemiesDestroyed = 0;
            EndLevel();
        }
    }
        // ------------------- SCALING FUNCTIONS -------------------

        private int GetMaxEnemiesForLevel()
    {
        // scale and clamp
        return Mathf.Clamp(baseMaxEnemies + (level - 1) * maxEnemiesIncrease, baseMaxEnemies, maxEnemiesCap);
    }

    private float GetSpawnIntervalForLevel()
    {
        // scale and clamp
        return Mathf.Clamp(baseSpawnInterval - (level - 1) * spawnIntervalDecrease, minSpawnInterval, maxSpawnInterval);
    }

    private int GetRequiredKillsForLevel()
    {
        return level * enemiesRequiredForNextLevel;
    }
}
