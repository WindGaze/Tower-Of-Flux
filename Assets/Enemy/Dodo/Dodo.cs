using System.Collections;
using UnityEngine;

public class Dodo : MonoBehaviour
{
    public float speed = 2f;
    public int damage = 20;
    public float idleTime = 2f;
    public int health = 50;
    public GameObject bulletPrefab;
    public float bulletSpeed = 5f;
    public float fireInterval = 3f;
    public float spreadAngle = 15f;
    public float darkenAmount = 0.5f;
    public float colorRecoveryTime = 0.5f;
    public bool isBoss = true;

    [Header("Audio")]
    public AudioClip fireAudioClip; // Assign in inspector

    private AudioSource audioSource;
    private Transform player;
    private Transform areaBoundary;
    private Vector2 targetPosition;
    private bool isMoving = false;
    private float idleTimer = 0f;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool isDead = false;
    private Rigidbody2D rb;
    private Collider2D dodoCollider;

    void Start()
    {
        areaBoundary = transform.parent;
        if (areaBoundary == null)
        {
            Debug.LogError("Dodo's parent (SpawnAnchor) not found!");
            return;
        }

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer not found on Dodo!");
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

        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody2D>();

        dodoCollider = GetComponent<Collider2D>();
        if (dodoCollider == null)
            dodoCollider = gameObject.AddComponent<BoxCollider2D>();

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
                FireSpreadBullets(fireDirection);

                // Play fire sound
                if (fireAudioClip != null && audioSource != null)
                    audioSource.PlayOneShot(fireAudioClip);
            }
        }
    }

    void FireSpreadBullets(Vector2 fireDirection)
    {
        for (int i = -1; i <= 1; i++)
        {
            float angle = i * spreadAngle;
            Vector2 rotatedDirection = RotateVector(fireDirection, angle);

            GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
            Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();

            if (bulletRb != null)
            {
                bulletRb.velocity = rotatedDirection * bulletSpeed;
                Collider2D bulletCollider = bullet.GetComponent<Collider2D>();

                if (bulletCollider != null && dodoCollider != null)
                {
                    Physics2D.IgnoreCollision(bulletCollider, dodoCollider);
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
        }
    }

    Vector2 RotateVector(Vector2 vector, float angle)
    {
        float rad = angle * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);
        return new Vector2(cos * vector.x - sin * vector.y, sin * vector.x + cos * vector.y);
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
        if (isDead) return;
        health -= damageAmount;
        Debug.Log("Dodo took damage! Remaining Health: " + health);

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
        Debug.Log("Dodo has died!");
        // Freeze movement/collision immediately
        isMoving = false;
        if (rb != null) { rb.velocity = Vector2.zero; rb.simulated = false; }
        if (dodoCollider != null) dodoCollider.enabled = false;
        this.enabled = false;
        StartCoroutine(FadeOutAndDestroy(0.4f)); // 0.4 seconds fade
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
}