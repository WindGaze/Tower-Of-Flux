using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneAutoLoader : MonoBehaviour
{
    [Tooltip("Name of the scene to load after delay")]
    public string nextSceneName = "YourSceneName";
    public float waitSeconds = 1.5f;

    void Start()
    {
        StartCoroutine(LoadSceneAfterDelay());
    }

    private System.Collections.IEnumerator LoadSceneAfterDelay()
    {
        yield return new WaitForSeconds(waitSeconds);
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogError("SceneAutoLoader: No scene name specified!");
        }
    }
}