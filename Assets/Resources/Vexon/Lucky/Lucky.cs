using UnityEngine;
using System.Collections;

public class Lucky : MonoBehaviour
{
    public int level = 1;
    private PlayerMovement player;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    [Header("Animation Event SFX")]
    public AudioClip animEventClip;
    public AudioClip spawnSFX;
    private AudioSource audioSource;
    [Header("Level 2 Settings")]
    public GameObject bulletPrefab;
    public Transform[] bulletSpawnPoints; // Assign spawn points in Inspector

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            level = GameManager.Instance.GetVexonLevel("Lucky");
        }
        else
        {
            Debug.LogError("GameManager instance not found!");
        }
        // Get component references
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>(); // Initialize the Animator
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        if (spawnSFX != null && audioSource != null)
            audioSource.PlayOneShot(spawnSFX);
        // Find the player object and get the PlayerMovement component
        player = FindObjectOfType<PlayerMovement>();

        if (player == null)
        {
            Debug.LogError("PlayerMovement not found in the scene!");
            return;
        }

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
        // Activate the luck buff
        if (player != null)
        {
            player.luckBuff = true;
            Debug.Log("Lucky buff activated for 10 seconds!");
        }
    }

    private void Level2Behavior()
    {
        // Activate Level 1 behavior
        if (player != null)
        {
            player.luckBuff = true;
            Debug.Log("Lucky buff activated for 10 seconds!");
        }

        // Spawn 4 bullets at predefined spawn points
        if (bulletPrefab != null && bulletSpawnPoints.Length > 0)
        {
            foreach (Transform spawnPoint in bulletSpawnPoints)
            {
                Instantiate(bulletPrefab, spawnPoint.position, spawnPoint.rotation);
            }
            Debug.Log("Spawned 4 bullets at Level 2!");
        }
        else
        {
            Debug.LogWarning("Bullet prefab or spawn points are missing!");
        }
    }

     private void Level3Behavior()
    {
        // Activate the luck buff
        if (player != null)
        {
            player.luckBuff2 = true;
            Debug.Log("Lucky buff 2 activated for 10 seconds!");
        }

        if (bulletPrefab != null && bulletSpawnPoints.Length > 0)
        {
            foreach (Transform spawnPoint in bulletSpawnPoints)
            {
                Instantiate(bulletPrefab, spawnPoint.position, spawnPoint.rotation);
            }
            Debug.Log("Spawned 4 bullets at Level 2!");
        }
        else
        {
            Debug.LogWarning("Bullet prefab or spawn points are missing!");
        }
    }

    private void MakeInvisible()
    {
        // Make the object invisible by disabling the sprite renderer
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }

        // Also disable any particle systems or other visual components
        ParticleSystem[] particles = GetComponentsInChildren<ParticleSystem>();
        foreach (ParticleSystem ps in particles)
        {
            ps.Stop();
        }
        Invoke(nameof(Disappear), 10f);
    }

    private void Disappear()
    {
        // Deactivate the luck buff
        if (player != null)
        {
            player.luckBuff = false;
            Debug.Log("Lucky buff deactivated!");
        }

        Destroy(gameObject);
    }
}
