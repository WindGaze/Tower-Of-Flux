using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoop1 : MonoBehaviour
{
    [Tooltip("Number of times the scene will reload before transitioning to the next scene")]
    public int sceneReloadCount = 0;

    [Tooltip("Maximum number of times the scene should reload before transitioning")]
    [SerializeField] private int maxReloadCount = 4;

    private void Awake()
    {
        // Load the reload count from PlayerPrefs
        sceneReloadCount = PlayerPrefs.GetInt("SceneReloadCount", 0);
        Debug.Log("Loaded reload count: " + sceneReloadCount);
    }

    // Public method to be called by other scripts instead of OnTriggerEnter
    public void TriggerSceneLoop()
    {
        Debug.Log("Scene loop triggered by method call.");
        sceneReloadCount++;

        // Save the reload count to PlayerPrefs
        PlayerPrefs.SetInt("SceneReloadCount", sceneReloadCount);
        PlayerPrefs.Save();

        if (sceneReloadCount >= maxReloadCount)
        {
            // Enable SceneTransitionDelayed
            SceneTransitionDelayed sceneTransitionDelayed = GetComponent<SceneTransitionDelayed>();
            if (sceneTransitionDelayed != null)
            {
                sceneTransitionDelayed.enabled = true;
                Debug.Log("SceneTransitionDelayed enabled.");
            }
            else
            {
                Debug.LogError("SceneTransitionDelayed script not found!");
            }

            // Disable this script
            this.enabled = false;

            // Reset the reload count
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