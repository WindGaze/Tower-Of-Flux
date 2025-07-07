using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionDelayed : MonoBehaviour
{
    [Tooltip("Enter the name of the next scene to load")]
    [SerializeField] private string nextSceneName;

    [Tooltip("Delay in seconds before loading the next scene")]
    [SerializeField] private float delayBeforeLoad = 2f; // Optional delay

    private bool isTriggered = false; // Track if the player has triggered the transition
    private float timer = 0f; // Timer for the delay

    private void Awake()
    {
        // Ensure the script starts disabled
        enabled = false;
        Debug.Log("SceneTransitionDelayed Awake called. Enabled: " + enabled);
    }

    private void Update()
    {
        // If the player has triggered the transition, start the delay timer
        if (isTriggered)
        {
            timer += Time.deltaTime;

            // After the delay, load the next scene
            if (timer >= delayBeforeLoad)
            {
                LoadNextScene();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player trigger detected, preparing to load next scene");
            isTriggered = true; // Set the trigger flag
            enabled = true; // Enable the script to start the Update loop
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Player collision detected, preparing to load next scene");
            isTriggered = true; // Set the trigger flag
            enabled = true; // Enable the script to start the Update loop
        }
    }

    private void LoadNextScene()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            Debug.Log("Loading next scene: " + nextSceneName);
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogError("No next scene specified on " + gameObject.name);
        }
    }
}