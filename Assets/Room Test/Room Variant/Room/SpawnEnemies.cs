using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnEnemies : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    public GameObject[] enemyPrefabs; // Array to assign multiple enemy prefabs

    [Header("Spawn Settings")]
    public int minEnemies = 4; // Minimum number of enemies to spawn
    public int maxEnemies = 6; // Maximum number of enemies to spawn
    public float minSpawnDistance = 2f; // Minimum distance from the player
    public float minWallDistance = 1.5f; // Minimum distance from any walls
    
    // ========== TRAP SETTINGS ========== //
    [Header("Trap Prefabs")]
    public GameObject[] trapPrefabs;
    
    [Header("Trap Spawn Settings")] 
    public int minTraps = 0;
    public int maxTraps = 2;
    public float trapMinDistanceFromPlayer = 1.5f;
    public float trapMinDistanceFromWalls = 1f;

    // ========== CONSUMABLE SETTINGS ========== //
    [Header("Consumable Prefabs")] 
    public GameObject[] consumablePrefabs;
    
    [Header("Consumable Spawn Settings")]
    public int minConsumables = 0;
    public int maxConsumables = 2;
    public float consumableMinDistanceFromPlayer = 1f; 
    public float consumableMinDistanceFromWalls = 0.5f;
    
    [Header("Door Settings")]
    public List<Transform> doorLocations = new List<Transform>();  // List of door positions
    public List<GameObject> doorPrefabs = new List<GameObject>();

    public LayerMask wallLayer; // Layer for walls
    public Transform spawnAnchor; // The parent object under which enemies will be spawned

    private Collider2D spawnAreaCollider; // Collider that defines the spawn area
    private bool hasSpawned = false; // To ensure enemies only spawn once
    public List<GameObject> spawnedEnemies = new List<GameObject>(); // List of spawned enemies
    private List<GameObject> spawnedDoors = new List<GameObject>(); // List to track all spawned doors
    private List<GameObject> spawnedTraps = new List<GameObject>();
    private List<GameObject> spawnedConsumables = new List<GameObject>();   

    private MinimapController minimapController; // Reference to MinimapController
    private bool minimapRegistered = false; // Ensure room is registered only once

    [Header("Spawn Optimization")]
    public bool preCalculateSpawns = true; // Toggle pre-calculation
    private List<Vector2> lockedSpawnPositions = new List<Vector2>(); // Stores validated positions

    void Start()
    {
        // Get the collider attached to this object
        spawnAreaCollider = GetComponent<Collider2D>();
        minimapController = FindObjectOfType<MinimapController>(); // Find minimap controller
    }

    // Trigger when the player collides with the object
    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the colliding object is tagged as "Player"
        if (other.CompareTag("Player") && !hasSpawned)
        {
            // Spawn all the doors when the player enters
            SpawnDoors();

            // Spawn enemies
            SpawnRandomEnemies(other.transform.position); // Pass the player position
            hasSpawned = true; // Prevent spawning multiple times
        }

        // Disable this object if it collides with an object tagged "Marker"
        if (other.CompareTag("Marker"))
        {
            StartCoroutine(HandleMarkerCollision());
        }

        // Register the room on the minimap if not already registered
        if (!minimapRegistered && minimapController != null)
        {
            Vector2 roomPosition = new Vector2(Mathf.Round(transform.position.x), Mathf.Round(transform.position.y));
            minimapController.RegisterRoom(roomPosition, minimapController.roomIconPrefab);
            minimapRegistered = true;
        }

        // Enable the minimap icon for this room when the player enters
        if (other.CompareTag("Player") && minimapController != null)
        {
            Vector2 roomPosition = new Vector2(Mathf.Round(transform.position.x), Mathf.Round(transform.position.y));
            minimapController.EnableRoomIcon(roomPosition); // Enable the minimap icon for this room
        }
    }

    // Trigger when the marker is spawned on top of this object
    IEnumerator HandleMarkerCollision()
    {
        // Spawn doors immediately when marker collides
        SpawnDoors();

        // Wait for 1 second before clearing doors
        yield return new WaitForSeconds(1f);

        // Destroy valid doors only, but keep the room active
        foreach (GameObject door in spawnedDoors)
        {
            DoorController doorController = door.GetComponentInChildren<DoorController>();

            if (doorController != null && !doorController.isInvalid)
            {
                Destroy(door); // Only remove valid doors
            }
        }
        spawnedDoors.Clear(); // Clear door list

        // Wait for another second before ending the process
        yield return new WaitForSeconds(1f);

        hasSpawned = true; // Marks this room as processed (so enemies won't spawn again)
    }


    // Method to disable this game object entirely
    private void DisableObject()
    {
        gameObject.SetActive(false);
    }

   
    // Method to spawn the doors at the assigned spawn points with specific Z rotation
   void SpawnDoors()
{
    // Ensure we have matching lists
    if (doorLocations.Count != doorPrefabs.Count)
    {
        Debug.LogError("Door locations and prefabs lists don't match in length!");
        return;
    }

    for (int i = 0; i < doorLocations.Count; i++)
    {
        if (doorLocations[i] != null && i < doorPrefabs.Count && doorPrefabs[i] != null)
        {
            GameObject spawnedDoor = Instantiate(
                doorPrefabs[i],
                doorLocations[i].position,
                Quaternion.identity,
                doorLocations[i]  // Parent to spawn point
            );
            
            spawnedDoors.Add(spawnedDoor);
        }
        else
        {
            Debug.LogWarning($"Missing door location or prefab at index {i}");
        }
    }
}

    // Method to spawn enemies, ensuring they are inside the collider area, not close to the player or walls
    void SpawnRandomEnemies(Vector2 playerPosition)
    {
        // ===== 1. ENEMY SPAWNING ===== //
        int enemyCount = Random.Range(minEnemies, maxEnemies + 1);        
        if (preCalculateSpawns)
        {
            lockedSpawnPositions = GetValidSpawnPositions(
                playerPosition, 
                enemyCount, 
                minWallDistance, 
                minSpawnDistance
            );
            
            for (int i = 0; i < lockedSpawnPositions.Count; i++)
            {
                SpawnEnemyAtPosition(lockedSpawnPositions[i]);
            }
        }
        else
        {
            for (int i = 0; i < enemyCount; i++)
            {
                TrySpawnEnemy(playerPosition);
            }
        }

        // ===== 2. TRAP SPAWNING ===== //
        // First DECLARE the variable
        int trapCount = Random.Range(minTraps, maxTraps + 1);
        // Then USE it
        for (int i = 0; i < trapCount; i++)
        {
            TrySpawnTrap(playerPosition);
        }

        // ===== 3. CONSUMABLE SPAWNING ===== //
        // First DECLARE the variable
        int consumableCount = Random.Range(minConsumables, maxConsumables + 1);
        // Then USE it
        for (int i = 0; i < consumableCount; i++)
        {
            TrySpawnConsumable(playerPosition);
        }

        StartCoroutine(CheckEnemiesDefeated());
    }
    
    List<Vector2> GetValidSpawnPositions(Vector2 playerPosition, int count, float wallDistance, float playerDistance)
    {
        List<Vector2> validPositions = new List<Vector2>();
        int attempts = 0;
        int maxAttempts = 1000; // A realistic cap to avoid infinite loop

        // Try to find as many unique valid positions as possible
        while (validPositions.Count < count && attempts < maxAttempts)
        {
            Vector2 candidatePos = GetRandomPointInBounds();
            if (!IsTooCloseToWall(candidatePos, wallDistance) && 
                Vector2.Distance(candidatePos, playerPosition) >= playerDistance)
            {
                validPositions.Add(candidatePos);
            }
            attempts++;
        }

        // If not enough valid positions, reuse ones we've already found
        if (validPositions.Count < count && validPositions.Count > 0)
        {
            int toAdd = count - validPositions.Count;
            for (int i = 0; i < toAdd; i++)
            {
                // Copy existing positions (could use modulus to loop through)
                validPositions.Add(validPositions[i % validPositions.Count]);
            }
            Debug.LogWarning("Not enough unique spawn positions found—some enemy positions are reused.");
        }
        else if (validPositions.Count == 0)
        {
            Debug.LogError("No valid positions found at all! No enemies can spawn.");
        }

        return validPositions;
    }

    void SpawnEnemyAtPosition(Vector2 position)
    {
        int randomIndex = Random.Range(0, enemyPrefabs.Length);
        GameObject enemy = Instantiate(enemyPrefabs[randomIndex], position, Quaternion.identity);
        
        if (spawnAnchor != null)
            enemy.transform.SetParent(spawnAnchor);
        
        spawnedEnemies.Add(enemy);
    }

    void TrySpawnTrap(Vector2 playerPosition)
    {
        int attempts = 0;
        while (attempts < 20)
        {
            Vector2 spawnPos = GetRandomPointInBounds();
            if (!IsTooCloseToWall(spawnPos, trapMinDistanceFromWalls) && 
                Vector2.Distance(spawnPos, playerPosition) >= trapMinDistanceFromPlayer)
            {
                SpawnTrapAtPosition(spawnPos);
                return;
            }
            attempts++;
        }
    }

    void SpawnTrapAtPosition(Vector2 position)
    {
        if (trapPrefabs.Length == 0) return;
        
        int randomIndex = Random.Range(0, trapPrefabs.Length);
        GameObject trap = Instantiate(trapPrefabs[randomIndex], position, Quaternion.identity);
        
        if (spawnAnchor != null)
            trap.transform.SetParent(spawnAnchor);
        
        spawnedTraps.Add(trap);
    }

