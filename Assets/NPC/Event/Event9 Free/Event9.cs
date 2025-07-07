using UnityEngine;

public class Event9 : MonoBehaviour
{
    public GameObject speech1; // "Need coins?"
    public GameObject speech2; // "Here's 10 coins!"
    public GameObject speech3; // "You're too rich!"
    public Transform spawnPoint;

    private GameObject currentSpeech;
    private bool isPlayerInside = false;
    private bool hasInteracted = false; // Tracks if interaction is complete
    private PlayerMovement player;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !hasInteracted)
        {
            isPlayerInside = true;
            player = other.GetComponent<PlayerMovement>();

            if (currentSpeech != null)
                Destroy(currentSpeech);

            currentSpeech = Instantiate(speech1, spawnPoint.position, Quaternion.identity);
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
        if (isPlayerInside && Input.GetKeyDown(KeyCode.E) && !hasInteracted)
        {
            if (player != null)
            {
                if (player.gold < 10)
                {
                    player.gold += 10;
                    hasInteracted = true;

                    if (currentSpeech != null)
                        Destroy(currentSpeech);
                    
                    currentSpeech = Instantiate(speech2, spawnPoint.position, Quaternion.identity);
                }
                else
                {
                    hasInteracted = true; // Still counts as completed interaction

                    if (currentSpeech != null)
                        Destroy(currentSpeech);
                    
                    currentSpeech = Instantiate(speech3, spawnPoint.position, Quaternion.identity);
                }
            }
        }
    }
}