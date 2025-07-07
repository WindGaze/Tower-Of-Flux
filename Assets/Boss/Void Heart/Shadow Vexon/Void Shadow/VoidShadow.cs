using UnityEngine;

public class VoidShadow : MonoBehaviour
{
    public GameObject shadowEight; // The bullet prefab for 8-directional shots
    public float eightDirBulletSpeed = 5f;
    [Header("Animation Event SFX")]
    public AudioClip animEventClip;
    private AudioSource audioSource;
    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetInteger("AnimState", 1);
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
    // Call this function from animation event to spawn 8-directional bullets
    public void SpawnEightDirectionalBullets()
    {
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
                    straightBullet.Initialize(direction, eightDirBulletSpeed, 1); // Always level 1
                }
                else
                {
                    Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
                    if (rb != null)
                    {
                        rb.velocity = direction * eightDirBulletSpeed;
                    }
                }
            }
        }
        else
        {
            Debug.LogWarning("No shadowEight prefab assigned!");
        }
    }

    // Call this via animation event to destroy the object
    public void Disappear()
    {
        Destroy(gameObject);
    }
}