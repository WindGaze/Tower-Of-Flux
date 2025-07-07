using UnityEngine;

public class WinterBullet : MonoBehaviour
{
    public float lifetime = 5f;    // How long the bullet lasts before being destroyed
    public int damage = 10;        // Bullet damage
    private Vector2 direction;     // Direction the bullet travels in
    private float speed;           // Speed of the bullet
    private Rigidbody2D rb;        // Rigidbody2D for movement

    [Header("Collision Settings")]
    public LayerMask enemyBulletLayer; // Set this in inspector to match EnemyBullet layer

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("No Rigidbody2D found on the bullet object!");
        }
        else
        {
            // Set the bullet to use triggers instead of collisions
            rb.isKinematic = false;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            
            // Make sure the Collider2D is set as a trigger
            Collider2D collider = GetComponent<Collider2D>();
            if (collider != null)
            {
                collider.isTrigger = true;
            }
            else
            {
                Debug.LogError("No Collider2D found on the bullet object!");
            }
        }
    }

    private void Start()
    {
        Destroy(gameObject, lifetime);  // Destroy the bullet after its lifetime
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
        // Add level-based modifications here if needed
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Bullet triggered with: " + collision.gameObject.name);

        // Destroy enemy bullet if layer matches
        if (enemyBulletLayer == (enemyBulletLayer | (1 << collision.gameObject.layer)))
        {
            Destroy(collision.gameObject);
            return;
        }

        // Handle Enemy or Boss collision
        if (collision.CompareTag("Enemy") || collision.CompareTag("Boss"))
        {
            // Try Maggot shortcut
            Maggot maggot = collision.GetComponent<Maggot>();
            if (maggot != null)
            {
                maggot.TakeDamage(damage);
            }
            else
            {
                // Try all scripts for TakeDamage(int)
                MonoBehaviour[] components = collision.GetComponents<MonoBehaviour>();
                foreach (MonoBehaviour component in components)
                {
                    System.Reflection.MethodInfo takeDamageMethod = component.GetType().GetMethod("TakeDamage", 
                        new System.Type[] { typeof(int) });

                    if (takeDamageMethod != null)
                    {
                        takeDamageMethod.Invoke(component, new object[] { damage });
                        break;
                    }
                }
            }
        }
        // Do not destroy this bullet (except by its own lifetime)
    }
}