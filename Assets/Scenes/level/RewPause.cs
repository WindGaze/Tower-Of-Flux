using UnityEngine;

public class RewPause : MonoBehaviour
{
    public GameObject pauseCanvas; // Assign your pause Canvas in Inspector

    private PlayerMovement playerMovementScript;
    private GameObject gunHolderObject;
    private bool wasPaused = false;

    private void Start()
    {
        // Cache references
        playerMovementScript = FindObjectOfType<PlayerMovement>();
        GunHolder gunHolderScript = FindObjectOfType<GunHolder>();
        if (gunHolderScript != null)
        {
            gunHolderObject = gunHolderScript.gameObject;
        }
        
        // Initialize pause state
        wasPaused = pauseCanvas != null && pauseCanvas.activeInHierarchy;
        UpdatePauseState(wasPaused);
    }

    private void Update()
    {
        if (pauseCanvas == null) return;

        bool isPaused = pauseCanvas.activeInHierarchy;

        // Only execute when pause state changes
        if (isPaused != wasPaused)
        {
            UpdatePauseState(isPaused);
            wasPaused = isPaused;
        }
    }

    private void UpdatePauseState(bool paused)
    {
        if (paused)
        {
            PauseGame();
        }
        else
        {
            UnpauseGame();
        }
    }

    private void PauseGame()
    {
        // Disable player movement
        if (playerMovementScript != null)
        {
            playerMovementScript.enabled = false;
            
            // Reset animator to idle state when pausing
            Animator animator = playerMovementScript.GetComponent<Animator>();
            if (animator != null)
            {
                animator.SetInteger("AnimState", 0); // Set to idle
                animator.Update(0); // Force immediate update
            }
        }

        // Disable gun holder
        if (gunHolderObject != null)
        {
            gunHolderObject.SetActive(false);
        }
        
        // Optional: Pause audio
        AudioListener.pause = true;
    }

    private void UnpauseGame()
    {
        // Enable player movement
        if (playerMovementScript != null)
        {
            playerMovementScript.enabled = true;
            
            // Reset animator parameters if needed
            Animator animator = playerMovementScript.GetComponent<Animator>();
            if (animator != null)
            {
                animator.SetTrigger("Reset");
                animator.Update(0);
            }
        }

        // Enable gun holder
        if (gunHolderObject != null)
        {
            gunHolderObject.SetActive(true);
        }
        
        // Optional: Unpause audio
        AudioListener.pause = false;
    }

    // Removed the OnEnable() method that was causing issues
}