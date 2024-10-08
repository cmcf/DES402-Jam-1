using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodSpawner : MonoBehaviour
{
    public GameObject foodPrefab;
    [SerializeField] float delayUntilNextSpawn = 5f; // Time between food spawns
    [SerializeField] float flashDuration = 1.5f; // Duration of the flash
    [SerializeField] float spawnRadius = 5f; // Radius around the spawner where items can spawn
    [SerializeField] int maxFood = 5; // Maximum amount of food on the screen at the same time
    [SerializeField] float minDistanceBetweenFood = 0.5f; // Minimum distance between food. Stops overlapping

    public bool canSpawnFood = false;

    // Stores all spawned food items
    List<GameObject> spawnedFood = new List<GameObject>();

    void Start()
    {
        StartCoroutine(SpawnFood());
    }

    // Method to trigger when the game starts
    public void OnGameStart()
    {
        // Enable food spawning when game has started
        canSpawnFood = true;
    }

    IEnumerator SpawnFood()
    {
        while (true)
        {
            // Only spawns if canSpawnFood is true
            if (canSpawnFood)
            {
                // Log before waiting to check the flow
                Debug.Log("Attempting to spawn food...");

                // Check how many food items are in the radius
                if (CountFoodInRadius() < maxFood)
                {
                    Vector2 spawnPos;
                    bool validPositionFound = false;
                    int attempts = 0;
                    int maxAttempts = 10;

                    while (!validPositionFound && attempts < maxAttempts)
                    {
                        spawnPos = GetRandomPosition();
                        if (!IsPositionOverlapping(spawnPos))
                        {
                            // If the position is valid, spawn the food item
                            GameObject newFoodPickup = Instantiate(foodPrefab, spawnPos, Quaternion.identity);
                            spawnedFood.Add(newFoodPickup);
                            // Food flashes
                            StartCoroutine(FlashFood(newFoodPickup));
                            validPositionFound = true;

                            // Log successful spawn
                            Debug.Log("Food spawned at position: " + spawnPos);
                        }
                        attempts++;
                    }
                }

                // Wait for the specified delay before attempting to spawn again
                yield return new WaitForSeconds(delayUntilNextSpawn);
            }
            else
            {
                yield return null; // Yield control until the next frame if spawning is disabled
            }
        }
    }

    IEnumerator FlashFood(GameObject food)
    {
        // Ensure the food object and its SpriteRenderer exist
        if (food == null) yield break;

        SpriteRenderer sr = food.GetComponent<SpriteRenderer>();
        if (sr == null) yield break;

        float elapsedTime = 0f;
        while (elapsedTime < flashDuration)
        {
            // If the food has been destroyed, exit the coroutine
            if (food == null) yield break;

            // Toggle visibility by enabling/disabling the sprite
            sr.enabled = !sr.enabled;

            // Flash every 0.2 seconds
            yield return new WaitForSeconds(0.2f);

            // Increase elapsed time
            elapsedTime += 0.2f;
        }

        // Ensure the food is visible after flashing 
        if (food != null)
        {
            sr.enabled = true;
        }
    }

    Vector2 GetRandomPosition()
    {
        // Get the spawner's position
        Vector2 spawnerPosition = transform.position;

        // Generate a random angle
        float angle = Random.Range(0f, Mathf.PI * 2);

        // Generate a random distance within the radius
        float distance = Random.Range(0f, spawnRadius);

        // Calculate the x and y coordinates based on the angle and distance
        float xPos = spawnerPosition.x + Mathf.Cos(angle) * distance;
        float yPos = spawnerPosition.y + Mathf.Sin(angle) * distance;

        return new Vector2(xPos, yPos);
    }

    bool IsPositionOverlapping(Vector2 position)
    {
        // Checks if the position overlaps with any existing food
        foreach (GameObject obstacle in spawnedFood)
        {
            if (obstacle != null)
            {
                float distance = Vector2.Distance(position, obstacle.transform.position);
                if (distance < minDistanceBetweenFood)
                {
                    return true;
                }
            }
        }
        // No overlap found
        return false; 
    }

    int CountFoodInRadius()
    {
        // Get the spawner's position
        Vector2 spawnerPosition = transform.position;

        // Count how many food items are within the spawn radius
        int count = 0;
        for (int i = spawnedFood.Count - 1; i >= 0; i--)
        {
            if (spawnedFood[i] == null)
            {
                // Remove any destroyed food items from the list
                spawnedFood.RemoveAt(i);
            }
            else
            {
                // Calculates the distance between the obstacle and the spawner
                float distance = Vector2.Distance(spawnerPosition, spawnedFood[i].transform.position);
                if (distance <= spawnRadius)
                {
                    count++;
                }
            }
        }
        return count;
    }
}
