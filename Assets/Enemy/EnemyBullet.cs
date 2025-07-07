using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public float lifetime = 5f;
    public int damage = 10;
    public float speed = 5f;  // Bullet speed

    private Rigidbody2D rb;

    private void Awake()
{
    rb = GetComponent<Rigidbody2D>();
    if (rb == null)
    {
        Debug.LogError("No Rigidbody2D found on the bullet object!");
    }
    else
    {
        rb.isKinematic = true; // Set it to kinematic to avoid external forces.
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.gravityScale = 0; // Disable gravity
    }
}


    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    public void Initialize(Vector2 direction)
    {
        if (rb != null)
        {
            rb.velocity = direction.normalized * speed;
        }
    }

    // Handle collision with player or other objects
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Ignore collisions with enemies, other bullets, and enemy bullets
        if (collision.gameObject.CompareTag("Enemy") || 
            collision.gameObject.CompareTag("EnemyBullet") || 
            collision.gameObject.CompareTag("Bullet"))
        {
            Physics2D.IgnoreCollision(collision.collider, GetComponent<Collider2D>());
        }
        else
        {
            // Destroy the bullet on collision with anything else
            Destroy(gameObject);
        }
    }
}
