using UnityEngine;

public class Event1 : MonoBehaviour
{
    public GameObject speech1; // First speech object
    public GameObject speech2; // Second speech object
    public GameObject speech3; // Not enough gold speech object
    public GameObject speech4; // HP full speech object
    public Transform spawnPoint; // Designated spawn point

    private GameObject currentSpeech; // Tracks the active speech object
    private bool isPlayerInside = false; // Tracks if the player is inside the collider
    private bool hasSwitchedToSpeech2 = false; // Tracks if speech2 has been activated
    private bool eventTriggered = false; // Ensures event only completes after HP increase
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

            // Show speech2 if switched before, otherwise speech1
            if (hasSwitchedToSpeech2)
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
        if (isPlayerInside && Input.GetKeyDown(KeyCode.E) && !eventTriggered) // Ensure event only ends after HP increase
        {
            if (player != null)
            {
                if (player.health >= player.maxHealth) // If HP is full, only show speech4
                {
                    if (currentSpeech != null)
                    {
                        Destroy(currentSpeech);
                    }
                    currentSpeech = Instantiate(speech4, spawnPoint.position, Quaternion.identity);
                }
                else if (player.gold >= 40) // If the player has enough gold
                {
                    player.gold -= 40; // Decrease gold by 30
                    player.health = Mathf.Min(player.health + 30, player.maxHealth); // Increase HP but not exceed max

                    // Switch to speech2 if it hasn't already been switched
                    if (!hasSwitchedToSpeech2)
                    {
                        if (currentSpeech != null)
                        {
                            Destroy(currentSpeech);
                        }
                        currentSpeech = Instantiate(speech2, spawnPoint.position, Quaternion.identity);
                        hasSwitchedToSpeech2 = true; // Mark that speech2 has been activated
                    }

                    eventTriggered = true; // Mark event as completed after HP increase
                }
                else // If player doesn't have enough gold, show speech3
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
