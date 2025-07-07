using System.Collections;
using UnityEngine;

public class TheEyes : MonoBehaviour
{
    public int damage = 30;
    public float speed = 2f;
    public int health = 50;
    public GameObject bulletPrefab;
    public float bulletSpeed = 5f;
    public float fireInterval = 3f;
    public float bulletOffset = 0.5f;
    public float darkenAmount = 0.5f;
    public float colorRecoveryTime = 0.5f;

    [Header("Audio")]
    public AudioClip fireAudioClip; // assign in inspector

    private Transform player;
    private Transform areaBoundary;
    private Vector2 targetPosition;
    private bool isMoving = false;
    private float idleTimer = 0f;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Collider2D myCollider;
    private AudioSource audioSource;
    private bool isDead = false;

    void Start()
    {
        areaBoundary = transform.parent;
        if (areaBoundary == null)
        {
            Debug.LogError("TheEyes' parent (SpawnAnchor) not found!");
            return;
        }

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer not found on TheEyes!");
            return;
        }
        originalColor = spriteRenderer.color;

        player = GameObject.FindGameObjectWithTag("Player").transform;
        if (player == null)
        {
            Debug.LogError("Player object not found! Make sure the Player is tagged correctly.");
        }

        myCollider = GetComponent<Collider2D>();
        if (myCollider == null)
        {
            myCollider = gameObject.AddComponent<BoxCollider2D>();
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        SetRandomTargetPosition();

        StartCoroutine(FireBulletAtPlayer());
    }

    void Update()
    {
        if (isDead) return;

        if (isMoving)
        {
            MoveTowardsTarget();
        }
        else
        {
            idleTimer += Time.deltaTime;
            if (idleTimer >= 2f)
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

            if (player != null && bulletPrefab != null && !isDead)
            {
                Vector2 fireDirection = (player.position - transform.position).normalized;

                // Play fire sound with random pitch
                if (fireAudioClip != null && audioSource != null)
                {
                    audioSource.pitch = Random.Range(1.2f, 1.8f);
                    audioSource.PlayOneShot(fireAudioClip);
                    audioSource.pitch = 1f;
                }

                FireDoubleBullets(fireDirection);
            }
        }
    }

    void FireDoubleBullets(Vector2 fireDirection)
    {
        Vector2 bulletPosLeft = (Vector2)transform.position + Vector2.Perpendicular(fireDirection) * bulletOffset;
        Vector2 bulletPosRight = (Vector2)transform.position - Vector2.Perpendicular(fireDirection) * bulletOffset;

        GameObject bulletLeft = Instantiate(bulletPrefab, bulletPosLeft, Quaternion.identity);
        Rigidbody2D bulletLeftRb = bulletLeft.GetComponent<Rigidbody2D>();
        if (bulletLeftRb != null)
        {
            bulletLeftRb.velocity = fireDirection * bulletSpeed;
            IgnoreEnemyCollisions(bulletLeft);
        }

        GameObject bulletRight = Instantiate(bulletPrefab, bulletPosRight, Quaternion.identity);
        Rigidbody2D bulletRightRb = bulletRight.GetComponent<Rigidbody2D>();
        if (bulletRightRb != null)
        {
            bulletRightRb.velocity = fireDirection * bulletSpeed;
            IgnoreEnemyCollisions(bulletRight);
        }
    }

    void IgnoreEnemyCollisions(GameObject bullet)
    {
        Collider2D bulletCollider = bullet.GetComponent<Collider2D>();
        if (myCollider != null && bulletCollider != null)
        {
            Physics2D.IgnoreCollision(bulletCollider, myCollider);
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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;

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
        Debug.Log("TheEyes took damage! Remaining Health: " + health);

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
        Debug.Log("TheEyes has died!");
        if (myCollider != null) myCollider.enabled = false;
        this.enabled = false;
        StartCoroutine(FadeOutAndDestroy(0.4f));
    }

    IEnumerator FadeOutAndDestroy(float fadeDuration)
    {
        float elapsed = 0f;
        Color baseColor = spriteRenderer.color;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            spriteRenderer.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
            yield return null;
        }
        Destroy(gameObject);
    }
}