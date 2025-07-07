using UnityEngine;

public class ShieldPlayer : MonoBehaviour
{
    public int health = 100;
    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
        animator.SetInteger("AnimState", 1);

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.isKinematic = true;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true; // ✅ Enable trigger mode
            col.usedByEffector = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("EnemyBullet") || other.CompareTag("ConfuseBullet"))
        {
            var bullet = other.GetComponent<EnemyBullet>();
            if (bullet != null) TakeDamage(bullet.damage);
            Destroy(other.gameObject);
        }
        else if (other.CompareTag("Enemy") || other.CompareTag("Boss"))
        {
            HandleEnemyTrigger(other);
        }
        else if (other.CompareTag("Trap"))
        {
            var trap = other.GetComponent<Trap>();
            if (trap != null) TakeDamage(trap.damage);
        }
    }

    private void HandleEnemyTrigger(Collider2D other)
    {
        // First check for GrandSlime specifically
        var grandSlime = other.GetComponent<GrandSlime>();
        if (grandSlime != null)
        {
            TakeDamage(grandSlime.damage);
            return;
        }

        // Generic fallback for other enemies
        MonoBehaviour[] scripts = other.GetComponents<MonoBehaviour>();
        foreach (var script in scripts)
        {
            var damageField = script.GetType().GetField("damage");
            if (damageField != null)
            {
                TakeDamage((int)damageField.GetValue(script));
                return;
            }

            var damageProp = script.GetType().GetProperty("damage");
            if (damageProp != null)
            {
                TakeDamage((int)damageProp.GetValue(script, null));
                return;
            }
        }

        Debug.LogWarning($"No damage value found on {other.gameObject.name}");
    }

    public void TakeDamage(int damageAmount)
    {
        health -= damageAmount;
        Debug.Log($"Shield took {damageAmount} damage! Remaining Health: {health}");

        if (health <= 0)
        {
            health = 0;
            animator.SetInteger("AnimState", 2);
            Debug.Log("Shield destroyed");
        }
    }

    public void destroyShield()
    {
        Destroy(gameObject);
        Debug.Log("Shield shattered");
    }
}
