using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerCloneActivator : MonoBehaviour
{
    [Tooltip("Assign your disabled player clone here (optional)")]
    [SerializeField] private GameObject playerClone;
    
    [Tooltip("Tag to search for when checking if a player exists")]
    [SerializeField] private string playerTag = "Player";
    
    private void OnEnable()
    {
        // Subscribe to the scene loaded event
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // Unsubscribe from the scene loaded event
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Activation and player checking logic removed as requested.
        // This method now does nothing.
    }

    // Optional: Show helpful information in the Unity Editor
    private void Reset()
    {
        playerTag = "Player";
    }
}