using System.Collections;
using UnityEngine;

public class AquaLantern : MonoBehaviour
{
    public GameObject laserPrefab;
    public float teleportInterval = 4f;
    public float laserInterval = 6f;
    public float laserDelay = 6f; // Start firing after 6 seconds

    public float darkenAmount = 0.5f;
    public float colorRecoveryTime = 0.5f;
    public int health = 100;
    private Rigidbody2D rb;

    [Header("Audio")]
    public AudioClip fireAudioClip; // Assign in Inspector

    private AudioSource audioSource;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    private Collider2D spawnAreaCollider;
    private bool isInitialized = false;
    private Transform playerTransform;

    private float timeSinceLaserFired = 0f;
    private Collider2D myCollider;
    private bool isDead = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        Transform parentTransform = transform.parent;
        if (parentTransform != null)
        {
            spawnAreaCollider = parentTransform.GetComponent<Collider2D>();
        }

        if (spawnAreaCollider == null)
        {
            Debug.LogError("Spawn area collider not found! Make sure AquaLantern is a child of SpawnAnchor.");
            return;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogError("Player not found! Make sure the player object has the 'Player' tag.");
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        myCollider = GetComponent<Collider2D>();
        if (myCollider == null)
            myCollider = gameObject.AddComponent<BoxCollider2D>();

        isInitialized = true;
        StartCoroutine(AquaLanternBehavior());
    }

    private IEnumerator AquaLanternBehavior()
    {
        float elapsedTime = 0f;
        yield return new WaitForSeconds(laserDelay);

        while (health > 0 && isInitialized && !isDead)
        {
            if (elapsedTime % teleportInterval < Time.deltaTime)
            {
                TeleportToRandomLocation();
            }

            if (timeSinceLaserFired >= laserInterval)
            {
                FireLaser();
                timeSinceLaserFired = 0f;
            }

            timeSinceLaserFired += Time.deltaTime;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    private void TeleportToRandomLocation()
    {
        if (spawnAreaCollider == null)
        {
            Debug.LogError("Teleport area is not assigned!");
            return;
        }

        Bounds bounds = spawnAreaCollider.bounds;

        Vector2 randomPoint;
        int attempts = 10;
        do
        {
            randomPoint = new Vector2(
                Random.Range(bounds.min.x, bounds.max.x),
                Random.Range(bounds.min.y, bounds.max.y)
            );
            attempts--;
        }
        while (!spawnAreaCollider.OverlapPoint(randomPoint) && attempts > 0);

        if (attempts > 0)
        {
            transform.position = new Vector3(randomPoint.x, randomPoint.y, transform.position.z);
            Debug.Log("AquaLantern teleported to: " + randomPoint);
        }
        else
        {
            Debug.LogWarning("AquaLantern failed to find a valid teleportation point!");
        }
    }

    private void FireLaser()
    {
        if (laserPrefab == null)
        {
            Debug.LogError("Laser prefab is not assigned!");
            return;
        }

        if (playerTransform == null)
        {
            Debug.LogError("Player is not assigned or found!");
            return;
        }

        // Create the laser and aim at the player
        GameObject laser = Instantiate(laserPrefab, transform.position, Quaternion.identity);
        Vector2 directionToPlayer = (playerTransform.position - transform.position).normalized;
        laser.transform.right = directionToPlayer;

        Destroy(laser, 1f);

        // Play fire audio with random high pitch
        if (fireAudioClip != null && audioSource != null)
        {
            audioSource.pitch = Random.Range(1.2f, 1.8f);
            audioSource.PlayOneShot(fireAudioClip);
            audioSource.pitch = 1f; // Reset pitch after firing
        }

        Debug.Log("AquaLantern fired a laser at the player!");
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

        StartCoroutine(TemporaryDarkenEffect());

        if (health <= 0)
        {
            isDead = true;
            Die();
        }
    }

    private IEnumerator TemporaryDarkenEffect()
    {
        if (spriteRenderer != null)
        {
            Color darkenedColor = Color.Lerp(originalColor, Color.black, darkenAmount);
            spriteRenderer.color = darkenedColor;

            float elapsedTime = 0f;
            while (elapsedTime < colorRecoveryTime)
            {
                elapsedTime += Time.deltaTime;
                spriteRenderer.color = Color.Lerp(darkenedColor, originalColor, elapsedTime / colorRecoveryTime);
                yield return null;
            }

            spriteRenderer.color = originalColor;
        }
    }

    private void Die()
    {
        Debug.Log("AquaLantern has been destroyed!");

        // Freeze collider and movement, disable script
        if (myCollider != null) myCollider.enabled = false;
        this.enabled = false;

        // Optional: stop all coroutines so it never shoots or moves again
        StopAllCoroutines();

        StartCoroutine(FadeOutAndDestroy(0.4f));
    }

    private IEnumerator FadeOutAndDestroy(float fadeDuration)
    {
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            if (spriteRenderer != null)
                spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }
        Destroy(gameObject);
    }
}