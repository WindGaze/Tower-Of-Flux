using UnityEngine;

public class ExitButton : MonoBehaviour
{
    // Call this from your button's OnClick event
    public void ExitGame()
    {
        // If running in Unity Editor
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // If running as a standalone build
        Application.Quit();
#endif
    }
}