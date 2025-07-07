using UnityEngine;

public class Event8 : MonoBehaviour
{
    public GameObject speech1; // "Pay 30 coins?"
    public GameObject speech2; // "Coins taken!"
    public GameObject speech3; // "Not enough coins!"
    public Transform spawnPoint;

    private GameObject currentSpeech;
    private bool isPlayerInside = false;
    private bool hasTakenCoins = false;
    private PlayerMovement player;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !hasTakenCoins)
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
        if (isPlayerInside && Input.GetKeyDown(KeyCode.E) && !hasTakenCoins)
        {
            if (player != null)
            {
                if (player.gold >= 30)
                {
                    player.gold -= 30;
                    hasTakenCoins = true;

                    if (currentSpeech != null)
                        Destroy(currentSpeech);
                    
                    currentSpeech = Instantiate(speech2, spawnPoint.position, Quaternion.identity);
                }
                else
                {
                    if (currentSpeech != null)
                        Destroy(currentSpeech);
                    
                    currentSpeech = Instantiate(speech3, spawnPoint.position, Quaternion.identity);
                }
            }
        }
    }
}