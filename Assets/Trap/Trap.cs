using UnityEngine;

public class Trap : MonoBehaviour
{
    public int damage = 10;
    private Collider2D trapCollider;
    private AudioSource audioSource;

    private void Awake()
    {
        trapCollider = GetComponent<Collider2D>();
        audioSource = GetComponent<AudioSource>();
        if (trapCollider == null)
        {
            Debug.LogError("No Collider2D found on the trap object!");
        }
        else
        {
            trapCollider.isTrigger = false; // Regular collision
        }
        if (audioSource == null)
        {
            Debug.LogError("No AudioSource found on the trap object!");
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Ignore certain tags
        if (collision.gameObject.CompareTag("Enemy") || 
            collision.gameObject.CompareTag("EnemyBullet") || 
            collision.gameObject.CompareTag("Bullet"))
        {
            Physics2D.IgnoreCollision(collision.collider, trapCollider);
            return;
        }

        // Play sound when colliding with player
        if (collision.gameObject.CompareTag("Player") && audioSource != null)
        {
            audioSource.Play();
        }
    }

    public int GetDamage()
    {
        return damage;
    }
}