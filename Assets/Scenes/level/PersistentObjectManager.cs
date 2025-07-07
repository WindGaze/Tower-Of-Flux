using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Destroy ALL objects with the "Player" tag (including DontDestroyOnLoad ones)
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            Destroy(player);
            Debug.Log("Destroyed Player object: " + player.name);
        }
    }

    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}