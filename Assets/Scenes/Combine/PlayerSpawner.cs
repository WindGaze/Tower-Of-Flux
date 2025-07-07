using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerSpawner : MonoBehaviour
{
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        
        // When object is re-enabled, find spawn spot and bind player
        GameObject spawnSpot = GameObject.FindWithTag("SpawnSpot");
        if (spawnSpot != null)
        {
            StartCoroutine(BindPlayerToSpawn(spawnSpot.transform.position));
        }
        else
        {
            Debug.LogWarning("No spawn spot found when re-enabling player!");
        }
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        
        // Stop all coroutines when disabled to prevent any ongoing binding
        StopAllCoroutines();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        GameObject spawnSpot = GameObject.FindWithTag("SpawnSpot");

        if (spawnSpot != null)
        {
            StartCoroutine(BindPlayerToSpawn(spawnSpot.transform.position));
        }
        else
        {
            Debug.LogWarning("No spawn spot found in the new scene!");
        }
    }

    private IEnumerator BindPlayerToSpawn(Vector2 spawnPosition)
    {
        float timer = 0f;
        while (timer < 2f) // Keep binding for 2 seconds
        {
            transform.position = spawnPosition; // Force player position
            timer += Time.deltaTime;
            yield return null; // Wait for the next frame
        }
    }
}