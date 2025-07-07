using UnityEngine;

public class HpPotion : MonoBehaviour
{
    public int healAmount = 30; // Adjust this in Inspector

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if colliding with player
        if (other.CompareTag("Player"))
        {
            // Get player's health component
            PlayerMovement player = other.GetComponent<PlayerMovement>();
            if (player != null)
            {
                // Heal player (without exceeding max HP)
                player.health = Mathf.Min(player.health + healAmount, player.maxHealth);
            }

            // Destroy the potion
            Destroy(gameObject);
        }
    }
}