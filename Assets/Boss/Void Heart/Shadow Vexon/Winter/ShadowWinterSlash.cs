using UnityEngine;
using System.Collections;

public class ShadowWinterSlash : MonoBehaviour
{
    public float lifetime = 5f;
    public int damage = 10;
    public float slowDuration = 3f;
    public float speedReductionPercent = 0.3f; // 30% reduction
    private Vector2 direction;
    private float speed;
    private Rigidbody2D rb;

    [Header("Collision Settings")]
    public LayerMask enemyBulletLayer;

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
            if (collider != null) collider.isTrigger = true;
            else Debug.LogError("No Collider2D found on the bullet object!");
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

    public void Initialize(Vector2 dir, float spd, int level = 1)
    {
        direction = dir.normalized;
        speed = spd;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Destroy enemy bullet if hit
        if (enemyBulletLayer == (enemyBulletLayer | (1 << collision.gameObject.layer)))
        {
            Destroy(collision.gameObject);
            return;
        }

        // Damage and slow player (phases through, bullet is NOT destroyed)
        if (collision.CompareTag("Player"))
        {
            PlayerMovement playerMovement = collision.GetComponent<PlayerMovement>();
            if (playerMovement != null)
            {
                playerMovement.TakeDamage(damage);

                // Apply or reset slow effect on the player
                SlowEffect existingSlow = playerMovement.GetComponent<SlowEffect>();
                if (existingSlow != null)
                {
                    existingSlow.ResetTimer();
                }
                else
                {
                    SlowEffect newEffect = playerMovement.gameObject.AddComponent<SlowEffect>();
                    newEffect.Initialize(playerMovement, speedReductionPercent, slowDuration);
                }
            }
            return;
        }
        // Do nothing on all other collisions (phases through everything but enemy bullets)
    }
}