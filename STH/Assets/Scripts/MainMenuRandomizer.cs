using UnityEngine;
using System.Collections;

public class MainMenuRandomizer : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private GameObject cubePrefab;
    [SerializeField] private BoxCollider spawnArea;
    [SerializeField] private Transform obstacleHolder;
    [SerializeField] private LayerMask obstacleLayer; // Layer for obstacles only

    [Header("Spawn Settings")]
    [SerializeField] private int numOfObstaclesToSpawn = 10;
    [SerializeField] private float spawnDelay = 0.5f;

    void Start()
    {
        StartCoroutine(SpawnObstaclesRoutine());
    }

    private IEnumerator SpawnObstaclesRoutine()
    {
        for (int i = 0; i < numOfObstaclesToSpawn; i++)
        {
            SpawnObstacle();
            yield return new WaitForSeconds(spawnDelay);
        }
    }

    private void SpawnObstacle()
    {
        if (cubePrefab == null || spawnArea == null) return;

        // Pick a random size for the obstacle
        Vector3 randomScale = new Vector3(
            Random.Range(2f, 10f),
            Random.Range(5f, 10f),
            Random.Range(2f, 10f)
        );

        Vector3 spawnLocation = GetRandomSpawnLocation(randomScale);

        // Instantiate and set layer
        GameObject newObstacle = Instantiate(cubePrefab, spawnLocation, Quaternion.identity, obstacleHolder);
        newObstacle.transform.localScale = randomScale;
        newObstacle.GetComponent<MeshRenderer>().material.color = Random.ColorHSV();
        newObstacle.layer = LayerMaskToLayer(obstacleLayer); // assign single layer
    }

    private Vector3 GetRandomSpawnLocation(Vector3 objectSize)
    {
        int maxAttempts = 30;
        Vector3 halfExtents = objectSize * 0.5f;

        for (int i = 0; i < maxAttempts; i++)
        {
            float randX = Random.Range(spawnArea.bounds.min.x + halfExtents.x, spawnArea.bounds.max.x - halfExtents.x);
            float randZ = Random.Range(spawnArea.bounds.min.z + halfExtents.z, spawnArea.bounds.max.z - halfExtents.z);
            float y = objectSize.y / 2f;

            Vector3 spawnLocation = new Vector3(randX, y, randZ);

            // Check for collisions only with existing obstacles
            Collider[] colliders = Physics.OverlapBox(spawnLocation, halfExtents, Quaternion.identity, obstacleLayer);
            if (colliders.Length == 0)
                return spawnLocation;
        }

        // If all attempts fail, just place inside bounds anyway
        float finalX = Random.Range(spawnArea.bounds.min.x + halfExtents.x, spawnArea.bounds.max.x - halfExtents.x);
        float finalZ = Random.Range(spawnArea.bounds.min.z + halfExtents.z, spawnArea.bounds.max.z - halfExtents.z);
        float finalY = objectSize.y / 2f;
        return new Vector3(finalX, finalY, finalZ);
    }

    // Convert a single LayerMask to its integer layer index (0-31)
    private int LayerMaskToLayer(LayerMask mask)
    {
        int layer = 0;
        int maskValue = mask.value;
        while (maskValue > 1)
        {
            maskValue = maskValue >> 1;
            layer++;
        }
        return layer;
    }
}
