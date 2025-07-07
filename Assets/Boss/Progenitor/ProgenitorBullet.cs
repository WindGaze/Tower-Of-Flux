using UnityEngine;
using System.Collections;

public class ProgenitorBullet : MonoBehaviour
{
    public int damage = 15;
    public float speed = 10f;
    public float searchRadius = 20f;
    public float delayBeforeHoming = 2f;
    public float lifetime = 5f; // <--- Add this line

    private Transform targetPlayer = null;
    private Rigidbody2D rb;
    private Collider2D bulletCollider;
    private bool isTracking = false;
    private int bonusDamage = 0;

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

        bulletCollider.enabled = false;
    }

    private void Start()
    {
        StartCoroutine(StartHomingAfterDelay());

        Destroy(gameObject, lifetime); // <--- Add this line
    }

    private IEnumerator StartHomingAfterDelay()
    {
        yield return new WaitForSeconds(delayBeforeHoming);

        FindClosestPlayer();
        isTracking = true;

        bulletCollider.enabled = true;
    }

    private void FixedUpdate()
    {
        if (targetPlayer != null)
        {
            Vector2 direction = (targetPlayer.position - transform.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

            if (isTracking)
            {
                rb.velocity = direction * speed;
            }
        }
        else if (isTracking)
        {
            FindClosestPlayer();
        }
    }

    private void FindClosestPlayer()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, searchRadius);
        float closestDistance = float.MaxValue;
        Transform closestPlayer = null;

        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag("Player"))
            {
                float distance = Vector2.Distance(transform.position, collider.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPlayer = collider.transform;
                }
            }
        }

        targetPlayer = closestPlayer;
    }

    public void Initialize(Vector2 initialDirection, float bulletSpeed, int level = 1)
    {
        speed = bulletSpeed;
        bonusDamage = (level > 1) ? (level - 1) * 2 : 0;
        damage += bonusDamage;

        float angle = Mathf.Atan2(initialDirection.y, initialDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Destroy(gameObject);
        }

        // Ignore collision with player bullets and player
        if (collision.gameObject.CompareTag("Bullet") ||
            collision.gameObject.CompareTag("Enemy") ||
            collision.gameObject.CompareTag("Boss") ||
            collision.gameObject.CompareTag("EnemyBullet"))
        {
            return;
        }
    }
}