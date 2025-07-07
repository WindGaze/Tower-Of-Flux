using System.Collections;
using UnityEngine;

public class Shadow : MonoBehaviour
{
    public int level = 1;
    public float eightDirBulletSpeed = 5f;
    private Animator animator;

    [Header("Level 1 Settings")]
    public GameObject bulletPrefab;
    public Transform[] bulletSpawnPoints;

    [Header("Level 2 Settings")]
    public GameObject shadowEight;

    [Header("Level 3 Settings")]
    public Transform[] l3SpawnPoints;
    [Header("Animation Event SFX")]
    public AudioClip animEventClip;
    public AudioClip spawnSFX;
    private AudioSource audioSource;
    private void Start()
    {
        if (GameManager.Instance != null)
        {
            level = GameManager.Instance.GetVexonLevel("Shadow");
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
        if (level == 2)
        {
            animator.SetInteger("AnimState", 2); 
        }

        // Fire correct projectile
        if (level == 1)
        {
            animator.SetInteger("AnimState", 1); 
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
    private void Level1Behavior()
    {
        if (bulletPrefab != null && bulletSpawnPoints.Length > 0)
        {
            foreach (Transform spawnPoint in bulletSpawnPoints)
            {
                Instantiate(bulletPrefab, spawnPoint.position, spawnPoint.rotation);
            }
            Debug.Log("Shadow spawned bullets at Level 1!");
        }
        else
        {
            Debug.LogWarning("Bullet prefab or spawn points are missing!");
        }    
    }

    private void Level2Behavior()
{   
    // First part - original bullets from spawn points
    if (bulletPrefab != null && bulletSpawnPoints.Length > 0)
    {
        foreach (Transform spawnPoint in bulletSpawnPoints)
        {
            Instantiate(bulletPrefab, spawnPoint.position, spawnPoint.rotation);
        }
        Debug.Log("Shadow spawned bullets at Level 2!"); // Fixed the log message
    }
    else
    {
        Debug.LogWarning("Bullet prefab or spawn points are missing!");
    }  

    // Second part - 8 directional bullets
    if (shadowEight != null)
    {
        float angleStep = 360f / 8;
        for (int i = 0; i < 8; i++)
        {
            float angle = i * angleStep;
            Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));

            GameObject bullet = Instantiate(shadowEight, transform.position, Quaternion.identity);
            StraightBullet straightBullet = bullet.GetComponent<StraightBullet>();
            
            if (straightBullet != null)
            {
                // Initialize with direction, speed, and level
                straightBullet.Initialize(direction, eightDirBulletSpeed, level);
            }
            else
            {
                // Fallback to using Rigidbody2D if StraightBullet component isn't found
                Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.velocity = direction * eightDirBulletSpeed;
                }
            }
        }
        Debug.Log("Shadow spawned 8-directional bullets!");
    }
}

    private void Level3Behavior()
    {
        if (bulletPrefab != null && l3SpawnPoints.Length > 0)
        {
            foreach (Transform spawnPoint in l3SpawnPoints)
            {
                Instantiate(bulletPrefab, spawnPoint.position, spawnPoint.rotation);
            }
            Debug.Log("Shadow spawned bullets at Level 3!");
        }
        else
        {
            Debug.LogWarning("Bullet prefab or Level 3 spawn points are missing!");
        } 

        if (shadowEight != null)
        {
            float angleStep = 360f / 8;
            for (int i = 0; i < 8; i++)
            {
                float angle = i * angleStep;
                Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));

                GameObject bullet = Instantiate(shadowEight, transform.position, Quaternion.identity);
                Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.velocity = direction * eightDirBulletSpeed;
                }
            }
        }
    }

    private void Disappear()
    {
        Destroy(gameObject);
    }

}
