using UnityEngine;

public class GolemBullet : MonoBehaviour
{
    public float lifetime = 5f;    // How long the bullet lasts before being destroyed
    public int damage = 10;        // Bullet damage
    private Vector2 direction;     // Direction the bullet travels in
    private float speed;           // Speed of the bullet
    private Rigidbody2D rb;        // Rigidbody2D for movement
    private int bonusDamage = 0;   // Additional damage based on the level of the Golem

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
            // Update bullet velocity in the set direction
            rb.velocity = direction * speed;
        }
    }

    // Initialize the bullet with direction, speed, and level-based damage
    public void Initialize(Vector2 dir, float spd, int level = 1)
    {
        direction = dir.normalized;   // Normalize direction to ensure consistent speed
        speed = spd;                  // Set the bullet speed
        bonusDamage = (level > 1) ? (level - 1) * 2 : 0;  // Calculate bonus damage based on Golem's level
        damage += bonusDamage;        // Apply bonus damage
    }

    // Use OnTriggerEnter2D instead of OnCollisionEnter2D
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Bullet triggered with: " + collision.gameObject.name);

        // Only react to Enemy or Boss tags
        if (!collision.CompareTag("Enemy") && !collision.CompareTag("Boss"))
        {
            return;
        }

        // Try to call TakeDamage(int) on any attached script (works for both enemies and bosses)
        MonoBehaviour[] components = collision.GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour component in components)
        {
            System.Reflection.MethodInfo takeDamageMethod = component.GetType().GetMethod("TakeDamage",
                new System.Type[] { typeof(int) });

            if (takeDamageMethod != null)
            {
                takeDamageMethod.Invoke(component, new object[] { damage });
                break; // Use the first TakeDamage found
            }
        }

        // Never destroy the bullet
    }
}