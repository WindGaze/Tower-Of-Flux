using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class PlayerRetry : MonoBehaviour
{
    [SerializeField] private GameObject transitionCanvas; // Assign your canvas in inspector
    [SerializeField] private Text transitionText; // Optional text element
    [SerializeField] private float delayBeforeTransition = 2f;
    [SerializeField] private Button activationButton; // The button that triggers the transition

    [Header("Transition Fade")]
    [SerializeField] private float fadeDuration = 1.5f; // Duration for fade

    private bool isTransitioning = false;
    private CanvasGroup canvasGroup; // For fading

    private PlayerMovement playerMovement; // Reference to PlayerMovement

    private void Start()
    {
        if (activationButton != null)
        {
            activationButton.onClick.AddListener(OnButtonClick);
        }
        else
        {
            Debug.LogWarning("No button assigned to PlayerRetry script on " + gameObject.name);
        }

        if (transitionCanvas != null)
        {
            canvasGroup = transitionCanvas.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = transitionCanvas.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f; // Ensure invisible at start
            transitionCanvas.SetActive(false);
        }

        // Find the PlayerMovement component in the scene
        playerMovement = FindObjectOfType<PlayerMovement>();
        if (playerMovement == null)
        {
            Debug.LogWarning("PlayerMovement not found in scene by PlayerRetry.");
        }
    }

    public void OnButtonClick()
    {
        // Reset player stats if found
        if (playerMovement != null)
        {
            playerMovement.ResetPlayerState();
            Debug.Log("Player stats reset via PlayerRetry.");
        }
        StartTransition();
    }

    private void OnAfterSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Optional: Find PlayerMovement again after reload if needed
        playerMovement = FindObjectOfType<PlayerMovement>();
        SceneManager.sceneLoaded -= OnAfterSceneLoaded;
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
                    transitionText.text = "Reloading the scene...";

                StartCoroutine(FadeInAndLoad());
            }
            else
            {
                if (transitionCanvas != null) transitionCanvas.SetActive(true);
                StartCoroutine(LoadNextSceneAfterDelay());
            }
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

        string dest = GetDestinationSceneName();

        if (!string.IsNullOrEmpty(dest))
        {
            Debug.Log("Reloading the current scene: " + dest);

            Time.timeScale = 1f; // <-- THIS LINE

            SceneManager.LoadScene(dest);
        }
        // ...rest of the code
    }

    private IEnumerator LoadNextSceneAfterDelay()
    {
        yield return new WaitForSeconds(delayBeforeTransition);

        string dest = GetDestinationSceneName();

        if (!string.IsNullOrEmpty(dest))
        {
            Debug.Log("Reloading the current scene: " + dest);
            SceneManager.LoadScene(dest);
        }
        else
        {
            Debug.LogError("No current scene specified on " + gameObject.name);
            if (transitionCanvas != null)
                transitionCanvas.SetActive(false);
            isTransitioning = false;
            SceneManager.sceneLoaded -= OnAfterSceneLoaded;
        }
    }

    private string GetDestinationSceneName()
    {
        // Always reload the current scene
        return SceneManager.GetActiveScene().name;
    }

    private void OnDestroy()
    {
        if (activationButton != null)
            activationButton.onClick.RemoveListener(OnButtonClick);
        SceneManager.sceneLoaded -= OnAfterSceneLoaded;
    }
}