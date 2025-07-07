using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnCollision : MonoBehaviour
{
    public float destructionDuration = 2f; // Time period during which collisions will cause destruction

    private bool canDestroy = true; // Determines if objects should be destroyed on collision

    void Start()
    {
        // Disable destruction after the specified duration
        Invoke(nameof(DisableDestruction), destructionDuration);
    }

    private void DisableDestruction()
    {
        canDestroy = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (canDestroy)
        {
            Destroy(other.gameObject); // Destroy the other object that enters the trigger
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (canDestroy)
        {
            Destroy(collision.gameObject); // Destroy the other object that collides
        }
    }
}
