using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTransition : MonoBehaviour
{
    [Tooltip("Enter the name of the next scene to load")]
    [SerializeField] private string nextSceneName;
    [SerializeField] private GameObject transitionCanvas; // Assign your canvas in inspector
    [SerializeField] private Text transitionText; // Optional text element
    [SerializeField] private float delayBeforeTransition = 2f;

    private bool isTransitioning = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isTransitioning)
        {
            Debug.Log("Player trigger detected, showing transition");
            StartTransition();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && !isTransitioning)
        {
            Debug.Log("Player collision detected, showing transition");
            StartTransition();
        }
    }

    private void StartTransition()
    {
        isTransitioning = true;
        
        // Show the transition canvas
        if (transitionCanvas != null)
        {
            transitionCanvas.SetActive(true);
            
            // Optional: Set text if available
            if (transitionText != null)
            {
                transitionText.text = "Moving to next area...";
            }
        }
        
        // Start coroutine to load scene after delay
        StartCoroutine(LoadNextSceneAfterDelay());
    }

    private System.Collections.IEnumerator LoadNextSceneAfterDelay()
    {
        yield return new WaitForSeconds(delayBeforeTransition);
        
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogError("No next scene specified on " + gameObject.name);
            // Hide canvas if scene fails to load
            if (transitionCanvas != null)
            {
                transitionCanvas.SetActive(false);
            }
            isTransitioning = false;
        }
    }
}