using UnityEngine;

public class AOEExplosionBullet : MonoBehaviour
{
    public float lifetime = 5f;
    public GameObject explosionPrefab; // The explosion effect prefab with collider
    private Vector2 direction;
    private float speed;
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
            // Set the bullet to non-kinematic and use continuous collision detection
            rb.isKinematic = false;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
    }

    private void Start()
    {
        Destroy(gameObject, lifetime); // Destroy the bullet after its lifetime
    }

    private void FixedUpdate()
    {
        if (rb != null)
        {
            rb.velocity = direction * speed;
        }
    }

    public void Initialize(Vector2 dir, float spd, int level = 1, bool flipX = false)
    {
        direction = dir.normalized;
        speed = spd;
        
        // Apply flip if needed
        if (flipX)
        {
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Bullet collided with: " + collision.gameObject.name);

        if (collision.gameObject.CompareTag("Bullet") || collision.gameObject.CompareTag("Player"))
        {
            return;  // Ignore collision with other bullets and the player
        }

        // Spawn explosion effect at the point of collision
        if (explosionPrefab != null)
        {
            GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            ExplosionEffect explosionEffect = explosion.GetComponent<ExplosionEffect>();
            if (explosionEffect != null)
            {
                // Pass the gun's level to set the explosion damage
                explosionEffect.SetDamage(FindObjectOfType<AOEGun>().level); // Assuming there's only one gun in the scene
            }
        }

        Destroy(gameObject);  // Destroy the bullet after collision
    }
}