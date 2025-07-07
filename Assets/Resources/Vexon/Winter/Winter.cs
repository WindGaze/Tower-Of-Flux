using UnityEngine;

public class Winter : MonoBehaviour
{
    [Header("GameManager Sync")]
    public int level = 1; // Default, will override with GameManager's value
    
    [Header("Shooting Parameters")]
    public GameObject projectilePrefab;  
    public GameObject projectilePrefab2;  
    [Header("Animation Event SFX")]
    public AudioClip animEventClip;
    public AudioClip spawnSFX;
    private AudioSource audioSource;
    [Header("Shield Parameters")] // Changed from "Level 2 Parameters"
    public GameObject shieldPrefab; // Assign shield prefab in inspector
    public Transform playerTransform;
    private float shieldDuration = 10f; // Renamed from auraDuration
    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogWarning("No GameObject with 'Player' tag found");
        }
        
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        if (spawnSFX != null && audioSource != null)
            audioSource.PlayOneShot(spawnSFX);

        if (GameManager.Instance != null)
        {
            level = GameManager.Instance.GetVexonLevel("Winter");
        }

        ExecuteLevelBehavior();
    }
    public void PlayAnimEventSFX()
    {
        if (animEventClip != null && audioSource != null)
            audioSource.PlayOneShot(animEventClip);
    }
    private void ExecuteLevelBehavior()
    {
        if (level == 2)
        {
            animator.SetInteger("AnimState", 2); 
        }
        else if (level == 1)
        {
            animator.SetInteger("AnimState", 1); 
        }
        else if (level == 3)
        {
            animator.SetInteger("AnimState", 3); 
        }
    }

    private void Level1Behavior() 
    {
        SnowSlash(projectilePrefab);
    }
    
    private void Level2Behavior() 
    {
        SnowSlash(projectilePrefab);
        SpawnShield();
    }

    private void Level3Behavior() 
    {
        TrueSnowSlash(projectilePrefab2);
        SpawnShield();
    }

    private void SpawnShield()
    {
        if (shieldPrefab != null && playerTransform != null)
        {
            // Check for existing shield first
            ShieldPlayer existingShield = playerTransform.GetComponentInChildren<ShieldPlayer>();
            
            if (existingShield != null)
            {
                // Option 1: Refresh duration on existing shield
                Destroy(existingShield.gameObject); // Remove old shield first
            }
            
            // Create new shield
            GameObject newShield = Instantiate(shieldPrefab, playerTransform.position, 
                                            Quaternion.identity, playerTransform);
            Destroy(newShield, shieldDuration);
            Debug.Log("Shield spawned on player");
        }
    }
    private GameObject FindClosestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject closest = null;
        float minDistance = Mathf.Infinity;
        Vector3 position = transform.position;

        foreach (GameObject enemy in enemies)
        {
            float distance = Vector3.Distance(enemy.transform.position, position);
            if (distance < minDistance)
            {
                closest = enemy;
                minDistance = distance;
            }
        }
        return closest;
    }

    private void SnowSlash(GameObject projectile)
    {
        GameObject closestEnemy = FindClosestEnemy();
        if (closestEnemy != null && projectile != null)
        {
            Vector3 direction = (closestEnemy.transform.position - transform.position).normalized;
            // Calculate only Z rotation
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.Euler(0, 0, angle);
            
            GameObject spawnedProjectile = Instantiate(projectile, transform.position, rotation);

            WinterBullet bullet = spawnedProjectile.GetComponent<WinterBullet>();
            if (bullet != null)
            {
                bullet.Initialize(direction, 10f, level);
            }
        }
    }

    private void TrueSnowSlash(GameObject projectile)
    {
        GameObject closestEnemy = FindClosestEnemy();
        if (closestEnemy != null && projectile != null)
        {
            Vector3 direction = (closestEnemy.transform.position - transform.position).normalized;
            // Calculate only Z rotation
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.Euler(0, 0, angle);
            
            GameObject spawnedProjectile = Instantiate(projectile, transform.position, rotation);

            WinterSlow bullet = spawnedProjectile.GetComponent<WinterSlow>();
            if (bullet != null)
            {
                bullet.Initialize(direction, 10f, level);
            }
        }
    }

    public void Disappear()
    {
        Destroy(gameObject);
    }
}