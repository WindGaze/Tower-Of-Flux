using UnityEngine;

public class GolemShield : MonoBehaviour
{
    public int health = 100;
    private Animator animator;
    private float shieldDuration = 10f;
    private float timer;

    private void Start()
    {
        animator = GetComponent<Animator>();
        animator.SetInteger("AnimState", 1); // Assuming 1 means active state

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.isKinematic = true;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true; // Enable trigger mode
            col.usedByEffector = false;
        }

        timer = shieldDuration;
    }

    private void Update()
    {
        // Countdown timer for shield expiration
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            DestroyShield();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("EnemyBullet"))
        {
            var bullet = other.GetComponent<EnemyBullet>();
            if (bullet != null)
            {
                TakeDamage(bullet.damage);
                Destroy(other.gameObject); // Destroy the bullet on impact
            }
        }
    }

    public void TakeDamage(int damageAmount)
    {
        health -= damageAmount;
        Debug.Log($"GolemShield took {damageAmount} damage! Remaining Health: {health}");

        if (health <= 0)
        {
            health = 0;
            animator.SetInteger("AnimState", 2); // Assuming 2 means destroyed state
            Debug.Log("GolemShield destroyed");
        }
    }

    private void DestroyShield()
    {
        animator.SetInteger("AnimState", 2); // Assuming 2 means destroyed state
        Debug.Log("GolemShield expired");
        Destroy(gameObject);
    }
}
