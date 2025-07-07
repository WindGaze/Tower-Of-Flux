using System.Collections;
using UnityEngine;

public class SlimeChild : MonoBehaviour
{
    public int damage = 10;
    public float speed = 1f;
    public int health = 100;
    public float darkenAmount = 0.5f;
    public float colorRecoveryTime = 0.5f;
    public float moveToPlayerInterval = 3f;
    public float followPlayerDuration = 4f;

    [Header("Death Effect")]
    public AudioClip deathSound;

    private AudioSource audioSource;
    private Transform player;
    private Transform areaBoundary;
    private Vector2 targetPosition;
    private bool isMoving = false;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool followingPlayer = false;
    private Rigidbody2D rb;
    private float collisionCheckDistance = 0.1f;
    private ContactFilter2D wallFilter;
    private bool isDead = false;
    private Coroutine moveToPlayerCoroutine; // <-- to track coroutine
    private Coroutine moveToRandomCoroutine;

    void Start()
    {
        areaBoundary = transform.parent;
        if (areaBoundary == null)
        {
            Debug.LogError("Slime's parent (SpawnAnchor) not found!");
            return;
        }

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer not found on Slime!");
            return;
        }
        originalColor = spriteRenderer.color;

        player = GameObject.FindGameObjectWithTag("Player").transform;
        if (player == null)
            Debug.LogError("Player object not found! Make sure the Player is tagged correctly.");

        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D not found on Slime!");
            return;
        }
        rb.isKinematic = true;

        wallFilter = new ContactFilter2D();
        wallFilter.SetLayerMask(LayerMask.GetMask("Wall"));
        wallFilter.useLayerMask = true;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        SetRandomTargetPosition();
        moveToPlayerCoroutine = StartCoroutine(MoveToPlayerEveryInterval());
    }

    void Update()
    {
        if (!followingPlayer && isMoving)
            MoveTowardsTarget();
    }

    IEnumerator MoveToPlayerEveryInterval()
    {
        while (true)
        {
            yield return new WaitForSeconds(moveToPlayerInterval);

            if (player != null)
                moveToRandomCoroutine = StartCoroutine(MoveTowardPlayerForDuration());
        }
    }

    IEnumerator MoveTowardPlayerForDuration()
    {
        followingPlayer = true;
        float elapsedTime = 0f;

        while (elapsedTime < followPlayerDuration)
        {
            elapsedTime += Time.deltaTime;
            if (player != null)
            {
                Vector2 playerDirection = (player.position - transform.position).normalized;

                if (!IsPathBlocked(playerDirection))
                {
                    transform.position += (Vector3)(playerDirection * speed * Time.deltaTime);
                }
            }
            yield return null;
        }

        followingPlayer = false;
        SetRandomTargetPosition();
    }

    public void SetRandomTargetPosition()
    {
        if (followingPlayer) return;

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
        Vector2 direction = (targetPosition - currentPosition).normalized;

        if (!IsPathBlocked(direction))
        {
            transform.position = Vector2.MoveTowards(currentPosition, targetPosition, speed * Time.deltaTime);
        }
        else
        {
            isMoving = false;
        }

        if (Vector2.Distance(currentPosition, targetPosition) < 0.1f)
        {
            isMoving = false;
        }
    }

    bool IsPathBlocked(Vector2 direction)
    {
        RaycastHit2D[] hits = new RaycastHit2D[1];
        int hitCount = Physics2D.Raycast(transform.position, direction, wallFilter, hits, collisionCheckDistance);
        Debug.DrawRay(transform.position, direction * collisionCheckDistance, Color.red);
        return hitCount > 0;
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
            rb.velocity = Vector2.zero;
            Destroy(collision.gameObject);
        }
    }

    public void TakeDamage(int damageAmount)
    {
        if (isDead) return;
        health -= damageAmount;
        Debug.Log("SlimeChild took damage! Remaining Health: " + health);

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
        Debug.Log("SlimeChild has died!");

        // -------- FREEZE movement and disable all colliders ----------
        isMoving = false;
        followingPlayer = false;
        // Cancel all running movement coroutines (optional)
        if (moveToPlayerCoroutine != null)
            StopCoroutine(moveToPlayerCoroutine);
        if (moveToRandomCoroutine != null)
            StopCoroutine(moveToRandomCoroutine);

        rb.velocity = Vector2.zero;
        rb.simulated = false; // freeze rb/movement
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false; // disable collider

        // Also disable this script, so Update, etc stop running
        this.enabled = false;

        if (deathSound != null && audioSource != null)
            audioSource.PlayOneShot(deathSound);

        float deathSoundDuration = (deathSound != null) ? deathSound.length : 0f;
        StartCoroutine(FadeOutAndDestroy(deathSoundDuration));
    }

    IEnumerator FadeOutAndDestroy(float delay = 0f)
    {
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        float fadeDuration = 0.3f;
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

    IEnumerator MoveSmallSlime(Rigidbody2D rb, Vector2 direction)
    {
        float moveDuration = 0.5f;
        float elapsedTime = 0f;
        float speed = 2f;

        while (elapsedTime < moveDuration)
        {
            rb.MovePosition(rb.position + direction * speed * Time.deltaTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }
}