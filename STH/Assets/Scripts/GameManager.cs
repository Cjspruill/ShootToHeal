using UnityEngine;

public class GameManager : MonoBehaviour
{

    [SerializeField] GameObject[] enemiesToSpawn;
    [SerializeField] BoxCollider spawnArea;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InvokeRepeating("SpawnEnemy", 5f, 10f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void SpawnEnemy()
    {
        int randEnemyToSpawn = Random.Range(0, enemiesToSpawn.Length-1);
        float randXValue = Random.Range(spawnArea.bounds.min.x, spawnArea.bounds.max.x);
        float randZValue = Random.Range(spawnArea.bounds.min.z, spawnArea.bounds.max.z);

        
        Vector3 spawnLocation = new Vector3(randXValue, 1.75f, randZValue);

        Instantiate(enemiesToSpawn[randEnemyToSpawn], spawnLocation, Quaternion.identity);
    }
}
