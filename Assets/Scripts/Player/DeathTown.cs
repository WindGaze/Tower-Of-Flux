using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class DeathTown : MonoBehaviour
{
    [Tooltip("If blank, will reload current scene. Otherwise, loads relevant scene by name.")]
    [SerializeField] private string targetSceneName = "";
    [SerializeField] private GameObject transitionCanvas;
    [SerializeField] private Text transitionText;
    [SerializeField] private float delayBeforeTransition = 2f;
    [SerializeField] private Button activationButton;

    [Header("Death Canvas Settings")]
    [SerializeField] private GameObject deathCanvas; // Reference to your Death Canvas
    [SerializeField] private float closeCanvasDelay = 1f; // How long to wait after scene load to close

    [Header("Gem Reward Settings")]
    [SerializeField] private int gemMultiplier = 4;
    [SerializeField] private Text rewardText;

    [Header("Transition Fade")]
    [SerializeField] private float fadeDuration = 1.5f;

    [Header("Additional Activation")]
    [SerializeField] private GameObject objectToActivate; // Assign your extra object here

    private bool isTransitioning = false;
    private CanvasGroup canvasGroup;

    // Reference to PlayerMovement (or your player stats script)
    private PlayerMovement playerMovement;

    private void Start()
    {
        if (activationButton != null)
        {
            activationButton.onClick.AddListener(OnButtonClick);
        }
        else
        {
            Debug.LogWarning("No button assigned to DeathTown script on " + gameObject.name);
        }

        if (transitionCanvas != null)
        {
            canvasGroup = transitionCanvas.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = transitionCanvas.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
            transitionCanvas.SetActive(false);
        }

        // Try to find PlayerMovement once at start
        playerMovement = FindObjectOfType<PlayerMovement>();
        if (playerMovement == null)
        {
            Debug.LogWarning("PlayerMovement not found by DeathTown.");
        }
    }

    public void OnButtonClick()
    {
        // Increase gems
        AwardGemsFromMarkers();

        // Reset player stats and weapons if found
        ResetPlayerStat();

        // Use inspector value or passed-in
        ResetToScene(targetSceneName);
    }

    /// <summary>
    /// Call this function to reset/transition. If sceneName is empty or null, reloads current scene.
    /// </summary>
    public void ResetToScene(string sceneName)
    {
        if (isTransitioning) return;
        targetSceneName = sceneName;
        StartTransition();
    }

    public void StartTransition()
    {
        if (!isTransitioning)
        {
            isTransitioning = true;
            if (transitionCanvas != null && canvasGroup != null)
            {
                transitionCanvas.SetActive(true);

                if (transitionText != null)
                    transitionText.text = string.IsNullOrEmpty(targetSceneName) ?
                        "Reloading the scene..." :
                        "Loading " + targetSceneName + "...";

                StartCoroutine(FadeInAndLoad());
            }
            else
            {
                if (transitionCanvas != null) transitionCanvas.SetActive(true);
                StartCoroutine(LoadNextSceneAfterDelay());
            }
            // Optional: subscribe to sceneLoaded for post-load reset
            SceneManager.sceneLoaded += OnAfterSceneLoaded;
        }
    }

    private IEnumerator FadeInAndLoad()
    {
        float t = 0f;
        canvasGroup.alpha = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Clamp01(t / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 1f;
        yield return new WaitForSecondsRealtime(delayBeforeTransition);

        string dest = GetDestinationSceneName(targetSceneName);
        if (!string.IsNullOrEmpty(dest))
        {
            Debug.Log(dest == SceneManager.GetActiveScene().name 
                ? $"Reloading the current scene: {dest}"
                : $"Transitioning to scene: {dest}");

            // Store reference to the Death Canvas before loading new scene
            GameObject deathCanvasRef = deathCanvas;

            Time.timeScale = 1f;
            SceneManager.LoadScene(dest);

            // Start coroutine on this persistent object to close canvas after scene load
            StartCoroutine(CloseDeathCanvasAfterDelay(deathCanvasRef));
        }
        else
        {
            Debug.LogError("No target scene specified on " + gameObject.name);
            if (transitionCanvas != null)
            {
                transitionCanvas.SetActive(false);
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = false;
            }
            isTransitioning = false;
            SceneManager.sceneLoaded -= OnAfterSceneLoaded;
        }
    }

    private IEnumerator LoadNextSceneAfterDelay()
    {
        yield return new WaitForSeconds(delayBeforeTransition);
        string dest = GetDestinationSceneName(targetSceneName);

        if (!string.IsNullOrEmpty(dest))
        {
            Debug.Log(dest == SceneManager.GetActiveScene().name 
                ? $"Reloading the current scene: {dest}"
                : $"Transitioning to scene: {dest}");

            // Store reference to the Death Canvas before loading new scene
            GameObject deathCanvasRef = deathCanvas;

            SceneManager.LoadScene(dest);

            // Start coroutine on this persistent object to close canvas after scene load
            StartCoroutine(CloseDeathCanvasAfterDelay(deathCanvasRef));
        }
        else
        {
            Debug.LogError("No target scene specified on " + gameObject.name);
            if (transitionCanvas != null)
                transitionCanvas.SetActive(false);
            isTransitioning = false;
            SceneManager.sceneLoaded -= OnAfterSceneLoaded;
        }
    }

    private IEnumerator CloseDeathCanvasAfterDelay(GameObject canvas)
    {
        if (canvas != null)
        {
            // Wait a bit after scene load before disabling the canvas
            yield return new WaitForSecondsRealtime(closeCanvasDelay);
            canvas.SetActive(false);
            Debug.Log("Death Canvas has been closed after scene transition");

            // Also close the transition canvas if it exists
            if (transitionCanvas != null)
            {
                transitionCanvas.SetActive(false);
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = 0f;
                    canvasGroup.interactable = true;
                    canvasGroup.blocksRaycasts = false;
                }
                Debug.Log("Transition Canvas has been closed after scene transition");
            }

            // Re-enable player movement and reset animation state
            ActivatePlayerAndExtraObject();

            // Reset transitioning state
            isTransitioning = false;
        }
    }

    /// <summary>
    /// Activate the player movement and the assigned extra object.
    /// </summary>
    private void ActivatePlayerAndExtraObject()
    {
        // Player activation
        if (playerMovement != null)
        {
            playerMovement.enabled = true;

            Animator playerAnimator = playerMovement.GetComponent<Animator>();
            if (playerAnimator != null)
            {
                playerAnimator.SetInteger("AnimState", 0);
                Debug.Log("Player AnimState reset to 0");
            }
            Debug.Log("PlayerMovement component re-enabled after scene transition");
        }
        else
        {
            // Try to find player again after scene load
            playerMovement = FindObjectOfType<PlayerMovement>();
            if (playerMovement != null)
            {
                playerMovement.enabled = true;

                Animator playerAnimator = playerMovement.GetComponent<Animator>();
                if (playerAnimator != null)
                {
                    playerAnimator.SetInteger("AnimState", 0);
                    Debug.Log("Player AnimState reset to 0");
                }
                Debug.Log("PlayerMovement found and re-enabled after scene transition");
            }
        }

        // Extra object activation
        if (objectToActivate != null)
        {
            objectToActivate.SetActive(true);
            Debug.Log("Extra object activated after scene transition");
        }
    }

    /// <summary>
    /// If input is null/empty/whitespace, returns current; otherwise returns trimmed.
    /// </summary>
    private string GetDestinationSceneName(string name)
    {
        if (string.IsNullOrEmpty(name) || string.IsNullOrWhiteSpace(name))
        {
            return SceneManager.GetActiveScene().name;
        }
        else
        {
            return name.Trim();
        }
    }

    /// <summary>
    /// Finds and resets the player's stat if possible,
    /// then enables and resets all gun scripts.
    /// </summary>
    private void ResetPlayerStat()
    {
        // Always try to re-find in case player objects reload
        playerMovement = FindObjectOfType<PlayerMovement>();
        if (playerMovement != null)
        {
            // During transition, disable player movement temporarily
            // It will be re-enabled after scene load in CloseDeathCanvasAfterDelay
            playerMovement.enabled = false;

            playerMovement.ResetPlayerState();
            Debug.Log("Player stats reset via DeathTown.");
        }
        else
        {
            Debug.LogWarning("No PlayerMovement found when trying to reset!");
        }

        // --- WEAPON RESET LOGIC ADDED HERE ---
        EnableAndResetAllGunScripts();
    }

    /// <summary>
    /// Enables all gun scripts and calls ResetSpeed() on each.
    /// </summary>
    private void EnableAndResetAllGunScripts()
    {
        // Enable and reset StraightGun
        foreach (var gun in FindObjectsOfType<StraightGun>(true))
        {
            gun.enabled = true;
            gun.ResetSpeed();
        }
        // Enable and reset SingleGun
        foreach (var gun in FindObjectsOfType<SingleGun>(true))
        {
            gun.enabled = true;
            gun.ResetSpeed();
        }
        // Enable and reset SpreadGun
        foreach (var gun in FindObjectsOfType<SpreadGun>(true))
        {
            gun.enabled = true;
            gun.ResetSpeed();
        }
        // Enable and reset AOEGun
        foreach (var gun in FindObjectsOfType<AOEGun>(true))
        {
            gun.enabled = true;
            gun.ResetSpeed();
        }
        Debug.Log("All gun scripts enabled and ResetSpeed() called (DeathTown).");
    }

    /// <summary>
    /// Optionally reset player again after scene load (covers all cases).
    /// </summary>
    private void OnAfterSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Make sure player stats and weapon scripts are reset after scene load
        ResetPlayerStat();

        // We handle unsubscribing here, but we'll let the CloseDeathCanvasAfterDelay coroutine 
        // handle the canvas state to ensure proper timing
        SceneManager.sceneLoaded -= OnAfterSceneLoaded;
    }

    /// <summary>
    /// Calculate and award any gem rewards before transition.
    /// </summary>
    private void AwardGemsFromMarkers()
    {
        if (GameManager.Instance != null)
        {
            int markerCount = GameManager.Instance.markerCollisions;
            int gemsToAward = markerCount * gemMultiplier;
            GameManager.Instance.playerGems += gemsToAward;

            Debug.Log($"Awarded {gemsToAward} gems from {markerCount} marker collisions");

            if (rewardText != null)
            {
                rewardText.text = $"+{gemsToAward} gems!";
            }

            GameManager.Instance.ResetMarkerCollisions();
            SaveSystem.SaveGame();
        }
    }

    private void OnDestroy()
    {
        if (activationButton != null)
            activationButton.onClick.RemoveListener(OnButtonClick);
        SceneManager.sceneLoaded -= OnAfterSceneLoaded;
    }
}