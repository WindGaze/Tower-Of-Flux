using UnityEngine;
using UnityEngine.UI;

public class PlayerGemsDisplay : MonoBehaviour
{
    public Text gemsText; // Assign this in the Inspector

    void Start()
    {
        UpdateGemsDisplay();
    }

    void Update()
    {
        UpdateGemsDisplay();
    }

    void UpdateGemsDisplay()
    {
        if (gemsText != null && GameManager.Instance != null)
        {
            gemsText.text = GameManager.Instance.playerGems.ToString();
        }
    }
}