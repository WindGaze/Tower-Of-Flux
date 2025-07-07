using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AncestralSpawner : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    public GameObject[] enemyPrefabs; // Array to assign multiple enemy prefabs

    [Header("Spawn Settings")]
    public int minEnemies = 4; // Minimum number of enemies to spawn
    public int maxEnemies = 6; // Maximum number of enemies to spawn
    public float minSpawnDistance = 2f; // Minimum distance from the player
    public float minWallDistance = 1.5f; // Minimum distance from any walls

    public LayerMask wallLayer; // Layer for walls
    public Transform spawnAnchor; // The parent object under which enemies will be spawned

    private Collider2D spawnAreaCollider; // Collider that defines the spawn area
    private List<GameObject> spawnedEnemies = new List<GameObject>(); // List of spawned enemies

    void Start()
    {
        // Get the collider attached to this object
        spawnAreaCollider = GetComponent<Collider2D>();
    }

    // Method to spawn enemies, ensuring they are inside the collider area, not close to the player or walls
    public void SpawnRandomEnemies(Vector2 playerPosition)
    {
        // Random number of enemies to spawn
        int enemyCount = Random.Range(minEnemies, maxEnemies);

        for (int i = 0; i < enemyCount; i++)
        {
            Vector2 spawnPosition;

            // Ensure the spawn position is inside the collider, not too close to the player or walls
            do
            {
                spawnPosition = GetRandomPointInBounds();
            }
            while (Vector2.Distance(spawnPosition, playerPosition) < minSpawnDistance || IsTooCloseToWall(spawnPosition)); // Recalculate if too close to the player or walls

            // Randomly choose an enemy prefab from the list
            int randomIndex = Random.Range(0, enemyPrefabs.Length);
            GameObject enemyToSpawn = enemyPrefabs[randomIndex];

            // Instantiate the enemy at the random position and add it to the list of spawned enemies
            GameObject spawnedEnemy = Instantiate(enemyToSpawn, spawnPosition, Quaternion.identity);

            // Make the spawned enemy a child of the spawnAnchor
            if (spawnAnchor != null)
            {
                spawnedEnemy.transform.SetParent(spawnAnchor);
            }

            spawnedEnemies.Add(spawnedEnemy); // Add enemy to the list
        }
    }

    // Get a random point within the collider's bounds
    Vector2 GetRandomPointInBounds()
    {
        Bounds bounds = spawnAreaCollider.bounds; // Get the bounds of the collider
        float x = Random.Range(bounds.min.x, bounds.max.x); // Get random x within bounds
        float y = Random.Range(bounds.min.y, bounds.max.y); // Return random y within the bounds
        return new Vector2(x, y); // Return random point inside the bounds
    }

    // Method to check if the spawn position is too close to a wall
    bool IsTooCloseToWall(Vector2 spawnPosition)
    {
        // Check if there's any wall object within the minWallDistance radius around the spawn position
        Collider2D wallCheck = Physics2D.OverlapCircle(spawnPosition, minWallDistance, wallLayer);

        // Return true if there's a wall nearby, false otherwise
        return wallCheck != null;
    }

    // Method to check if all enemies have been destroyed
    public bool AreAllEnemiesDestroyed()
    {
        // Remove any destroyed enemies from the list
        spawnedEnemies.RemoveAll(enemy => enemy == null);

        // Return true if all enemies have been destroyed
        return spawnedEnemies.Count == 0;
    }
}
