using UnityEngine;
using System.Collections;

public class LuckyBullet : MonoBehaviour
{
    public int damage = 15;
    public float speed = 10f;
    public float searchRadius = 20f;
    public float delayBeforeHoming = 2f;
    
    private Transform target = null;
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
    }
    
    private IEnumerator StartHomingAfterDelay()
    {
        yield return new WaitForSeconds(delayBeforeHoming);
        
        FindClosestTarget();
        isTracking = true;
        bulletCollider.enabled = true;
    }
    
    private void FixedUpdate()
    {
        if (isTracking && target != null)
        {
            Vector2 direction = (target.position - transform.position).normalized;
            rb.velocity = direction * speed;
            
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
        else if (isTracking)
        {
            // Target was destroyed or missing, find new one
            FindClosestTarget();
        }
    }
    
    private void FindClosestTarget()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, searchRadius);
        float closestDistance = float.MaxValue;
        Transform closestTarget = null;
        
        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag("Enemy") || collider.CompareTag("Boss"))
            {
                float distance = Vector2.Distance(transform.position, collider.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestTarget = collider.transform;
                }
            }
        }
        
        target = closestTarget;
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
        Debug.Log("Homing bullet collided with: " + collision.gameObject.name);
        
        if (collision.gameObject.CompareTag("Enemy") || collision.gameObject.CompareTag("Boss"))
        {
            // Apply damage via TakeDamage(int) if available
            MonoBehaviour[] components = collision.gameObject.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour component in components)
            {
                var takeDamageMethod = component.GetType().GetMethod("TakeDamage", new System.Type[] { typeof(int) });
                if (takeDamageMethod != null)
                {
                    takeDamageMethod.Invoke(component, new object[] { damage });
                    break;
                }
            }
            // Destroy this bullet on enemy/boss contact
            Destroy(gameObject);
            return;
        }
        
        // Ignore collision with player bullets and player
        if (collision.gameObject.CompareTag("Bullet") ||
            collision.gameObject.CompareTag("Player") ||
            collision.gameObject.CompareTag("EnemyBullet"))
        {
            return;
        }
    }
}