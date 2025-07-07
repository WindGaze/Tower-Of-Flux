using UnityEngine;

public class Event5 : MonoBehaviour
{
    public int bonusDamageAmount = 1;
    public GameObject speech1;
    public GameObject speech2;
    public Transform spawnPoint;

    private GameObject currentSpeech;
    private bool isPlayerInside = false;
    private bool hasPurchased = false;
    private PlayerMovement player;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) 
        {
            isPlayerInside = true;
            player = other.GetComponent<PlayerMovement>();

            if (currentSpeech != null)
                Destroy(currentSpeech);

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
        if (isPlayerInside && Input.GetKeyDown(KeyCode.E) && !hasPurchased) // Added !hasPurchased check
        {
            if (player != null)
            {
                ApplyBloodMetalEffect();
                hasPurchased = true;

                if (currentSpeech != null)
                    Destroy(currentSpeech);
                
                currentSpeech = Instantiate(speech2, spawnPoint.position, Quaternion.identity);
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