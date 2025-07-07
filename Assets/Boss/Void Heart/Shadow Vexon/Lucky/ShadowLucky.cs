using UnityEngine;

public class ShadowLucky : MonoBehaviour
{
    [Header("Bullet Settings")]
    public GameObject bulletPrefab;
    public Transform[] bulletSpawnPoints; // Assign spawn points in Inspector
    [Header("Animation Event SFX")]
    public AudioClip animEventClip;
    private AudioSource audioSource;
    
    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetInteger("AnimState", 1); // Set anim state to 1 on spawn
        }
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void PlayAnimEventSFX()
    {
        if (animEventClip != null && audioSource != null)
            audioSource.PlayOneShot(animEventClip);
    }
    // Call this method (e.g. via Animation Event) to spawn bullets
    public void SpawnBullets()
    {
        if (bulletPrefab != null && bulletSpawnPoints != null && bulletSpawnPoints.Length > 0)
        {
            foreach (Transform spawnPoint in bulletSpawnPoints)
            {
                Instantiate(bulletPrefab, spawnPoint.position, spawnPoint.rotation);
            }
            Debug.Log("Spawned bullets!");
        }
        else
        {
            Debug.LogWarning("Bullet prefab or spawn points are missing!");
        }
    }

    private void Disappear()
    {
        Destroy(gameObject);
    }
}