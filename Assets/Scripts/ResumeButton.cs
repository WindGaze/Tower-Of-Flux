using UnityEngine;

public class ResumeButton : MonoBehaviour
{
    public GameObject pauseCanvas; // Assign your pause canvas in the Inspector

    public void ResumeGame()
    {
        if (pauseCanvas != null)
            pauseCanvas.SetActive(false);

        Time.timeScale = 1f;
    }
}