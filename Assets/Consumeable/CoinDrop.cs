using UnityEngine;

public class CoinDrop : MonoBehaviour
{
    public int coinAmount = 10;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerMovement playerMovement = collision.GetComponent<PlayerMovement>();
            if (playerMovement != null)
            {
                playerMovement.gold += coinAmount;
                Debug.Log("CoinDrop collected: Gold increased by " + coinAmount);
            }
            else
            {
                Debug.LogWarning("PlayerMovement component not found on Player.");
            }

            Destroy(gameObject);
        }
    }
}