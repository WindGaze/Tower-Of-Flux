using UnityEngine;
using UnityEngine.UI;

public class SpeedLevelDisplay : MonoBehaviour
{
    public Text levelText;

    private void OnEnable()
    {
        // Subscribe when enabled
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnSpeedLevelChanged += UpdateSpeedText;
            UpdateSpeedText(GameManager.Instance.speedLevel); // Initial update
        }
    }

    private void OnDisable()
    {
        // Unsubscribe when disabled
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnSpeedLevelChanged -= UpdateSpeedText;
        }
    }

    private void UpdateSpeedText(int newLevel)
    {
        if (levelText != null)
        {
            levelText.text = "Speed Level : LV " + newLevel + "/5";
        }
    }
}