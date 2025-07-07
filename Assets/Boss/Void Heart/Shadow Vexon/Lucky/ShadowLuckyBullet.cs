using UnityEngine;
using System.Collections;

public class ShadowLuckyBullet : MonoBehaviour
{
    public int damage = 15;
    public float speed = 10f;
    public float delayBeforeHoming = 2f;
    public float lifetime = 5f;

    private Transform targetPlayer = null;
    private Rigidbody2D rb;
    private Collider2D bulletCollider;
    private bool isTracking = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        bulletCollider = GetComponent<Collider2D>();

        if (rb == null)
        {
            Debug.LogError("No Rigidbody2D found on the homing bullet object!");
            return;
        }

        if (bulletCollider == null)
        {
            Debug.LogError("No Collider2D found on the homing bullet object!");
            return;
        }

        rb.isKinematic = false;
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        bulletCollider.enabled = false; // Disable collider initially
    }

    private void Start()
    {
        StartCoroutine(StartHomingAfterDelay());
        Destroy(gameObject, lifetime); // Bullet self-destructs after its lifetime
    }

    private IEnumerator StartHomingAfterDelay()
    {
        yield return new WaitForSeconds(delayBeforeHoming);

        FindPlayer();
        isTracking = true;

        bulletCollider.enabled = true;
    }

    private void FixedUpdate()
    {
        if (isTracking && targetPlayer != null)
        {
            Vector2 direction = (targetPlayer.position - transform.position).normalized;
            rb.velocity = direction * speed;

            // Rotate bullet to face direction
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
        else if (isTracking)
        {
            // Player may have been destroyed and re-instantiated.
            FindPlayer();
        }
    }

    private void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            targetPlayer = playerObj.transform;
        }
    }

    public void Initialize(Vector2 initialDirection, float bulletSpeed)
    {
        speed = bulletSpeed;

        // Set initial rotation
        float angle = Mathf.Atan2(initialDirection.y, initialDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // If hit the Player, use EnemyBullet-like logic (call TakeDamage)
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerMovement player = collision.gameObject.GetComponent<PlayerMovement>();
            if (player != null)
            {
                player.TakeDamage(damage);
            }
        }

        // Destroy this bullet on any collision (Player, wall, etc)
        Destroy(gameObject);
    }
}