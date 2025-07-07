using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonPressSceneTransition : MonoBehaviour
{
    [Tooltip("Name of the scene to load when the button is pressed")]
    [SerializeField] private string nextSceneName;

    // Called when the button is clicked (assign this in the Inspector)
    public void OnButtonPress()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            Debug.Log("Loading scene: " + nextSceneName);
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogError("Next scene name is not set on " + gameObject.name);
        }
    }
}