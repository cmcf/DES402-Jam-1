using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{

    // Reference to the obstacle prefab
    public GameObject obstaclePrefab;
    // Spawn point for the obstacle
    public Transform spawnPoint;

    // Obstacle move speed
    [SerializeField] float obstacleSpeed = 4f;
    // Time interval between obstacle spawns
    [SerializeField] float spawnInterval = 10f;
    // How long the obstacles stay on screen for
    [SerializeField] float obstacleLifeSpan = 20f;

    public bool canSpawnObstacle = false;

    

    void Start()
    {
        // Start spawning obstacles
        StartCoroutine(SpawnObstacles());
    }

    IEnumerator SpawnObstacles()
    {
        while (true)
        {
            if (canSpawnObstacle)
            {
                // Instantiate the obstacle at the spawn point
                GameObject newObstacle = Instantiate(obstaclePrefab, spawnPoint.position, spawnPoint.rotation);

                // Start moving the obstacle and destroy it after the set lifespan
                StartCoroutine(MoveObstacle(newObstacle, obstacleLifeSpan));

            }

            // Wait for the next spawn
            yield return new WaitForSeconds(spawnInterval);
        }
    }

     IEnumerator MoveObstacle(GameObject obstacle, float lifeSpan)
     {
        float elapsedTime = 0f;

        // Move the obstacle until it reaches its lifespan
        while (elapsedTime < lifeSpan && obstacle != null)
        {
            // Move the obstacle to the left
            obstacle.transform.Translate(Vector3.left * obstacleSpeed * Time.deltaTime);

            // Update elapsed time
            elapsedTime += Time.deltaTime;

            yield return null;
        }
        // Destroy the obstacle after its lifespan
        if (obstacle != null)
        {
            Destroy(obstacle);
        }

    }

    public void OnGameStart()
    {
        canSpawnObstacle = true;
    }
}
