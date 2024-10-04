using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodSpawner : MonoBehaviour
{
    public GameObject foodPrefab;
    [SerializeField] float spawnDelay = 5f; // Time between food spawns
    [SerializeField] float flashDuration = 1.5f; // Duration of the flash
    [SerializeField] float spawnRadius = 5f; // Radius around the spawner where items can spawn
    [SerializeField] int maxFood = 5; // Maximum amount of food on the screen at the same time
    [SerializeField] float minDistanceBetweenFood = 0.5f; // Minimum distance between food. Stops overlapping

    // Stores all spawned food items
    List<GameObject> spawnedFood = new List<GameObject>();

    void Start()
    {
        StartCoroutine(SpawnObstacles());
    }

    IEnumerator SpawnObstacles()
    {
        while (true)
        {
            // Spawns food after delay has passed
            yield return new WaitForSeconds(spawnDelay);

            // Check how many food items are in the radius
            if (CountFoodInRadius() < maxFood)
            {
                // Try to get a valid spawn position within the radius
                Vector2 spawnPos;
                bool validPositionFound = false;
                int attempts = 0;
                // Limits the number of attempts to find a non-overlapping position
                int maxAttempts = 10; 

                // Try finding a valid spawn position within the max number of attempts
                while (!validPositionFound && attempts < maxAttempts)
                {
                    spawnPos = GetRandomPosition();
                    if (!IsPositionOverlapping(spawnPos))
                    {
                        // If the position is valid, spawn the food item
                        GameObject newFoodPickup = Instantiate(foodPrefab, spawnPos, Quaternion.identity);
                        // Adds spawned food into current list
                        spawnedFood.Add(newFoodPickup);
                        // Food flashes
                        StartCoroutine(FlashFood(newFoodPickup));
                        validPositionFound = true; 
                    }
                    attempts++;
                }
            }
        }
    }

    IEnumerator FlashFood(GameObject food)
    {
        SpriteRenderer sr = food.GetComponent<SpriteRenderer>();
        float elapsedTime = 0f;
        while (elapsedTime < flashDuration)
        {
            if (food.GetComponent<SpriteRenderer>() != null)
            {
                // Toggle visibility by enabling/disabling the sprite
                sr.enabled = !sr.enabled;
                // Flash every 0.2 seconds
                yield return new WaitForSeconds(0.2f); 
                elapsedTime += 0.2f;
            }
        }
        // Ensure the food is visible after flashing
        sr.enabled = true;
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
