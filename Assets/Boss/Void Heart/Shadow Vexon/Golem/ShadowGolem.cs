using UnityEngine;

public class ShadowGolem : MonoBehaviour
{
    public GameObject projectilePrefab;
    private Animator animator;
    [Header("Animation Event SFX")]
    public AudioClip animEventClip;
    private AudioSource audioSource;
    private void Start()
    {
        animator = GetComponent<Animator>();
        animator.SetInteger("AnimState", 1); // Always Level 1 animation for ShadowGolem
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }
    public void PlayAnimEventSFX()
    {
        if (animEventClip != null && audioSource != null)
            audioSource.PlayOneShot(animEventClip);
    }
    // This should be called by your animation event
    public void ThrowAtPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Vector3 direction = (player.transform.position - transform.position).normalized;
            GameObject spawnedProjectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);

            ShadowGolemProjectile sgp = spawnedProjectile.GetComponent<ShadowGolemProjectile>();
            if (sgp != null)
            {
                sgp.Initialize(direction, 15f); // Set speed as needed
            }
        }
        else
        {
            Debug.LogWarning("Player not found to target!");
        }
    }

    private void Disappear()
    {
        Destroy(gameObject);
    }
}