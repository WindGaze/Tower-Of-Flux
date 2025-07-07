using UnityEngine;

public class Event3 : MonoBehaviour
{
    public GameObject speech1; // Initial speech object
    public GameObject speech2; // Speech after purchasing (gold reduced and HP decreased)
    public GameObject speech3; // Not enough gold speech object
    public Transform spawnPoint; // Designated spawn point

    private GameObject currentSpeech; // Tracks the active speech object
    private bool isPlayerInside = false; // Tracks if the player is inside the collider
    private bool hasPurchased = false; // Tracks if the player has purchased (gold reduced and HP decreased)
    private bool eventTriggered = false; // Ensures the event can only be triggered once
    private PlayerMovement player; // Reference to the PlayerMovement script

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) // Always allow entry
        {
            isPlayerInside = true;
            player = other.GetComponent<PlayerMovement>(); // Get the PlayerMovement script

            if (currentSpeech != null)
            {
                Destroy(currentSpeech);
            }

            // Show speech2 if the player has already purchased, otherwise show speech1
            if (hasPurchased)
            {
                currentSpeech = Instantiate(speech2, spawnPoint.position, Quaternion.identity);
            }
            else
            {
                currentSpeech = Instantiate(speech1, spawnPoint.position, Quaternion.identity);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;

            if (currentSpeech != null)
            {
                Destroy(currentSpeech);
                currentSpeech = null;
            }
        }
    }

    private void Update()
    {
        if (isPlayerInside && Input.GetKeyDown(KeyCode.E) && !eventTriggered) // Ensure event can only be triggered once
        {
            if (player != null)
            {
                if (player.gold >= 40) // If the player has enough gold
                {
                    player.gold -= 40; // Decrease gold by 15
                    player.health = Mathf.Max(player.health - 20, 0); // Decrease HP by 20 (but not below 0)

                    // Switch to speech2 after purchasing
                    if (currentSpeech != null)
                    {
                        Destroy(currentSpeech);
                    }
                    currentSpeech = Instantiate(speech2, spawnPoint.position, Quaternion.identity);
                    hasPurchased = true; // Mark that the player has purchased
                    eventTriggered = true; // Mark the event as triggered
                }
                else // If the player doesn't have enough gold, show speech3
                {
                    if (currentSpeech != null)
                    {
                        Destroy(currentSpeech);
                    }
                    currentSpeech = Instantiate(speech3, spawnPoint.position, Quaternion.identity);
                }
            }
        }
    }
}