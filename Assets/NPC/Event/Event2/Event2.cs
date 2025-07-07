using UnityEngine;

public class Event2 : MonoBehaviour
{
    public GameObject speech1; // Default speech
    public GameObject speech2; // Speech after activation
    public GameObject speech3; // Not enough gold speech
    public Transform spawnPoint; // Spawn location for speech objects

    private GameObject currentSpeech; // Tracks the active speech object
    private bool isPlayerInside = false; // Tracks if the player is inside the collider
    private bool hasSwitchedToSpeech2 = false; // Tracks if speech2 has been activated
    private bool speedBoosted = false; // Ensures speed boost only happens once per scene
    private PlayerMovement player; // Reference to the PlayerMovement script

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = true;
            player = other.GetComponent<PlayerMovement>(); // Get PlayerMovement script

            if (currentSpeech != null)
            {
                Destroy(currentSpeech);
            }

            // Show speech2 if speed was boosted before, otherwise speech1
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
        if (isPlayerInside && Input.GetKeyDown(KeyCode.E) && !speedBoosted)
        {
            if (player != null)
            {
                if (player.gold >= 40) // Player has enough gold
                {
                    player.gold -= 40; // Deduct gold
                    player.speedBoost += 2; // Increase speed temporarily

                    // Show speech2 after activation
                    if (currentSpeech != null)
                    {
                        Destroy(currentSpeech);
                    }
                    currentSpeech = Instantiate(speech2, spawnPoint.position, Quaternion.identity);
                    hasSwitchedToSpeech2 = true;
                    speedBoosted = true; // Mark speed boost as applied
                }
                else // Not enough gold → Show speech3
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
