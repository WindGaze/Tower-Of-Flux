using UnityEngine;
using System.Collections;

public class ShadowGolemProjectile : MonoBehaviour
{
    public float lifetime = 5f;
    public float freezeDuration = 3f;
    public int damage = 10;
    private Vector2 direction;
    private float speed;
    private Rigidbody2D rb;
    private bool canHitPlayer = true;
    private float hitCooldown = 0.5f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("No Rigidbody2D found on the bullet object!");
        }
        else
        {
            rb.isKinematic = false;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            Collider2D collider = GetComponent<Collider2D>();
            if (collider != null) { collider.isTrigger = true; }
            else { Debug.LogError("No Collider2D found on the bullet object!"); }
        }
    }

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void FixedUpdate()
    {
        if (rb != null)
        {
            rb.velocity = direction * speed;
        }
    }

    public void Initialize(Vector2 dir, float spd)
    {
        direction = dir.normalized;
        speed = spd;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!canHitPlayer) return;

        if (collision.CompareTag("Player"))
        {
            PlayerMovement playerMovement = collision.GetComponent<PlayerMovement>();
            if (playerMovement != null)
            {
                // Damage the player just like EnemyBullet
                playerMovement.TakeDamage(damage);

                // Freeze the player
                StartCoroutine(FreezePlayer(playerMovement));
            }

            // Optional: Visual feedback
            StartCoroutine(FlashPlayer(collision.gameObject));

            // Start cooldown before the bullet can hit the player again
            StartCoroutine(HitCooldown());
        }
    }

    private IEnumerator HitCooldown()
    {
        canHitPlayer = false;
        yield return new WaitForSeconds(hitCooldown);
        canHitPlayer = true;
    }

    private IEnumerator FreezePlayer(PlayerMovement playerMovement)
    {
        // Cache components
        Rigidbody2D playerRb = playerMovement.GetComponent<Rigidbody2D>();
        Animator animator = playerMovement.GetComponent<Animator>();
        SpriteRenderer renderer = playerMovement.GetComponent<SpriteRenderer>();

        // Set frozen flag and visuals
        playerMovement.isFrozen = true;

        if (playerRb != null)
            playerRb.velocity = Vector2.zero;
        if (renderer != null)
            renderer.color = new Color(0.5f, 0.7f, 1f, 0.8f);

        float originalAnimSpeed = animator != null ? animator.speed : 1f;
        if (animator != null)
            animator.speed = 0f;

        yield return new WaitForSeconds(freezeDuration);

        // Restore everything
        playerMovement.isFrozen = false;
        if (renderer != null)
            renderer.color = Color.white;  // Or cache old color
        if (animator != null)
            animator.speed = originalAnimSpeed;
    }

    private IEnumerator FlashPlayer(GameObject player)
    {
        SpriteRenderer renderer = player.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            Color originalColor = renderer.color;
            renderer.color = new Color(0.7f, 0.9f, 1f); // Blueish tint
            yield return new WaitForSeconds(0.1f);
            renderer.color = originalColor;
        }
    }
}