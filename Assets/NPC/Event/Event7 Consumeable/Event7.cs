using UnityEngine;

public class Event7 : MonoBehaviour
{
    [Header("UI References")]
    public GameObject speechPrompt; 
    public GameObject speechPrompt2;
    public Transform spawnPoint;
    [Tooltip("The canvas that opens when pressing E")]
    public Canvas interactionCanvas; // Assign in Inspector!
    
    [Header("State (Set externally)")]
    public bool isInteracted = false; // Now only read, not modified here
    public bool isPlayerInside = false;
    
    private GameObject currentPrompt;
    private bool previousIsInteracted = false; // NEW: remember last state!

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isInteracted)
        {
            isPlayerInside = true;
            UpdatePrompt();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;
            DestroyCurrentPrompt();
            CloseCanvas(); // Explicitly close canvas
        }
    }

    private void Update()
    {
        // If player is in, the E key opens canvas and updates prompt
        if (isPlayerInside && Input.GetKeyDown(KeyCode.E) && !isInteracted)
        {
            OpenCanvas(); // Explicitly open canvas
            UpdatePrompt();
        }

        // NEW: Detect isInteracted changes and update prompt
        if (isPlayerInside && isInteracted != previousIsInteracted)
        {
            UpdatePrompt();
        }
        previousIsInteracted = isInteracted;
    }

    private void OpenCanvas()
    {
        if (interactionCanvas != null)
        {
            interactionCanvas.gameObject.SetActive(true);
            Debug.Log("Canvas GameObject enabled!");
        }
        else
        {
            Debug.LogWarning("InteractionCanvas not assigned!");
        }
    }

    private void CloseCanvas()
    {
        if (interactionCanvas != null)
            interactionCanvas.enabled = false;
    }

    private void UpdatePrompt()
    {
        DestroyCurrentPrompt();
        GameObject promptToUse = isInteracted ? speechPrompt2 : speechPrompt;
        if (promptToUse != null && spawnPoint != null)
        {
            currentPrompt = Instantiate(promptToUse, spawnPoint.position, Quaternion.identity);
        }
    }

    private void DestroyCurrentPrompt()
    {
        if (currentPrompt != null)
        {
            Destroy(currentPrompt);
            currentPrompt = null;
        }
    }
}