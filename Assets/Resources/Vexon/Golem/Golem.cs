using UnityEngine;

public class Golem : MonoBehaviour
{
    public GameObject projectilePrefab;  // For level 1 & 2
    public GameObject projectilePrefab2; // For level 3
    public GameObject shieldPrefab; // Shield prefab to spawn
    public int level = 1; // Default, will sync with GameManager
    private Animator animator;
    [Header("Animation Event SFX")]
    public AudioClip animEventClip;
    public AudioClip spawnSFX;
    private AudioSource audioSource;

    private void Start()
    {
        // Sync level with GameManager's vexon level
        if (GameManager.Instance != null)
        {
            level = GameManager.Instance.GetVexonLevel("Golem");
        }
        else
        {
            Debug.LogError("GameManager instance not found!");
        }
        animator = GetComponent<Animator>(); // Initialize the Animator
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        if (spawnSFX != null && audioSource != null)
            audioSource.PlayOneShot(spawnSFX);
        // Set animator state based on level
        if (level == 1)
        {
            animator.SetInteger("AnimState", 1); 
        }
        else if (level == 2)
        {
            animator.SetInteger("AnimState", 2); 
        }
        else if (level == 3)
        {
            animator.SetInteger("AnimState", 3); 
        }
    }
    public void PlayAnimEventSFX()
    {
        if (animEventClip != null && audioSource != null)
            audioSource.PlayOneShot(animEventClip);
    }
        // Level 1 Behavior - Throw GolemBullet at closest target
    private void Level1Behavior()
    {
        ThrowAtClosestTarget(projectilePrefab);
    }

    // Level 2 Behavior - Spawn an object on the player and shield
    private void Level2Behavior()
    {
        ThrowAtClosestTarget(projectilePrefab);
        SpawnShieldOnPlayer(); // Added shield spawn here
    }

    // Level 3 Behavior - Throw GolemBullet2 at closest target and spawn shield
    private void Level3Behavior()
    {
        ThrowAtClosestTarget(projectilePrefab2);
        SpawnShieldOnPlayer(); // Added shield spawn here
    }

    private void ThrowAtClosestTarget(GameObject projectile)
    {
        GameObject closestTarget = FindClosestTarget(); // targets enemy or boss
        if (closestTarget != null)
        {
            Vector3 direction = (closestTarget.transform.position - transform.position).normalized;
            GameObject spawnedProjectile = Instantiate(projectile, transform.position, Quaternion.identity);

            if (projectile == projectilePrefab2)
            {
                GolemBullet2 bullet = spawnedProjectile.GetComponent<GolemBullet2>();
                if (bullet != null)
                {
                    bullet.Initialize(direction, 10f, level);
                }
            }
            else
            {
                GolemBullet bullet = spawnedProjectile.GetComponent<GolemBullet>();
                if (bullet != null)
                {
                    bullet.Initialize(direction, 10f, level);
                }
            }
        }
        else
        {
            Debug.LogWarning("No enemies or bosses found to target!");
        }
    }

    private GameObject FindClosestTarget()
    {
        // Combine enemies and bosses into one array
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject[] bosses = GameObject.FindGameObjectsWithTag("Boss");

        float minDistance = Mathf.Infinity;
        GameObject closest = null;
        Vector3 currentPos = transform.position;

        foreach (GameObject target in enemies)
        {
            float distance = Vector3.Distance(target.transform.position, currentPos);
            if (distance < minDistance)
            {
                minDistance = distance;
                closest = target;
            }
        }
        foreach (GameObject target in bosses)
        {
            float distance = Vector3.Distance(target.transform.position, currentPos);
            if (distance < minDistance)
            {
                minDistance = distance;
                closest = target;
            }
        }
        return closest;
    }

    // Spawn shield on player
    private void SpawnShieldOnPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && shieldPrefab != null)
        {
            Instantiate(shieldPrefab, player.transform.position, Quaternion.identity, player.transform);
            Debug.Log("Shield spawned on player");
        }
        else
        {
            Debug.LogWarning("Player not found or shieldPrefab is not assigned!");
        }
    }

    private void Disappear()
    {
        Destroy(gameObject);
    }
}   