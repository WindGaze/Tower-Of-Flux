using UnityEngine;

public class ShadowWinter : MonoBehaviour
{
    [Header("Projectile")]
    public GameObject projectilePrefab2;  

    private Transform playerTransform;
    private Animator animator;

    [Header("Animation Event SFX")]
    public AudioClip animEventClip;
    private AudioSource audioSource;

    private void Start()
    {
        animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetInteger("AnimState", 1); // Always use Level 1 animation
        }

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
    }
    public void PlayAnimEventSFX()
    {
        if (animEventClip != null && audioSource != null)
            audioSource.PlayOneShot(animEventClip);
    }
    // Call this (eg, animation event) to shoot at the player
    public void TrueSnowSlash()
    {
        if (playerTransform != null && projectilePrefab2 != null)
        {
            Vector3 direction = (playerTransform.position - transform.position).normalized;
            
            // Calculate rotation to face player
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            
            GameObject spawnedProjectile = Instantiate(projectilePrefab2, transform.position, rotation);

            ShadowWinterSlash bullet = spawnedProjectile.GetComponent<ShadowWinterSlash>();
            if (bullet != null)
            {
                bullet.Initialize(direction, 10f, 1); // Level always 1 for Shadow variant
            }
        }
    }

    // Call this to destroy the object
    public void Disappear()
    {
        Destroy(gameObject);
    }
}