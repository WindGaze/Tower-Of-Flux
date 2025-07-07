using System.Collections;
using UnityEngine;

public class TheRemain : MonoBehaviour
{
    public GameObject spikePrefab; // Prefab for the spike
    public float roamSpeed = 2f; // Speed of roaming
    public float spikeInterval = 7f; // Interval to spawn spikes
    public float roamDuration = 6f; // Duration to roam
    public float idleDuration = 3f; // Duration to idle

    public int health = 50;
    public float darkenAmount = 0.5f;
    public float colorRecoveryTime = 0.5f;

    private Transform playerTransform; // Reference to the player
    private Transform areaBoundary; // Roaming area boundary
    private Vector2 targetPosition; // Roaming target position
    private bool isMoving = false;
    private Rigidbody2D rb;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("Animator not found on TheRemain!");
        }
        rb = GetComponent<Rigidbody2D>();
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogError("Player not found! Ensure the player object has the 'Player' tag.");
            return;
        }

        // Get the roam area (parent object)
        areaBoundary = transform.parent;
        if (areaBoundary == null)
        {
            Debug.LogError("Roam area not set! Ensure TheRemain is a child of a valid roam area.");
            return;
        }

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        else
        {
            Debug.LogError("SpriteRenderer not found on TheRemain!");
            return;
        }

        SetRandomTargetPosition();
        StartCoroutine(BehaviorCycle());
        StartCoroutine(SpikeSpawner());
    }

    void Update()
    {
        if (isMoving)
        {
            Roam();
            if (animator != null)
            {
                animator.SetInteger("AnimState", 1);
            }
        }
        else
        {
            if (animator != null)
            {
                animator.SetInteger("AnimState", 0);
            }
        }
    }

    private IEnumerator BehaviorCycle()
    {
        while (true)
        {
            // Roam for a certain duration
            isMoving = true;
            yield return new WaitForSeconds(roamDuration);

            // Idle for a certain duration
            isMoving = false;
            yield return new WaitForSeconds(idleDuration);

            // Set a new random target position after idling
            SetRandomTargetPosition();
        }
    }

    private IEnumerator SpikeSpawner()
    {
        while (true)
        {
            yield return new WaitForSeconds(spikeInterval);

            if (playerTransform != null)
            {
                SpawnSpike();
            }
        }
    }

    private void Roam()
    {
        Vector2 currentPosition = transform.position;

        // Direction vector for facing logic
        Vector2 direction = (targetPosition - currentPosition).normalized;

    
        if (direction.x > 0)
        {
            spriteRenderer.flipX = false; // Moving right: NOT flipped
        }
        else if (direction.x < 0)
        {
            spriteRenderer.flipX = true;  // Moving left: FLIPPED
}
        // Move towards the target position
        transform.position = Vector2.MoveTowards(currentPosition, targetPosition, roamSpeed * Time.deltaTime);

        // If close to the target position, set a new one
        if (Vector2.Distance(currentPosition, targetPosition) < 0.1f)
        {
            SetRandomTargetPosition();
        }
    }

    private void SetRandomTargetPosition()
    {
        Collider2D areaCollider = areaBoundary.GetComponent<Collider2D>();
        if (areaCollider != null)
        {
            Bounds bounds = areaCollider.bounds;

            // Pick a random point in the roam area
            targetPosition = new Vector2(
                Random.Range(bounds.min.x, bounds.max.x),
                Random.Range(bounds.min.y, bounds.max.y)
            );
        }
        else
        {
            Debug.LogError("Roam area does not have a Collider2D!");
        }
    }

    private void SpawnSpike()
    {
        if (spikePrefab == null || playerTransform == null)
        {
            Debug.LogError("Spike prefab or player reference is missing!");
            return;
        }

        // Spawn spike directly on the player's position
        Instantiate(spikePrefab, playerTransform.position, Quaternion.identity);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {
            // Use TryGetComponent to check for any MonoBehaviour that has a 'damage' field
            // This is more generic than checking for specific script types
            MonoBehaviour[] components = collision.gameObject.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour component in components)
            {
                // Using reflection to find a public 'damage' field in any of the attached scripts
                System.Reflection.FieldInfo damageField = component.GetType().GetField("damage");
                if (damageField != null)
                {
                    int bulletDamage = (int)damageField.GetValue(component);
                    TakeDamage(bulletDamage);
                    break; // Use the first damage value found
                }
            }

            Destroy(collision.gameObject);
        }
    }

    public void TakeDamage(int damageAmount)
    {
        health -= damageAmount;
        Debug.Log("TheRemain took damage! Remaining Health: " + health);

        StartCoroutine(TemporaryDarkenEffect());

        if (health <= 0)
        {
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