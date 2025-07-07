using UnityEngine;

public class MissileAutomaton : MonoBehaviour
{
    public GameObject explosionPrefab;
    public int missileDamage = 30;
    
    private Vector2 direction;
    private float speed;
    private Rigidbody2D rb;
    public bool destination = false; // If true, missile flies straight to locked position
    private Vector2 targetPosition; // Stores locked position

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("No Rigidbody2D found on the missile object!");
        }
        else
        {
            rb.isKinematic = false;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
    }

    // Sets the target position for non-homing missiles
    public void SetTargetPosition(Vector2 targetPos)
    {
        targetPosition = targetPos;
    }

    // Initialize missile with direction and speed (no spawnPoint needed)
    public void Initialize(Vector2 dir, float spd)
    {
        direction = dir.normalized;
        speed = spd;

        // Rotate missile to face movement direction
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    private void FixedUpdate()
    {
        if (destination)
        {
            // Fly straight to locked position (non-homing)
            Vector2 newPos = Vector2.MoveTowards(
                transform.position,
                targetPosition,
                speed * Time.fixedDeltaTime
            );
            rb.MovePosition(newPos);

            // Destroy if it reaches the target (optional)
            if (Vector2.Distance(transform.position, targetPosition) < 0.1f)
            {
                Explode();
            }
        }
        else
        {
            // Original behavior (if homing was intended)
            rb.velocity = direction * speed;
        }
    }

    private void Explode()
    {
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity)
                .GetComponent<ExplosionEffect>()?.SetDamage(missileDamage);
        }
        Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy") || collision.gameObject.CompareTag("Boss"))
            return; // Ignore collisions with enemies/boss
        
        Explode();
    }

    private void OnBecameInvisible()
    {
        Destroy(gameObject);
    }
}