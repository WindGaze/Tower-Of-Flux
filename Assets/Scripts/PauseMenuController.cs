using UnityEngine;

public class PauseMenuController : MonoBehaviour
{
    public GameObject pauseCanvas; // Assign your canvas in Inspector

    public bool isPaused = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }

        // --- This part checks for external deactivation of the pauseCanvas ---
        if (isPaused && pauseCanvas != null && !pauseCanvas.activeSelf)
        {
            // Canvas got deactivated externally, so clearly unpause
            isPaused = false;
            Time.timeScale = 1f;
        }
    }

    void TogglePause()
    {
        isPaused = !isPaused;

        if (pauseCanvas != null)
            pauseCanvas.SetActive(isPaused);

        if (isPaused)
        {
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = 1f;
        }
    }
}