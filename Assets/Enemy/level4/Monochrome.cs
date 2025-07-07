using System.Collections;
using UnityEngine;

public class Monochrome : MonoBehaviour
{
    public int damage = 10;
    public float speed = 1f;
    public int health = 100;
    public float darkenAmount = 0.5f;
    public float colorRecoveryTime = 0.5f;
    public float moveToPlayerInterval = 3f;
    public float followPlayerDuration = 4f;

    // SOUND
    public AudioClip deathSound; // Assign in Inspector
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
    private Collider2D areaCollider;
    private Animator animator;

    private bool isDead = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
            Debug.LogError("Animator not found on Monochrome!");

        areaBoundary = transform.parent;
        if (areaBoundary == null)
        {
            Debug.LogError("Monochrome's parent (SpawnAnchor) not found!");
            return;
        }

        areaCollider = areaBoundary.GetComponent<Collider2D>();
        if (areaCollider == null)
        {
            Debug.LogError("SpawnAnchor does not have a Collider2D attached!");
            return;
        }

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer not found on Monochrome!");
            return;
        }
        originalColor = spriteRenderer.color;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null)
        {
            Debug.LogError("Player object not found! Make sure the Player is tagged correctly.");
        }
        else
        {
            player = playerObj.transform;
        }

        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D not found on Monochrome!");
            return;
        }
        rb.isKinematic = true;

        wallFilter = new ContactFilter2D();
        wallFilter.SetLayerMask(LayerMask.GetMask("Wall"));
        wallFilter.useLayerMask = true;

        // AUDIO
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        SetRandomTargetPosition();
        StartCoroutine(MoveToPlayerEveryInterval());
    }

    void Update()
    {
        if (isDead) return;

        if (!followingPlayer && isMoving)
        {
            MoveTowardsTarget();
        }
        UpdateAnimationState();
    }

    void UpdateAnimationState()
    {
        if (animator != null)
        {
            if (isMoving || followingPlayer)
                animator.SetInteger("AnimState", 1);
            else
                animator.SetInteger("AnimState", 0);
        }
    }

    IEnumerator MoveToPlayerEveryInterval()
    {
        while (true)
        {
            if (isDead) yield break;
            yield return new WaitForSeconds(moveToPlayerInterval);

            if (isDead) yield break;
            if (player != null)
            {
                yield return MoveTowardPlayerForDuration();
            }
        }
    }

    IEnumerator MoveTowardPlayerForDuration()
    {
        followingPlayer = true;
        float elapsedTime = 0f;

        while (elapsedTime < followPlayerDuration)
        {
            if (isDead) yield break;

            elapsedTime += Time.deltaTime;
            if (player != null)
            {
                Vector2 playerDirection = (player.position - transform.position).normalized;

                // Sprite flipping
                if (playerDirection.x > 0f)
                {
                    spriteRenderer.flipX = false;
                }
                else if (playerDirection.x < 0f)
                {
                    spriteRenderer.flipX = true;
                }

                if (!IsPathBlocked(playerDirection))
                {
                    transform.position += (Vector3)(playerDirection * speed * Time.deltaTime);
                    ClampPositionToBounds();
                }
            }
            yield return null;
        }

        followingPlayer = false;
        SetRandomTargetPosition();
    }

    void SetRandomTargetPosition()
    {
        if (areaBoundary != null && areaCollider != null)
        {
            Bounds bounds = areaCollider.bounds;
            float randomX = Random.Range(bounds.min.x, bounds.max.x);
            float randomY = Random.Range(bounds.min.y, bounds.max.y);
            targetPosition = new Vector2(randomX, randomY);
            isMoving = true;
        }
    }

    void MoveTowardsTarget()
    {
        if (isDead) return;

        Vector2 currentPosition = transform.position;
        Vector2 direction = (targetPosition - currentPosition).normalized;

        // Sprite flipping
        if (direction.x > 0f)
        {
            spriteRenderer.flipX = false;
        }
        else if (direction.x < 0f)
        {
            spriteRenderer.flipX = true;
        }

        if (!IsPathBlocked(direction))
        {
            transform.position = Vector2.MoveTowards(currentPosition, targetPosition, speed * Time.deltaTime);
            ClampPositionToBounds();
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

    void ClampPositionToBounds()
    {
        if (areaCollider != null)
        {
            Bounds bounds = areaCollider.bounds;
            Vector2 clampedPosition = transform.position;

            clampedPosition.x = Mathf.Clamp(clampedPosition.x, bounds.min.x, bounds.max.x);
            clampedPosition.y = Mathf.Clamp(clampedPosition.y, bounds.min.y, bounds.max.y);

            transform.position = clampedPosition;
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

            rb.velocity = Vector2.zero;
            Destroy(collision.gameObject);
        }
    }

    public void TakeDamage(int damageAmount)
    {
        if (isDead) return;
        health -= damageAmount;

        if (health <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(TemporaryDarkenEffect());
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
        isDead = true;
        isMoving = false;
        followingPlayer = false;
        rb.velocity = Vector2.zero;
        rb.simulated = false;

        GetComponent<Collider2D>().enabled = false;
        this.enabled = false;

        if (deathSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(deathSound);
        }

        float deathSoundDuration = (deathSound != null) ? deathSound.length : 0f;
        StartCoroutine(FadeOutAndDestroy(deathSoundDuration));
    }

    IEnumerator FadeOutAndDestroy(float delay = 0f)
    {
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        float fadeDuration = 0.4f;
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