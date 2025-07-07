using UnityEngine;
using UnityEngine.UI;

public class PlayerLevelDisplay : MonoBehaviour
{
    public Text levelText;

    private void OnEnable()
    {
        // Subscribe when enabled
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerLevelChanged += UpdateLevelText;
            UpdateLevelText(GameManager.Instance.playerLevel); // Initial update
        }
    }

    private void OnDisable()
    {
        // Unsubscribe when disabled
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerLevelChanged -= UpdateLevelText;
        }
    }

    private void UpdateLevelText(int newLevel)
    {
        if (levelText != null)
        {
            levelText.text = "Max Health : LV " + newLevel + "/5";
        }
    }
}