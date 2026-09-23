using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public Transform[] spawnPoints;
    float spawnInterval = 1.5f;
    public float firstSpawnDelay = 1f;
    public GameManager manager;

    private void Start()
    {
        StartCoroutine(
            SpawnLoop()
        );
        
    }

    private void Update()
    {
        //Debug.Log(manager.gameDuration);
    }

    private IEnumerator SpawnLoop()
    {
        
        yield return new WaitForSeconds(
            firstSpawnDelay
        );

        while (GameManager.Instance != null &&
               !GameManager.Instance.IsGameOver)
        {
            SpawnEnemy();

            yield return new WaitForSeconds(
                spawnInterval
            );
        }
    }

    private void SpawnEnemy()
    {
        if (spawnPoints == null ||
            spawnPoints.Length == 0)
        {
            return;
        }

        int randomIndex =
            Random.Range(
                0,
                spawnPoints.Length
            );

        Transform spawnPoint =
            spawnPoints[randomIndex];

        Instantiate(
            enemyPrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );
    }
}
