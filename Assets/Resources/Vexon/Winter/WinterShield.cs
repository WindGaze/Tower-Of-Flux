using UnityEngine;

public class WinterShield : MonoBehaviour
{
    private Animator animator;
    private int hitCount = 0;
    private const int maxHits = 3; // Shield breaks after 3 hits

    private void Start()
    {
        animator = GetComponent<Animator>();
        animator.SetInteger("AnimState", 1);

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.isKinematic = true;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
            col.usedByEffector = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("EnemyBullet") || other.CompareTag("ConfuseBullet"))
        {
            HandleHit();
            Destroy(other.gameObject);
        }
        else if (other.CompareTag("Enemy") || other.CompareTag("Boss") || other.CompareTag("Trap"))
        {
            HandleHit();
        }
    }

    private void HandleHit()
    {
        hitCount++;
        Debug.Log($"Shield hit! ({hitCount}/{maxHits})");

        if (hitCount >= maxHits)
        {
            animator.SetInteger("AnimState", 2);
            Debug.Log("Shield destroyed");
            // Destruction will be handled by animation event calling destroyShield()
        }
    }

    public void destroyShield()
    {
        Destroy(gameObject);
        Debug.Log("Shield shattered");
    }
}