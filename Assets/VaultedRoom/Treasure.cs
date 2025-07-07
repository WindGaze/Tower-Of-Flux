using UnityEngine;
using System.Collections.Generic;

public class Treasure : MonoBehaviour
{
    [System.Serializable]
    public class SpawnableObject
    {
        public GameObject prefab;
        [Range(0f, 1f)] public float spawnChance;
    }

    [Header("Spawning Settings")]
    public List<SpawnableObject> spawnableObjects;
    public float pushDistance = 1f;
    public float pushSpeed = 3f;
    public float decelerationRate = 0.9f;
    [Tooltip("Time in seconds before spawned object's collider activates")]
    public float colliderDelay = 0.5f;

    public GameObject objectToDestroy;

    private Animator animator;
    private bool playerInRange = false;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            animator.SetInteger("AnimState", 1);
            if (objectToDestroy != null)
            {
                objectToDestroy.SetActive(false);
            }
        }
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerInRange = false;
    }

    private void TrySpawnItem()
    {
        if (spawnableObjects == null || spawnableObjects.Count == 0)
        {
            Destroy(gameObject);
            return;
        }

        GameObject selectedPrefab = SelectRandomPrefab();
        if (selectedPrefab != null)
        {
            SpawnWithPushEffect(selectedPrefab);
        }

        GetComponent<Collider2D>().enabled = false;
    }

    private GameObject SelectRandomPrefab()
    {
        float totalChance = 0f;
        foreach (var item in spawnableObjects)
        {
            if (item.prefab != null) totalChance += item.spawnChance;
        }

        float randomPoint = Random.Range(0f, totalChance);
        float runningTotal = 0f;

        foreach (var item in spawnableObjects)
        {
            if (item.prefab == null) continue;

            runningTotal += item.spawnChance;
            if (randomPoint <= runningTotal) return item.prefab;
        }

        return spawnableObjects[0].prefab; // Fallback
    }

    private void SpawnWithPushEffect(GameObject prefab)
    {
        GameObject spawnedObject = Instantiate(prefab, transform.position, Quaternion.identity);
        
        // Disable all colliders initially
        Collider2D[] colliders = spawnedObject.GetComponentsInChildren<Collider2D>();
        foreach (Collider2D col in colliders)
        {
            col.enabled = false;
        }

        // Add and configure the mover component
        KinematicMover2D mover = spawnedObject.GetComponent<KinematicMover2D>() ?? 
                               spawnedObject.AddComponent<KinematicMover2D>();
        mover.SetupPush(
            Random.insideUnitCircle.normalized, 
            pushSpeed, 
            decelerationRate,
            colliders,
            colliderDelay
        );
    }
}

public class KinematicMover2D : MonoBehaviour
{
    private Vector2 movementDirection;
    private float currentSpeed;
    private float deceleration;
    private bool isMoving = false;
    private Collider2D[] objectColliders;
    private float colliderDelay;
    private float delayTimer;

    public void SetupPush(Vector2 direction, float speed, float decelerationRate, Collider2D[] colliders, float delay)
    {
        movementDirection = direction.normalized;
        currentSpeed = speed;
        deceleration = decelerationRate;
        objectColliders = colliders;
        colliderDelay = delay;
        delayTimer = 0f;
        isMoving = true;
    }

    private void Update()
    {
        if (!isMoving) return;

        // Handle collider delay
        delayTimer += Time.deltaTime;
        if (delayTimer >= colliderDelay && !objectColliders[0].enabled)
        {
            foreach (Collider2D col in objectColliders)
            {
                col.enabled = true;
            }
        }

        // Handle movement
        transform.position += (Vector3)(movementDirection * currentSpeed * Time.deltaTime);
        currentSpeed = Mathf.Lerp(currentSpeed, 0f, deceleration * Time.deltaTime);

        if (currentSpeed < 0.01f)
        {
            currentSpeed = 0f;
            isMoving = false;
        }
    }
}