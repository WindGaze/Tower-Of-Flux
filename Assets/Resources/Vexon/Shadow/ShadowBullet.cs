using System.Collections;
using UnityEngine;

public class ShadowBullet : MonoBehaviour
{
    public int damage = 15;
    public float speed = 10f;
    public float searchRadius = 20f;
    public float delayBeforeHoming = 2f;

    private Transform targetEnemy = null;
    private Rigidbody2D rb;
    private Collider2D bulletCollider;
    private bool isTracking = false;
    private int bonusDamage = 0;
    private Vector2 initialDirection;

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
        // Move in the initial direction before homing kicks in
        rb.velocity = initialDirection.normalized * speed;

        // Rotate to face initial direction
        float angle = Mathf.Atan2(initialDirection.y, initialDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        StartCoroutine(StartHomingAfterDelay());
    }

    private IEnumerator StartHomingAfterDelay()
    {
        yield return new WaitForSeconds(delayBeforeHoming);
        FindClosestEnemy();
        isTracking = true;
        bulletCollider.enabled = true;
    }

    private void FixedUpdate()
    {
        if (isTracking && targetEnemy != null)
        {
            Vector2 direction = (targetEnemy.position - transform.position).normalized;
            rb.velocity = direction * speed;

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
        else if (isTracking)
        {
            FindClosestEnemy();
        }
    }

    private void FindClosestEnemy()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, searchRadius);
        float closestEnemyDistance = float.MaxValue;
        float closestBossDistance = float.MaxValue;
        Transform closestEnemy = null;
        Transform closestBoss = null;

        foreach (Collider2D collider in colliders)
        {
            float distance = Vector2.Distance(transform.position, collider.transform.position);

            if (collider.CompareTag("Enemy"))
            {
                if (distance < closestEnemyDistance)
                {
                    closestEnemyDistance = distance;
                    closestEnemy = collider.transform;
                }
            }
            else if (collider.CompareTag("Boss"))
            {
                if (distance < closestBossDistance)
                {
                    closestBossDistance = distance;
                    closestBoss = collider.transform;
                }
            }
        }

        targetEnemy = closestEnemy != null ? closestEnemy : closestBoss;

        if (targetEnemy != null)
        {
            Debug.DrawLine(transform.position, targetEnemy.position,
                targetEnemy.CompareTag("Boss") ? Color.red : Color.green, 0.5f);
        }
    }

    public void Initialize(Vector2 direction, float bulletSpeed, int level = 1)
    {
        speed = bulletSpeed;
        initialDirection = direction.normalized;
        bonusDamage = (level > 1) ? (level - 1) * 2 : 0;
        damage += bonusDamage;

        // Rotation handled in Start using the saved initialDirection
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Boss"))
        {
            damage = 0;
        }

        if (collision.gameObject.CompareTag("Enemy") || collision.gameObject.CompareTag("Boss"))
        {
            Destroy(gameObject);
        }

        if (collision.gameObject.CompareTag("Bullet") ||
            collision.gameObject.CompareTag("Player") ||
            collision.gameObject.CompareTag("EnemyBullet"))
        {
            return;
        }
    }
}
