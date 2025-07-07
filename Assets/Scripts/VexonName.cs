using UnityEngine;
using UnityEngine.UI;

public class VexonName : MonoBehaviour
{
    [Tooltip("Drag the TextToObjectSpawner here or it will try to auto-find one in the scene.")]
    public TextToObjectSpawner spawner;

    private Text uiText;

    void Awake()
    {
        uiText = GetComponent<Text>();
        if (spawner == null)
        {
            spawner = FindObjectOfType<TextToObjectSpawner>();
            if (spawner == null)
            {
                Debug.LogWarning("VexonName: No TextToObjectSpawner found in scene.");
            }
        }
    }

    void Update()
    {
        if (uiText != null && spawner != null && spawner.objectPrefab != null)
        {
            uiText.text = spawner.objectPrefab.name;
        }
        else if (uiText != null && spawner != null)
        {
            uiText.text = "No prefab assigned.";
        }
    }
}