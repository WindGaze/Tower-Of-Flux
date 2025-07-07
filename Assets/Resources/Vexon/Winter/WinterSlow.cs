using UnityEngine;

public class WinterSlow : MonoBehaviour
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
            bool slowed = false;
            // Try to call TakeDamage(int) and slow
            MonoBehaviour[] components = collision.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour component in components)
            {
                System.Reflection.MethodInfo takeDamageMethod = component.GetType().GetMethod("TakeDamage", new System.Type[] { typeof(int) });
                if (takeDamageMethod != null)
                {
                    takeDamageMethod.Invoke(component, new object[] { damage });
                    ApplySlowEffect(component);
                    slowed = true;
                    break;
                }
            }
            // Fallback for scripts without TakeDamage(int): still try to slow them
            if (!slowed)
            {
                foreach (MonoBehaviour component in components)
                {
                    ApplySlowEffect(component);
                }
            }
        }
    }

    private void ApplySlowEffect(MonoBehaviour target)
    {
        // Use reflection to find a speed variable
        System.Reflection.FieldInfo speedField = target.GetType().GetField("speed", 
            System.Reflection.BindingFlags.Public | 
            System.Reflection.BindingFlags.NonPublic | 
            System.Reflection.BindingFlags.Instance);
        
        if (speedField != null && speedField.FieldType == typeof(float))
        {
            float currentSpeed = (float)speedField.GetValue(target);
            float slowedSpeed = currentSpeed * 0.2f;
            speedField.SetValue(target, slowedSpeed);
            StartCoroutine(RestoreSpeed(target, speedField, currentSpeed, 5f));
        }
    }

    private System.Collections.IEnumerator RestoreSpeed(MonoBehaviour target, System.Reflection.FieldInfo speedField, float originalSpeed, float duration)
    {
        yield return new WaitForSeconds(duration);

        if (target != null && target.gameObject != null)
        {
            speedField.SetValue(target, originalSpeed);
        }
    }
}