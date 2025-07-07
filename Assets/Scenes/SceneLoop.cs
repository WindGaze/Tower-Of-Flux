using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoop : MonoBehaviour
{
    [Tooltip("Number of times the scene will reload before transitioning to the next scene")]
    public int sceneReloadCount = 0;

    [Tooltip("Maximum number of times the scene should reload before transitioning")]
    [SerializeField] private int maxReloadCount = 4;

    private bool hasTriggered = false; // Flag to track if collision already happened

    private void Awake()
    {
        // Load the reload count from PlayerPrefs (or use a static variable)
        sceneReloadCount = PlayerPrefs.GetInt("SceneReloadCount", 0);
        Debug.Log("Loaded reload count: " + sceneReloadCount);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Only trigger once per scene load
        if (collision.CompareTag("Player") && !hasTriggered)
        {
            hasTriggered = true; // Set flag to prevent repeated triggers
            Debug.Log("Player detected. Reloading scene.");
            sceneReloadCount++;

            // Save the reload count to PlayerPrefs (or use a static variable)
            PlayerPrefs.SetInt("SceneReloadCount", sceneReloadCount);
            PlayerPrefs.Save(); // Ensure the data is saved immediately

            if (sceneReloadCount >= maxReloadCount)
            {
                // Enable SceneTransitionDelayed
                SceneTransitionDelayed sceneTransitionDelayed = GetComponent<SceneTransitionDelayed>();
                if (sceneTransitionDelayed != null)
                {
                    sceneTransitionDelayed.enabled = true; // Enable the script
                    Debug.Log("SceneTransitionDelayed enabled.");
                }
                else
                {
                    Debug.LogError("SceneTransitionDelayed script not found!");
                }

                // Disable this script
                this.enabled = false;

                // Reset the reload count for future use
                PlayerPrefs.DeleteKey("SceneReloadCount");
            }
            else
            {
                // Reload the current scene
                Debug.Log("Reloading scene. Count: " + sceneReloadCount);
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }
    }
}