// ===== CONSUMABLE SPAWNING METHODS ===== //
    void TrySpawnConsumable(Vector2 playerPosition)
    {
        int attempts = 0;
        while (attempts < 20)
        {
            Vector2 spawnPos = GetRandomPointInBounds();
            if (!IsTooCloseToWall(spawnPos, consumableMinDistanceFromWalls) && 
                Vector2.Distance(spawnPos, playerPosition) >= consumableMinDistanceFromPlayer)
            {
                SpawnConsumableAtPosition(spawnPos);
                return;
            }
            attempts++;
        }
    }

   // Add this to your SpawnEnemies class
public ConsumableManager consumableManager;

void SpawnConsumableAtPosition(Vector2 position)
{
    if (consumablePrefabs == null || consumablePrefabs.Length == 0) return;
    
    // Create a list to store valid prefab indices
    List<int> validIndices = new List<int>();
    
    // Check each prefab
    for (int i = 0; i < consumablePrefabs.Length; i++)
    {
        bool canSpawn = true;
        
        // If consumableManager exists, check if this prefab is active
        if (consumableManager != null)
        {
            foreach (ConsumableItem item in consumableManager.consumables)
            {
                // Compare by name instead of reference
                if (item.prefab != null && consumablePrefabs[i] != null && 
                    item.prefab.name == consumablePrefabs[i].name && item.isActive)
                {
                    canSpawn = false;
                    break;
                }
            }
        }
        
        // If we can spawn this prefab, add its index to valid indices
        if (canSpawn)
        {
            validIndices.Add(i);
        }
    }
    
    // If no valid prefabs to spawn, just return
    if (validIndices.Count == 0) return;
    
    // Choose a random prefab from valid ones
    int randomIndex = validIndices[Random.Range(0, validIndices.Count)];
    GameObject consumable = Instantiate(consumablePrefabs[randomIndex], position, Quaternion.identity);
    
    if (spawnAnchor != null)
        consumable.transform.SetParent(spawnAnchor);
    
    spawnedConsumables.Add(consumable);
}
        // Original random-spawn fallback
    void TrySpawnEnemy(Vector2 playerPosition)
    {
        int attempts = 0;
        while (attempts < 20) // Keep original maxAttempts
        {
            Vector2 spawnPos = GetRandomPointInBounds();
        if (!IsTooCloseToWall(spawnPos, minWallDistance) && 
    Vector2.Distance(spawnPos, playerPosition) >= minSpawnDistance)
            {
                SpawnEnemyAtPosition(spawnPos);
                return;
            }
            attempts++;
        }
    }
        // Get a random point within the collider's bounds
    Vector2 GetRandomPointInBounds()
    {
        Bounds bounds = spawnAreaCollider.bounds; // Get the bounds of the collider
        float x = Random.Range(bounds.min.x, bounds.max.x); // Get random x within bounds
        float y = Random.Range(bounds.min.y, bounds.max.y); // Get random y within the bounds
        return new Vector2(x, y); // Return random point inside the bounds
    }

    bool IsTooCloseToWall(Vector2 spawnPosition, float distance)
    {
        if (wallLayer.value == 0)
        {
            Debug.LogWarning("Wall Layer not set in SpawnEnemies script!");
            return false;
        }

        Collider2D[] nearbyWalls = Physics2D.OverlapCircleAll(spawnPosition, distance, wallLayer);
        return nearbyWalls.Length > 0;
    }
    
    IEnumerator CheckEnemiesDefeated()
    {
        // Loop until all enemies are destroyed
        while (spawnedEnemies.Count > 0)
        {
            // Remove any enemies that have been destroyed from the list
            spawnedEnemies.RemoveAll(enemy => enemy == null);

            // Wait for the next check (every 0.5 seconds)
            yield return new WaitForSeconds(0.5f);
        }

        // Find the player to give gold
        PlayerMovement player = FindObjectOfType<PlayerMovement>();
        if (player != null)
        {
            // Award 10 gold to the player
            player.AddGold(3);
            Debug.Log("All enemies defeated! Adding 3 gold to player.");
        }

        // Once all enemies are defeated, remove or deactivate all doors
        foreach (GameObject door in spawnedDoors)
        {
            DoorController doorController = door.GetComponentInChildren<DoorController>(); // Get the DoorController script from the child collider

            if (doorController != null && !doorController.isInvalid)  // Check if the door is valid
            {
                Destroy(door); // Remove the door if it's not invalid
            }
        }

        // Clear the list of spawned doors
        spawnedDoors.Clear();
    }

    public void RegisterNewEnemy(GameObject newEnemy)
    {
        if (spawnAnchor != null)
        {
            newEnemy.transform.SetParent(spawnAnchor); // Set the new enemy as a child of the spawnAnchor
        }

        spawnedEnemies.Add(newEnemy); // Add the new enemy to the list
    }

        // Optionally visualize the spawn area and wall detection in the editor
    private void OnDrawGizmosSelected()
    {
        if (spawnAreaCollider == null) return;

        // Draw spawn area
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(spawnAreaCollider.bounds.center, spawnAreaCollider.bounds.size);

        // Draw locked positions (cyan) if in pre-calculate mode
        if (preCalculateSpawns && lockedSpawnPositions != null)
        {
            Gizmos.color = Color.cyan;
            foreach (Vector2 pos in lockedSpawnPositions)
            {
                Gizmos.DrawWireSphere(pos, 0.3f);
            }
        }
    }
}