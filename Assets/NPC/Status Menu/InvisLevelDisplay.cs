using UnityEngine;
using UnityEngine.UI;

public class InvisLevelDisplay : MonoBehaviour
{
    public Text levelText; // Assign this in the Inspector to your UI Text element

    void Update()
    {
        if (GameManager.Instance != null && levelText != null)
        {
            // Update the text to show current level in (X)/5 format
            levelText.text = "Invisibility Level : LV " + GameManager.Instance.invincibilityLevel + "/5";
        }
    }
}