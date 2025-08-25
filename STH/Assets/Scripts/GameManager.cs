using UnityEngine;
using System.Collections;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [SerializeField] PlayerController playerController;
    [SerializeField] private LayerMask spawnIgnoreLayers;

    [Header("Enemy Settings")]
    [SerializeField] private GameObject[] enemiesToSpawn;   // [0] & [1] = normal, [2] = runner
    [SerializeField] private BoxCollider spawnArea;
    [SerializeField] private Transform enemyHolder;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI enemiesDefeatedText;
    [SerializeField] private TextMeshProUGUI currentLevelText;
    [SerializeField] private TextMeshProUGUI currentXPText;
    [SerializeField] private TextMeshProUGUI canSprintText;

    [Header("Obstacles")]
    [SerializeField] GameObject cubePrefab;
    [SerializeField] Transform obstacleHolder;

    [Header("Spawn Settings")]
    [SerializeField] private float baseSpawnInterval = 3f;
    [SerializeField] private float spawnIntervalDecrease = 0.2f;
    [SerializeField] private float minSpawnInterval = 0.5f;
    [SerializeField] private float maxSpawnInterval = 10f;
    [SerializeField] private int baseMaxEnemies = 5;
    [SerializeField] private int maxEnemiesIncrease = 2;
    [SerializeField] private int maxEnemiesCap = 50;
    [SerializeField] private int baseXP = 100;
    [SerializeField] private float xpMultiplier = 1.5f;

    [Header("Runner Settings")]
    [SerializeField] private float runnerSpawnInterval = 8f;   // how often to send runners
    [SerializeField] private int maxRunners = 3;               // clamp how many can exist

    [Header("Level Settings")]
    [SerializeField] private int level = 1;
    [SerializeField] private int totalEnemiesDestroyed;
    [SerializeField] private int enemiesSpawned;
    [SerializeField] bool levelEnded;
    [SerializeField] int numOfObstaclesToSpawn;
    [SerializeField] public bool showHealthBars; 
    [SerializeField] public bool showSprintSlider; 
    [SerializeField] public bool showXP;

    public delegate void GameEvent();
    public static event GameEvent OnLevelEnd;
    public static event GameEvent OnLevelStart;

    private Coroutine spawnRoutine;
    private Coroutine runnerRoutine;

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
        playerController = FindFirstObjectByType<PlayerController>();

        for (int i = 0; i < numOfObstaclesToSpawn; i++)
        {
            SpawnObstacles();
        }

        StartLevel();
    }

    private void Update()
    {
        enemiesDefeatedText.text = "Enemies Destroyed: " + totalEnemiesDestroyed.ToString();
        currentLevelText.text = "Level: " + level;
        currentXPText.text = "XP: " + playerController.GetXp + "/ " + GetMaxXpForLevel();

        if (playerController.isSprinting && playerController.sprintCooldownTimer < playerController.GetSprintCooldown)
            canSprintText.text = "Huff, Huff, Huff!";
        else if (playerController.sprintCooldownTimer <= 0)
            canSprintText.text = "I Can Sprint!";
        else
            canSprintText.text = "I'm Tired!";

        if (!levelEnded && playerController.GetXp >= GetMaxXpForLevel())
        {
            EndLevel();
        }
    }

    // ------------------- LEVEL CONTROL -------------------

    public void StartLevel()
    {
        levelEnded = false;
        level++;
        Time.timeScale = 1;
        OnLevelStart?.Invoke();

        if (spawnRoutine != null) StopCoroutine(spawnRoutine);
        if (runnerRoutine != null) StopCoroutine(runnerRoutine);

        spawnRoutine = StartCoroutine(SpawnLoop());         // normal enemies
        runnerRoutine = StartCoroutine(RunnerLoop());       // runners
    }

    public void EndLevel()
    {
        if (levelEnded) return;

        levelEnded = true;
        Time.timeScale = 0;
        OnLevelEnd?.Invoke();

        if (spawnRoutine != null) StopCoroutine(spawnRoutine);
        if (runnerRoutine != null) StopCoroutine(runnerRoutine);

        playerController.GetXp = 0;
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
                SpawnNormalEnemy();
            }

            yield return new WaitForSeconds(interval);
        }
    }

    private IEnumerator RunnerLoop()
    {
        while (true)
        {
            if (enemyHolder.childCount < GetMaxEnemiesForLevel())
            {
                int runnerCount = 0;
                foreach (Transform child in enemyHolder)
                {
                    if (child.CompareTag("Runner")) // make sure your runner prefab is tagged "Runner"
                        runnerCount++;
                }

                if (runnerCount < maxRunners)
                {
                    SpawnRunner();
                }
            }
            yield return new WaitForSeconds(runnerSpawnInterval);
        }
    }

    private void SpawnNormalEnemy()
    {
        if (enemiesToSpawn.Length < 2 || spawnArea == null) return;

        int randIndex = Random.Range(0, 2); // only first two
        Vector3 spawnLocation;
        if (FindSpawnLocation(out spawnLocation))
        {
            Instantiate(enemiesToSpawn[randIndex], spawnLocation, Quaternion.identity, enemyHolder);
            enemiesSpawned++;
        }
    }

    private void SpawnRunner()
    {
        if (enemiesToSpawn.Length < 3 || spawnArea == null) return;

        Vector3 spawnLocation;
        if (FindSpawnLocation(out spawnLocation))
        {
            GameObject runner = Instantiate(enemiesToSpawn[2], spawnLocation, Quaternion.identity, enemyHolder);
            runner.tag = "Runner"; // so we can track them
            enemiesSpawned++;
        }
    }

    private bool FindSpawnLocation(out Vector3 spawnLocation)
    {
        spawnLocation = Vector3.zero;
        int maxAttempts = 20;
        float checkRadius = 0.5f;

        for (int i = 0; i < maxAttempts; i++)
        {
            float randX = Random.Range(spawnArea.bounds.min.x, spawnArea.bounds.max.x);
            float randZ = Random.Range(spawnArea.bounds.min.z, spawnArea.bounds.max.z);
            spawnLocation = new Vector3(randX, 1.75f, randZ);

            Collider[] colliders = Physics.OverlapSphere(spawnLocation, checkRadius, ~spawnIgnoreLayers);
            if (colliders.Length == 0)
                return true;
        }

        Debug.Log("Couldn't find free spawn spot!");
        return false;
    }

    // ------------------- ENEMY TRACKING -------------------

    public void EnemyDestroyed()
    {
        totalEnemiesDestroyed++;
    }

    // ------------------- SCALING FUNCTIONS -------------------

    private int GetMaxEnemiesForLevel()
    {
        return Mathf.Clamp(baseMaxEnemies + (level - 1) * maxEnemiesIncrease, baseMaxEnemies, maxEnemiesCap);
    }

    private float GetMaxXpForLevel()
    {
        return Mathf.FloorToInt(baseXP * Mathf.Pow(xpMultiplier, level - 1));
    }

    private float GetSpawnIntervalForLevel()
    {
        return Mathf.Clamp(baseSpawnInterval - (level - 1) * spawnIntervalDecrease, minSpawnInterval, maxSpawnInterval);
    }

    // ------------------- OBSTACLES -------------------

    public void SpawnObstacles()
    {
        if (cubePrefab == null || spawnArea == null) return;

        Vector3 spawnLocation;
        if (!FindSpawnLocation(out spawnLocation)) return;

        Quaternion newRotation = Quaternion.Euler(0, 0, 0);
        GameObject newObstacle = Instantiate(cubePrefab, spawnLocation, newRotation, obstacleHolder);
        newObstacle.transform.localScale = new Vector3(Random.Range(2, 10), Random.Range(5, 10), Random.Range(2, 10));
    }
}