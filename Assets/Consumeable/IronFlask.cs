using UnityEngine;

public class IronFlask : MonoBehaviour
{
    public GameObject Shield; // The shield prefab to instantiate
    public int healthBonus = 30; // Health to add if shield exists

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // Check if player already has a shield
        ShieldPlayer existingShield = other.GetComponentInChildren<ShieldPlayer>();
        
        if (existingShield != null)
        {
            // Increase existing shield's health
            existingShield.health += healthBonus;
            Debug.Log($"Increased existing shield health to {existingShield.health}");
        }
        else
        {
            // Spawn new shield if none exists
            Transform playerTransform = other.transform;
            if (Shield != null && playerTransform != null)
            {
                Instantiate(Shield, playerTransform.position, Quaternion.identity, playerTransform);
                Debug.Log("Created new shield");
            }
        }

        Destroy(gameObject); // Destroy the flask after pickup
    }
}