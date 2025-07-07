using UnityEngine;

public class Event4 : MonoBehaviour
{
    public int bonusDamageAmount = 1; // Damage boost amount
    public GameObject speech1; // Initial speech ("Pay 45 gold for bonus damage?")
    public GameObject speech2; // Success speech ("Damage boosted!")
    public GameObject speech3; // Fail speech ("Not enough gold!")
    public Transform spawnPoint; // Where speech bubbles appear

    private GameObject currentSpeech;
    private bool isPlayerInside = false;
    private bool hasPurchased = false; // Tracks if player paid gold and got the boost
    private bool eventTriggered = false; // Ensures one-time interaction
    private PlayerMovement player;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) 
        {
            isPlayerInside = true;
            player = other.GetComponent<PlayerMovement>();

            if (currentSpeech != null)
                Destroy(currentSpeech);

            // Show speech2 if already purchased, otherwise speech1
            currentSpeech = Instantiate(hasPurchased ? speech2 : speech1, spawnPoint.position, Quaternion.identity);
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
        if (isPlayerInside && Input.GetKeyDown(KeyCode.E) && !eventTriggered) 
        {
            if (player != null)
            {
                if (player.gold >= 40) // Check gold cost
                {
                    player.gold -= 40;
                    ApplyBloodMetalEffect(); // Grant damage boost
                    hasPurchased = true; // Mark as purchased
                    eventTriggered = true; // Prevent re-triggering

                    if (currentSpeech != null)
                        Destroy(currentSpeech);
                    
                    currentSpeech = Instantiate(speech2, spawnPoint.position, Quaternion.identity);
                }
                else // Not enough gold
                {
                    if (currentSpeech != null)
                        Destroy(currentSpeech);
                    
                    currentSpeech = Instantiate(speech3, spawnPoint.position, Quaternion.identity);
                }
            }
        }
    }

    private void ApplyBloodMetalEffect()
    {
        if (player != null)
        {
            player.bulletFlask = true;
            player.bonusDamage += bonusDamageAmount;
            Debug.Log($"Damage boosted! New bonusDamage: {player.bonusDamage}");
        }
    }
}