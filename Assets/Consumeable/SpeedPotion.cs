using UnityEngine;

public class SpeedPotion : MonoBehaviour
{
    public int speedBoostAmount = 1; // Same as Event2's += 2

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerMovement player = other.GetComponent<PlayerMovement>();
        if (player != null)
        {
            // Exact same speed boost logic as Event2
            player.speedBoost += speedBoostAmount;
            
            Destroy(gameObject); // Destroy on pickup like HpPotion
        }
    }
}