using System.Collections;
using UnityEngine;

public class Ghost : MonoBehaviour
{
    public float speed = 2f;
    public int damage = 30;
    public int health = 50;
    public float darkenAmount = 0.5f;
    public float colorRecoveryTime = 0.5f;
    public float bulletFireInterval = 6f;
    public float roamDuration = 5f;
    public GameObject bulletPrefab;
    public float bulletSpeed = 5f;
    [Header("Audio")]
    public AudioClip fireAudioClip; // Assign in inspector

    private AudioSource audioSource;
    private Transform player;
    private Collider2D areaBoundary;
    private Vector2 direction;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool isRoaming = false;
    private Collider2D ghostCollider;
    private bool isDead = false;

    void Start()
    {
        areaBoundary = transform.parent.GetComponent<Collider2D>();
        if (areaBoundary == null)
        {
            Debug.LogError("Ghost's parent (SpawnAnchor) does not have a Collider2D component!");
            return;
        }

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer not found on Ghost!");
            return;
        }
        originalColor = spriteRenderer.color;

        ghostCollider = GetComponent<Collider2D>();
        if (ghostCollider == null)
        {
            Debug.LogError("Collider2D not found on Ghost!");
            return;
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        player = GameObject.FindGameObjectWithTag("Player").transform;
        if (player == null)
        {
            Debug.LogError("Player object not found! Make sure the Player is tagged correctly.");
        }

        StartCoroutine(FireBulletRoutine());
        StartCoroutine(RoamRoutine());
    }

    void Update()
    {
        if (isDead) return;                   // <--
        if (!isRoaming)
        {
            MoveTowardsPlayer();
        }
        else
        {
            MoveInDirection();
        }
    }

    void MoveTowardsPlayer()
    {
        if (player != null)
        {
            direction = (player.position - transform.position).normalized;
            Vector2 newPosition = (Vector2)transform.position + direction * speed * Time.deltaTime;

            if (areaBoundary.OverlapPoint(newPosition))
            {
                transform.position = newPosition;
            }
        }
    }

    void MoveInDirection()
    {
        Vector2 newPosition = (Vector2)transform.position + direction * speed * Time.deltaTime;

        if (areaBoundary.OverlapPoint(newPosition))
        {
            transform.position = newPosition;
        }
        else
        {
            ChooseRandomDirection();
        }
    }

    void ChooseRandomDirection()
    {
        direction = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
    }

    IEnumerator FireBulletRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(bulletFireInterval);

            if (player != null && bulletPrefab != null)
            {
                Vector2 fireDirection = (player.position - transform.position).normalized;
                GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
                Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();

                if (bulletRb != null)
                {
                    bulletRb.velocity = fireDirection * bulletSpeed;

                    Collider2D bulletCollider = bullet.GetComponent<Collider2D>();
                    if (bulletCollider != null && ghostCollider != null)
                    {
                        Physics2D.IgnoreCollision(bulletCollider, ghostCollider);
                    }
                }

                // Play fire audio with higher random pitch
                if (fireAudioClip != null && audioSource != null)
                {
                    audioSource.pitch = Random.Range(0.9f, 1.8f);          // Higher pitch range
                    audioSource.PlayOneShot(fireAudioClip);
                    audioSource.pitch = 1f; // Reset to default after play
                }
            }
        }
    }

    IEnumerator RoamRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(bulletFireInterval);

            isRoaming = true;
            ghostCollider.enabled = false;
            SetOpacity(0.4f);

            yield return new WaitForSeconds(roamDuration);

            isRoaming = false;
            ghostCollider.enabled = true;
            SetOpacity(1f);
        }
    }

    void SetOpacity(float opacity)
    {
        Color color = spriteRenderer.color;
        color.a = opacity;
        spriteRenderer.color = color;
    }

    public void TakeDamage(int damageAmount)
    {
        if (isDead) return;
        health -= damageAmount;
        Debug.Log("Ghost took damage! Remaining Health: " + health);

        StartCoroutine(TemporaryDarkenEffect());

        if (health <= 0)
        {
            isDead = true;
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
        Debug.Log("Ghost has died!");

        // Disable collider, movement, script
        isRoaming = false;
        if (ghostCollider != null) ghostCollider.enabled = false;
        this.enabled = false;

        // Optional: stop all coroutines -- disables future bullet or roam
        StopAllCoroutines();

        StartCoroutine(FadeOutAndDestroy(0.4f)); // 0.4 sec fade
    }

    IEnumerator FadeOutAndDestroy(float fadeDuration)
    {
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
}