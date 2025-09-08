using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;
using Unity.Cinemachine;
using System.Collections.Generic;
using System.Linq;

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
    [SerializeField] private TextMeshProUGUI currentCashText;
    [SerializeField] private TextMeshProUGUI canSprintText;

    [Header("Obstacles")]
    [SerializeField] private List<GameObject> obstaclePrefabs; // drag multiple prefabs here
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
    [SerializeField] public bool levelEnded;
    [SerializeField] int numOfObstaclesToSpawn;
    [SerializeField] public bool showHealthBars;
    [SerializeField] public bool showSprintSlider;
    [SerializeField] public bool doubleGunsActive;
    [SerializeField] public bool machineGunActive;
    [SerializeField] public bool shotgunActive;
    [SerializeField] public bool flamethrowerActive;
    [SerializeField] public bool aiMeleeBotActive;
    [SerializeField] public bool aiRangedBotActive;
    [SerializeField] public bool showTargetReticle;

    [SerializeField] public bool showXP;

    public delegate void GameEvent();
    public static event GameEvent OnLevelEnd;
    public static event GameEvent OnLevelStart;
    public static event GameEvent OnGameOver;

    private Coroutine spawnRoutine;
    private Coroutine runnerRoutine;

    float score;

    public bool tutorialEnemySpawned;

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
        playerController = FindFirstObjectByType<PlayerController>();
        enemiesDefeatedText = GameObject.FindGameObjectWithTag("EnemiesDefeatedText").GetComponent<TextMeshProUGUI>();
        currentLevelText = GameObject.FindGameObjectWithTag("CurrentLevelText").GetComponent<TextMeshProUGUI>();
        currentXPText = GameObject.FindGameObjectWithTag("CurrentXPText").GetComponent<TextMeshProUGUI>();
        currentCashText = GameObject.FindGameObjectWithTag("CurrentCashText").GetComponent<TextMeshProUGUI>();
        canSprintText = GameObject.FindGameObjectWithTag("CanSprintText").GetComponent<TextMeshProUGUI>();

        currentLevelText.gameObject.SetActive(false);
        currentXPText.gameObject.SetActive(false);
        canSprintText.gameObject.SetActive(false);

        for (int i = 0; i < numOfObstaclesToSpawn; i++)
        {
            StartCoroutine(SpawnObstacles());
        }

        if (TutorialManager.Instance != null && TutorialManager.Instance.isTutorial) return;     
        
        //StartLevel();
        
    }




    private void Update()
    {

        
        enemiesDefeatedText.text = "Enemies Destroyed: " + totalEnemiesDestroyed.ToString();
        currentLevelText.text = "Level: " + level;
        currentXPText.text = "XP: " + playerController.GetXp + "/ " + GetMaxXpForLevel();
        currentCashText.text = "Cash: " + playerController.GetCash;

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



        if (TutorialManager.Instance != null && TutorialManager.Instance.isTutorial)
        {
            var currentPage = TutorialManager.Instance.GetTutorialPages[TutorialManager.Instance.GetCurrentPageIndex];

            if(currentPage.header == "Spawn Grunt" && !tutorialEnemySpawned)
            {
                SpawnNormalEnemy();
                TutorialManager.Instance.CompleteStep();
                tutorialEnemySpawned = true;
            }

             if(currentPage.header == "Runners" && !tutorialEnemySpawned)
            {
                SpawnRunner();
                TutorialManager.Instance.CompleteStep();
                tutorialEnemySpawned = true;
            }

             if(currentPage.header == "Tank" && !tutorialEnemySpawned)
            {
                SpawnTank();
                TutorialManager.Instance.CompleteStep();
                tutorialEnemySpawned = true;
            }
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

        if (TutorialManager.Instance != null && TutorialManager.Instance.isTutorial) return;

        spawnRoutine = StartCoroutine(SpawnLoop());         // normal enemies
        runnerRoutine = StartCoroutine(RunnerLoop());       // runners
    }

    public void EndLevel()
    {
        if (levelEnded) return;



        //// Only reward every 5 levels between 5 and 100
        if (level > 0 && level <= 1000 && level % 5 == 0)
        {
            if (playerController.health.GetHealth > 25)
            {
                if (LevelPlayAds.Instance.IsCashAdReady())
                    LevelPlayAds.Instance.ShowCashAd();
                else
                {
                    Debug.Log("Cash ad not ready. Loading...");
                    LevelPlayAds.Instance.LoadCashAd();
                }
            }
        else if(playerController.health.GetHealth <= 25)
            {
                if (LevelPlayAds.Instance.IsHealthAdReady())
                    LevelPlayAds.Instance.ShowHealthAd();
                else
                {
                    Debug.Log("Health ad not ready. Loading...");
                    LevelPlayAds.Instance.LoadHealthAd();
                }
            }
        }

        levelEnded = true;
        Time.timeScale = 0;
        OnLevelEnd?.Invoke();

        if (spawnRoutine != null) StopCoroutine(spawnRoutine);
        if (runnerRoutine != null) StopCoroutine(runnerRoutine);

        playerController.GetXp = 0;
    }

    public void GameOver()
    {
        Time.timeScale = 0;
        OnGameOver?.Invoke();

        if (spawnRoutine != null) StopCoroutine(spawnRoutine);
        if (runnerRoutine != null) StopCoroutine(runnerRoutine);
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

        int enemyIndex = 0;
        if (level >= 10)
        {
            float tankChance = Mathf.InverseLerp(10, 200, level);
            if (Random.value < tankChance)
                enemyIndex = 1; // tank
        }

        GameObject prefab = enemiesToSpawn[enemyIndex];

        // calculate collider bounds to know size
        Vector3 size = GetPrefabBoundsSize(prefab);

        Vector3 spawnLocation;
        if (FindSpawnLocation(out spawnLocation, size))
        {
            Instantiate(prefab, spawnLocation, Quaternion.identity, enemyHolder);
            enemiesSpawned++;
        }
    }


    private void SpawnRunner()
    {
        if (enemiesToSpawn.Length < 3 || spawnArea == null) return;

        GameObject prefab = enemiesToSpawn[2];
        Vector3 size = GetPrefabBoundsSize(prefab);

        Vector3 spawnLocation;
        if (FindSpawnLocation(out spawnLocation, size))
        {
            GameObject runner = Instantiate(prefab, spawnLocation, Quaternion.identity, enemyHolder);
            runner.tag = "Runner";
            enemiesSpawned++;
        }
    }

    void SpawnTank()
    {
        GameObject prefab = enemiesToSpawn[1];

        // calculate collider bounds to know size
        Vector3 size = GetPrefabBoundsSize(prefab);

        Vector3 spawnLocation;
        if (FindSpawnLocation(out spawnLocation, size))
        {
            Instantiate(prefab, spawnLocation, Quaternion.identity, enemyHolder);
            enemiesSpawned++;
        }
    }

    // helper to calculate prefab size
    private Vector3 GetPrefabBoundsSize(GameObject prefab)
    {
        Collider col = prefab.GetComponentInChildren<Collider>();
        if (col != null)
            return col.bounds.size;

        // fallback: arbitrary small size if no collider
        return Vector3.one;
    }

    private bool FindSpawnLocation(out Vector3 spawnLocation, Vector3 objectSize, float paddingXZ = 2f, Quaternion? rotation = null)
    {
        spawnLocation = Vector3.zero;
        int maxAttempts = 30;

        // Half extents: add padding only to X/Z so tall obstacles don’t fail
        Vector3 halfExtents = new Vector3(
            (objectSize.x * 0.5f) + paddingXZ,
            objectSize.y * 0.5f,
            (objectSize.z * 0.5f) + paddingXZ
        );

        Quaternion rot = rotation ?? Quaternion.identity;

        for (int i = 0; i < maxAttempts; i++)
        {
            float randX = Random.Range(spawnArea.bounds.min.x, spawnArea.bounds.max.x);
            float randZ = Random.Range(spawnArea.bounds.min.z, spawnArea.bounds.max.z);

            spawnLocation = new Vector3(randX, objectSize.y / 2f, randZ);

            Collider[] colliders = Physics.OverlapBox(
                spawnLocation,
                halfExtents,
                rot,
                ~spawnIgnoreLayers
            );

            if (colliders.Length == 0)
                return true;
        }

        Debug.LogWarning("Couldn't find free spawn spot after " + maxAttempts + " attempts!");
        return false;
    }

    private bool FindObstacleSpawnLocation(GameObject prefab, out Vector3 spawnLocation, Quaternion rotation, Vector3 scale, int maxAttempts = 30)
    {
        spawnLocation = Vector3.zero;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            // Random X/Z position inside spawn area
            float randX = Random.Range(spawnArea.bounds.min.x, spawnArea.bounds.max.x);
            float randZ = Random.Range(spawnArea.bounds.min.z, spawnArea.bounds.max.z);
            Vector3 candidatePos = new Vector3(randX, 0f, randZ);

            // Temporary instance
            GameObject temp = Instantiate(prefab, candidatePos, rotation);
            temp.SetActive(false); // hide it during check
            temp.transform.localScale = scale;

            bool blocked = false;

            // Find the "Colliders" sub-object anywhere in the hierarchy
            Transform collidersRoot = null;
            foreach (Transform t in temp.GetComponentsInChildren<Transform>(true))
            {
                if (t.name == "Colliders")
                {
                    collidersRoot = t;
                    break;
                }
            }

            if (collidersRoot == null)
            {
                Debug.LogWarning($"Prefab '{prefab.name}' has no 'Colliders' sub-object! Attempt #{attempt + 1}");
                Destroy(temp);
                continue;
            }

            // Check each BoxCollider
            foreach (BoxCollider col in collidersRoot.GetComponentsInChildren<BoxCollider>(true))
            {
                // Calculate world-space center
                Vector3 worldCenter = col.transform.position + col.transform.rotation * Vector3.Scale(col.center, col.transform.lossyScale);

                // Calculate half-size in world space
                Vector3 worldHalfSize = Vector3.Scale(col.size, col.transform.lossyScale) * 0.5f;

                // Check for overlaps
                if (Physics.CheckBox(worldCenter, worldHalfSize, col.transform.rotation, ~spawnIgnoreLayers))
                {
                    blocked = true;
                    break;
                }
            }

            Destroy(temp);

            if (!blocked)
            {
                spawnLocation = candidatePos;
                return true; // valid placement
            }
        }

        return false; // failed to find a spot
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

    public IEnumerator SpawnObstacles()
    {
        if (obstaclePrefabs == null || obstaclePrefabs.Count == 0 || spawnArea == null)
            yield break;

        int maxAttempts = 50;
        int obstacleLayer = LayerMask.NameToLayer("NavMeshObstacleIgnore");

        // Pick a random prefab
        int randomIndex = Random.Range(0, obstaclePrefabs.Count);
        GameObject prefabToSpawn = obstaclePrefabs[randomIndex];

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            // Random XZ position within spawn area
            float randX = Random.Range(spawnArea.bounds.min.x, spawnArea.bounds.max.x);
            float randZ = Random.Range(spawnArea.bounds.min.z, spawnArea.bounds.max.z);
            Vector3 candidatePos = new Vector3(randX, spawnArea.bounds.max.y + 20f, randZ);

            // Instantiate ghost
            GameObject ghost = Instantiate(prefabToSpawn, candidatePos, Quaternion.identity, obstacleHolder);

            // Set scale
            ghost.transform.localScale = (randomIndex == 0)
                ? new Vector3(Random.Range(2, 10), Random.Range(5, 10), Random.Range(2, 10))
                : Vector3.one;

            // Set rotation
            Quaternion rotation;
            if (randomIndex == 0)
                rotation = Quaternion.identity;
            else if (randomIndex == 1 || randomIndex == 2)
                rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            else
                rotation = Quaternion.identity;
            ghost.transform.rotation = rotation;

            // Set layer recursively
            SetLayerRecursively(ghost, obstacleLayer);

            // Disable renderers until confirmed placement
            foreach (Renderer r in ghost.GetComponentsInChildren<Renderer>())
                r.enabled = false;

            // Enable placement colliders
            Transform collidersRoot = FindCollidersDeep(ghost.transform);
            if (collidersRoot == null)
            {
                Debug.LogWarning($"Prefab '{prefabToSpawn.name}' missing 'Colliders' sub-object!");
                Destroy(ghost);
                yield break;
            }

            BoxCollider[] placementColliders = collidersRoot.GetComponentsInChildren<BoxCollider>();
            foreach (BoxCollider c in placementColliders)
                c.enabled = true;

            // Raycast down to find ground height (ignore layers in spawnIgnoreLayers)
            if (!Physics.Raycast(candidatePos, Vector3.down, out RaycastHit hit, 100f, ~spawnIgnoreLayers))
            {
                Destroy(ghost);
                continue;
            }
            ghost.transform.position = hit.point + Vector3.up * 0.01f;

            // Wait a physics frame
            yield return new WaitForFixedUpdate();

            // --------- Accurate overlap check using Bounds.Intersects ---------
            bool intersects = false;

            foreach (BoxCollider c in placementColliders)
            {
                Bounds worldBounds = c.bounds;

                foreach (Transform otherObstacle in obstacleHolder)
                {
                    if (otherObstacle.gameObject == ghost) continue;

                    BoxCollider[] otherColliders = otherObstacle.GetComponentsInChildren<BoxCollider>();
                    foreach (BoxCollider other in otherColliders)
                    {
                        if (worldBounds.Intersects(other.bounds))
                        {
                            intersects = true;
                            break;
                        }
                    }
                    if (intersects) break;
                }

                if (intersects) break;
            }
            // -----------------------------------------------------------------

            if (!intersects)
            {
                // Valid placement: show renderers
                foreach (Renderer r in ghost.GetComponentsInChildren<Renderer>())
                    r.enabled = true;

                // Keep Colliders sub-object for gameplay
                yield break; // placed successfully
            }

            // Failed attempt
            Destroy(ghost);
        }

        Debug.LogWarning($"Could not place obstacle '{prefabToSpawn.name}' after {maxAttempts} attempts!");
    }


    // --- Helper Methods ---
    private Transform FindCollidersDeep(Transform parent)
    {
        foreach (Transform child in parent)
        {
            if (child.name == "Colliders") return child;
            Transform found = FindCollidersDeep(child);
            if (found != null) return found;
        }
        return null;
    }

    private void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform t in obj.GetComponentsInChildren<Transform>())
            t.gameObject.layer = layer;
    }
    


    public int GetScore()
    {
        int baseScore = (totalEnemiesDestroyed * 10);   // 10 points per enemy
        float xpScore = playerController.GetXp * 2;     // XP gives some extra points
        float cashScore = playerController.GetCash * 5;   // Cash is worth more
        int levelBonus = level * 100;                   // Surviving levels adds flat bonus

        int scoreRounded = Mathf.RoundToInt(baseScore + xpScore + cashScore + levelBonus);

        return scoreRounded;
    }

    public int GetLevel()
    {
        return level;
    }

    public int GetEnemiesDefeated()
    {
        return totalEnemiesDestroyed;
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        Debug.Log("Game paused.");
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        Debug.Log("Game resumed.");
    }
}