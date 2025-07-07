using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class RespiteButton : MonoBehaviour
{
    [Tooltip("Enter the name of the next scene to load")]
    [SerializeField] private string nextSceneName;
    [SerializeField] private GameObject transitionCanvas; // Assign your canvas in inspector
    [SerializeField] private Text transitionText; // Optional text element
    [SerializeField] private float delayBeforeTransition = 2f;
    [SerializeField] private Button activationButton; // The button that triggers the transition

    [Header("Gem Reward Settings")]
    [SerializeField] private int gemMultiplier = 4;
    [SerializeField] private Text rewardText;
    
    [Header("Transition Fade")]
    [SerializeField] private float fadeDuration = 1.5f; // Duration for fade in seconds

    private bool isTransitioning = false;
    private CanvasGroup canvasGroup; // For fading

    private void Start()
    {
        if (activationButton != null)
        {
            activationButton.onClick.AddListener(OnButtonClick);
        }
        else
        {
            Debug.LogWarning("No button assigned to RespiteButton script on " + gameObject.name);
        }

        // Try to get CanvasGroup for fading
        if (transitionCanvas != null)
        {
            canvasGroup = transitionCanvas.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = transitionCanvas.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f; // Ensure invisible at start
            transitionCanvas.SetActive(false); // Start disabled
        }
    }
    
    public void OnButtonClick()
    {
        AwardGemsFromMarkers();
        StartTransition();
    }
    
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

    public void StartTransition()
    {
        if (!isTransitioning)
        {
            Debug.Log("Button clicked, showing transition with fade");
            isTransitioning = true;

            // Fade-in transition
            if (transitionCanvas != null && canvasGroup != null)
            {
                transitionCanvas.SetActive(true);

                if (transitionText != null)
                    transitionText.text = "Moving to respite area...";

                StartCoroutine(FadeInAndLoad());
            }
            else
            {
                // Fallback: just activate canvas and wait
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

        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogError("No next scene specified on " + gameObject.name);
            if (transitionCanvas != null)
            {
                transitionCanvas.SetActive(false);
                canvasGroup.alpha = 0f;
            }
            isTransitioning = false;
        }
    }

    private IEnumerator LoadNextSceneAfterDelay()
    {
        yield return new WaitForSeconds(delayBeforeTransition);

        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogError("No next scene specified on " + gameObject.name);
            if (transitionCanvas != null)
                transitionCanvas.SetActive(false);
            isTransitioning = false;
        }
    }

    private void OnDestroy()
    {
        if (activationButton != null)
            activationButton.onClick.RemoveListener(OnButtonClick);
    }
}