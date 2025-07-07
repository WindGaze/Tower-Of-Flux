using System.Collections;
using UnityEngine;

public class Maggot : MonoBehaviour
{
    public int damage = 15;
    public float speed = 2f;
    public float chargeSpeed = 8f;
    public float idleTime = 2f;
    public int health = 50;
    public float darkenAmount = 0.5f;
    public float colorRecoveryTime = 0.5f;
    public float chargeDistance = 5f;
    public float chargeDuration = 0.5f;
    public float chargeCooldown = 3f;
    public float initialChargeDelay = 4f;

    [Header("Audio")]
    public AudioClip chargeClip;
    private AudioSource audioSource;

    private Transform player;
    private Transform areaBoundary;
    private Vector2 targetPosition;
    private bool isMoving = false;
    private float idleTimer = 0f;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool isCharging = false;
    private bool isOnChargeCooldown = false;
    private bool canCharge = false;

    private Rigidbody2D rb;
    private float collisionCheckDistance = 0.1f;
    private ContactFilter2D wallFilter;
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        areaBoundary = transform.parent;
        if (areaBoundary == null)
        {
            Debug.LogError("Maggot's parent (SpawnAnchor) not found!");
            return;
        }

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer not found on Maggot!");
            return;
        }
        originalColor = spriteRenderer.color;

        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null)
        {
            Debug.LogError("Player object not found! Make sure the Player is tagged correctly.");
        }

        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D not found on Maggot!");
            return;
        }
        rb.isKinematic = true;

        wallFilter = new ContactFilter2D();
        wallFilter.SetLayerMask(LayerMask.GetMask("Wall"));
        wallFilter.useLayerMask = true;

        SetRandomTargetPosition();
        StartCoroutine(InitialChargeDelay());

        // Audio setup
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Update()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (!isCharging && canCharge && distanceToPlayer <= chargeDistance && !isOnChargeCooldown)
        {
            StartCoroutine(ChargeTowardsPlayer());
        }
        else
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

        if (animator != null)
        {
            animator.SetInteger("AnimState", (isMoving || isCharging) ? 1 : 0);
        }
    }

    IEnumerator InitialChargeDelay()
    {
        yield return new WaitForSeconds(initialChargeDelay);
        canCharge = true;
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
        Vector2 direction = (targetPosition - currentPosition).normalized;
        if (direction.x > 0) // Moving right
        {
            spriteRenderer.flipX = true;
        }
        else if (direction.x < 0) // Moving left
        {
            spriteRenderer.flipX = false;
        }
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

    IEnumerator ChargeTowardsPlayer()
    {
        // --- Play charge sound here ---
        PlayChargeAudio();

        isCharging = true;
        isOnChargeCooldown = true;

        Vector2 chargeDirection = (player.position - transform.position).normalized;
        float chargeTime = 0f;
        if (chargeDirection.x > 0)
        {
            spriteRenderer.flipX = true;
        }
        else if (chargeDirection.x < 0)
        {
            spriteRenderer.flipX = false;
        }
        while (chargeTime < chargeDuration)
        {
            if (!IsPathBlocked(chargeDirection))
            {
                transform.position += (Vector3)(chargeDirection * chargeSpeed * Time.deltaTime);
            }
            else
            {
                break;
            }

            chargeTime += Time.deltaTime;
            yield return null;
        }

        isCharging = false;
        yield return new WaitForSeconds(chargeCooldown);
        isOnChargeCooldown = false;
    }

    private void PlayChargeAudio()
    {
        if (chargeClip != null && audioSource != null)
        {
            audioSource.pitch = 1f;
            audioSource.PlayOneShot(chargeClip);
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

            Destroy(collision.gameObject);
        }
    }

    public void TakeDamage(int damageAmount)
    {
        health -= damageAmount;
        Debug.Log("Maggot took damage! Remaining Health: " + health);
        StartCoroutine(TemporaryDarkenEffect());

        if (health <= 0)
        {
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
        Debug.Log("Maggot has died!");

        // Stop all movement immediately
        isMoving = false;
        isCharging = false;
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