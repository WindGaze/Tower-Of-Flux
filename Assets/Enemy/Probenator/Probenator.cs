using System.Collections;
using UnityEngine;

public class Probenator : MonoBehaviour
{
    public int damage = 35;
    public float speed = 2f;
    public float idleTime = 2f;
    public int health = 50;
    public GameObject bulletPrefab;
    public float bulletSpeed = 5f;
    public float fireInterval = 3f;
    public float spreadAngle = 30f;
    public float bulletFollowDuration = 7f;
    public float darkenAmount = 0.5f;
    public float colorRecoveryTime = 0.5f;
    public int numberOfBullets = 2;

    [Header("Audio")]
    public AudioClip fireAudioClip; // Assign in Inspector

    [Header("Fire Target Points")]
    public GameObject fireTargetA;
    public GameObject fireTargetB;

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
    private Rigidbody2D rb;

    void Start()
    {
        areaBoundary = transform.parent;
        if (areaBoundary == null)
        {
            Debug.LogError("Probenator's parent (SpawnAnchor) not found!");
            return;
        }
        rb = GetComponent<Rigidbody2D>();

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer not found on Probenator!");
            return;
        }
        originalColor = spriteRenderer.color;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
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

        StartCoroutine(FireBulletsAtTargets());
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

    IEnumerator FireBulletsAtTargets()
    {
        while (true)
        {
            yield return new WaitForSeconds(fireInterval);

            if (bulletPrefab != null && !isDead)
            {
                bool soundPlayed = false;

                // Shoot toward fireTargetA if assigned
                if (fireTargetA != null)
                {
                    Vector2 dirA = (fireTargetA.transform.position - transform.position).normalized;
                    if (!soundPlayed && fireAudioClip != null && audioSource != null)
                    {
                        audioSource.pitch = Random.Range(1.2f, 1.8f);
                        audioSource.PlayOneShot(fireAudioClip);
                        audioSource.pitch = 1f;
                        soundPlayed = true;
                    }
                    FireDiagonalBullets(dirA);
                }

                // Shoot toward fireTargetB if assigned
                if (fireTargetB != null)
                {
                    Vector2 dirB = (fireTargetB.transform.position - transform.position).normalized;
                    if (!soundPlayed && fireAudioClip != null && audioSource != null)
                    {
                        audioSource.pitch = Random.Range(1.2f, 1.8f);
                        audioSource.PlayOneShot(fireAudioClip);
                        audioSource.pitch = 1f;
                        soundPlayed = true;
                    }
                    FireDiagonalBullets(dirB);
                }

                // Fallback: if both are null, shoot at player
                if (fireTargetA == null && fireTargetB == null && player != null)
                {
                    Vector2 dirPlayer = (player.position - transform.position).normalized;
                    if (fireAudioClip != null && audioSource != null)
                    {
                        audioSource.pitch = Random.Range(1.2f, 1.8f);
                        audioSource.PlayOneShot(fireAudioClip);
                        audioSource.pitch = 1f;
                    }
                    FireDiagonalBullets(dirPlayer);
                }
            }
        }
    }

    void FireDiagonalBullets(Vector2 fireDirection)
    {
        float startAngle = -(spreadAngle / 2);
        float angleStep = (numberOfBullets > 1) ? (spreadAngle / (numberOfBullets - 1)) : 0f;

        for (int i = 0; i < numberOfBullets; i++)
        {
            float angle = startAngle + (angleStep * i);

            Vector2 rotatedDirection = RotateVector(fireDirection, angle);

            GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
            Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();

            if (bulletRb != null)
            {
                bulletRb.velocity = rotatedDirection * bulletSpeed;
                StartCoroutine(FollowPlayer(bullet, rotatedDirection));
            }
        }
    }

    IEnumerator FollowPlayer(GameObject bullet, Vector2 originalDirection)
    {
        Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
        float elapsedTime = 0f;

        while (elapsedTime < bulletFollowDuration)
        {
            elapsedTime += Time.deltaTime;

            if (player != null && bulletRb != null)
            {
                Vector2 followDirection = (player.position - bullet.transform.position).normalized;
                bulletRb.velocity = followDirection * bulletSpeed;
            }

            yield return null;
        }

        if (bulletRb != null)
        {
            bulletRb.velocity = originalDirection * bulletSpeed;
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
        Debug.Log("Probenator took damage! Remaining Health: " + health);

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