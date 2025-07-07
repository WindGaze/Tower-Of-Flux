using System.Collections;
using UnityEngine;

public class Wisp : MonoBehaviour
{
    public int damage = 10;
    public float speed = 2f;
    public float idleTime = 2f;
    public int health = 50;
    public GameObject bulletPrefab;
    public float bulletSpeed = 5f;
    public float fireInterval = 3f;
    public float darkenAmount = 0.5f;
    public float colorRecoveryTime = 0.5f;
    public AudioClip fireAudioClip;     // <-- assign in inspector
    private Rigidbody2D rb;

    private AudioSource audioSource;    // <--
    private Transform player;
    private Transform areaBoundary;
    private Vector2 targetPosition;
    private bool isMoving = false;
    private float idleTimer = 0f;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool isDead = false;        // <--

    void Start()
    {
        areaBoundary = transform.parent;
        if (areaBoundary == null)
        {
            Debug.LogError("Wisp's parent (SpawnAnchor) not found!");
            return;
        }
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer not found on Wisp!");
            return;
        }
        originalColor = spriteRenderer.color;

        player = GameObject.FindGameObjectWithTag("Player").transform;
        if (player == null)
        {
            Debug.LogError("Player object not found! Make sure the Player is tagged correctly.");
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        SetRandomTargetPosition();

        StartCoroutine(FireBulletAtPlayer());
    }

    void Update()
    {
        if (isMoving)
        {
            MoveTowardsTarget();
        }
        else
        {
            idleTimer += Time.deltaTime;
            if (idleTimer >= idleTime)
            {
                idleTimer = 0f;
                SetRandomTargetPosition();
            }
        }
    }

    void SetRandomTargetPosition()
    {
        if (areaBoundary != null)
        {
            Collider2D areaCollider = areaBoundary.GetComponent<Collider2D>();
            if (areaCollider != null)
            {
                Bounds bounds = areaCollider.bounds;
                float randomX = Random.Range(bounds.min.x, bounds.max.x);
                float randomY = Random.Range(bounds.min.y, bounds.max.y);
                targetPosition = new Vector2(randomX, randomY);
                isMoving = true;
            }
            else
            {
                Debug.LogError("SpawnAnchor does not have a Collider2D attached.");
            }
        }
    }

    void MoveTowardsTarget()
    {
        Vector2 currentPosition = transform.position;
        transform.position = Vector2.MoveTowards(currentPosition, targetPosition, speed * Time.deltaTime);

        if (Vector2.Distance(currentPosition, targetPosition) < 0.1f)
        {
            isMoving = false;
        }
    }

    IEnumerator FireBulletAtPlayer()
    {
        while (true)
        {
            yield return new WaitForSeconds(fireInterval);

            if (player != null && bulletPrefab != null)
            {
                Vector2 fireDirection = (player.position - transform.position).normalized;
                GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
                Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();

                if (bulletRb != null)
                {
                    bulletRb.velocity = fireDirection * bulletSpeed;

                    Collider2D bulletCollider = bullet.GetComponent<Collider2D>();
                    Collider2D wispCollider = GetComponent<Collider2D>();

                    if (bulletCollider != null && wispCollider != null)
                    {
                        Physics2D.IgnoreCollision(bulletCollider, wispCollider);
                        Collider2D[] enemyColliders = FindObjectsOfType<Collider2D>();
                        foreach (var enemyCollider in enemyColliders)
                        {
                            if (enemyCollider.CompareTag("Enemy"))
                            {
                                Physics2D.IgnoreCollision(bulletCollider, enemyCollider);
                            }
                        }
                    }
                }
                // Play fire audio
                if (fireAudioClip != null)
                  {
                GameObject sfxObject = new GameObject("Wisp_FireAudio");
                sfxObject.transform.position = transform.position;
                AudioSource sfxSource = sfxObject.AddComponent<AudioSource>();
                sfxSource.clip = fireAudioClip;
                if (audioSource != null)
                {
                    sfxSource.volume = audioSource.volume;
                    sfxSource.spatialBlend = audioSource.spatialBlend;
                    sfxSource.outputAudioMixerGroup = audioSource.outputAudioMixerGroup;
                }
                sfxSource.Play();
                Destroy(sfxObject, fireAudioClip.length);
                }
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {
            MonoBehaviour[] components = collision.gameObject.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour component in components)
            {
                System.Reflection.FieldInfo damageField = component.GetType().GetField("damage");
                if (damageField != null)
                {
                    int bulletDamage = (int)damageField.GetValue(component);
                    TakeDamage(bulletDamage);
                    break;
                }
            }
            Destroy(collision.gameObject);
        }
    }

    public void TakeDamage(int damageAmount)
    {
        if (isDead) return;                  // <--
        health -= damageAmount;
        Debug.Log("Wisp took damage! Remaining Health: " + health);

        StartCoroutine(TemporaryDarkenEffect());

        if (health <= 0)
        {
            isDead = true;                   // <--
            Die();
        }
    }

    IEnumerator TemporaryDarkenEffect()
    {
        if (spriteRenderer != null)
        {
            Color darkenedColor = Color.Lerp(originalColor, Color.black, darkenAmount);
            spriteRenderer.color = darkenedColor;

            float elapsedTime = 0f;
            while (elapsedTime < colorRecoveryTime)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / colorRecoveryTime;
                spriteRenderer.color = Color.Lerp(darkenedColor, originalColor, t);
                yield return null;
            }
            spriteRenderer.color = originalColor;
        }
    }

    void Die()
    {
        // Stop all movement immediately
        isMoving = false;
        rb.velocity = Vector2.zero;
        rb.simulated = false;

        GetComponent<Collider2D>().enabled = false;
        this.enabled = false;

        StartCoroutine(FadeOutAndDestroy());
    }

    IEnumerator FadeOutAndDestroy()
    {
        float fadeDuration = 1f;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }

        Destroy(gameObject);
    }
}