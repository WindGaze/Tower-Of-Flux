using UnityEngine;
using UnityEngine.UI;

public class VexonLevel : MonoBehaviour
{
    [Tooltip("Enter the vexon name to find its level (case-sensitive, must match in GameManager).")]
    public string vexonName;

    public GameManager gameManager; // Optional: Can auto-find

    private Text uiText;

    void Awake()
    {
        uiText = GetComponent<Text>();
        if (gameManager == null)
            gameManager = FindObjectOfType<GameManager>();
    }

    void Update()
    {
        if (uiText != null && gameManager != null && !string.IsNullOrEmpty(vexonName))
        {
            int level = gameManager.GetVexonLevel(vexonName);
            uiText.text = $"Level : {level}";
        }
    }
}