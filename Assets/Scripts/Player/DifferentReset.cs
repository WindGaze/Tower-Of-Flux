using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class DifferentReset : MonoBehaviour
{
    [Tooltip("If blank, will reload current scene. Otherwise, loads relevant scene by name.")]
    [SerializeField] private string targetSceneName = "";
    [SerializeField] private GameObject transitionCanvas;
    [SerializeField] private Text transitionText;
    [SerializeField] private float delayBeforeTransition = 2f;
    [SerializeField] private Button activationButton;

    [Header("Transition Fade")]
    [SerializeField] private float fadeDuration = 1.5f;

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
            Debug.LogWarning("No button assigned to DifferentReset script on " + gameObject.name);
        }

        if (transitionCanvas != null)
        {
            canvasGroup = transitionCanvas.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = transitionCanvas.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
            transitionCanvas.SetActive(false);
        }

        // Try to find PlayerMovement at start
        playerMovement = FindObjectOfType<PlayerMovement>();
        if (playerMovement == null)
        {
            Debug.LogWarning("PlayerMovement not found by DifferentReset.");
        }
    }

    public void OnButtonClick()
    {
        // Reset player stats if found
        ResetPlayerStat();

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
            // Subscribe for post-load reset
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
            Time.timeScale = 1f;
            SceneManager.LoadScene(dest);
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
            SceneManager.LoadScene(dest);
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
    /// Finds and resets the player's stat if possible.
    /// </summary>
    private void ResetPlayerStat()
    {
        // Always try to re-find in case player objects reload
        playerMovement = FindObjectOfType<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.ResetPlayerState();
            Debug.Log("Player stats reset via DifferentReset.");
        }
        else
        {
            Debug.LogWarning("No PlayerMovement found when trying to reset!");
        }
    }

    /// <summary>
    /// Optionally reset player again after scene load (covers all cases).
    /// </summary>
    private void OnAfterSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ResetPlayerStat();
        SceneManager.sceneLoaded -= OnAfterSceneLoaded;
    }

    private void OnDestroy()
    {
        if (activationButton != null)
            activationButton.onClick.RemoveListener(OnButtonClick);
        SceneManager.sceneLoaded -= OnAfterSceneLoaded;
    }

    public void CloseAssignedCanvas(GameObject canvasToClose)
    {
        if (canvasToClose != null)
        {
            CanvasGroup cg = canvasToClose.GetComponent<CanvasGroup>();
            canvasToClose.SetActive(false);
            if (cg != null)
            {
                cg.alpha = 0f;
                cg.interactable = false;
                cg.blocksRaycasts = false;
            }
            Debug.Log($"{canvasToClose.name} closed by user.");
        }
    }
    private IEnumerator AutoCloseTransitionCanvas(float delay = 2f)
    {
        yield return new WaitForSecondsRealtime(delay);
        CloseAssignedCanvas(transitionCanvas);
    }
}