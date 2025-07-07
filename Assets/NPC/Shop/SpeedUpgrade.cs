using UnityEngine;
using UnityEngine.UI;

public class SpeedUpgrade : MonoBehaviour
{
    public Button upgradeButton; // Reference to the button
    public Text costText; // Reference to the Text component that will display the cost
    public int gemCostBase = 50; // Base cost for speed upgrade
    public int gemCostIncrease = 10; // Cost increase per speed level

    private void Start()
    {
        // Update the button's interactable state and cost text when the game starts
        UpdateButtonState();
    }

    public void UpgradeSpeed()
    {
        int currentSpeedLevel = GameManager.Instance.speedLevel;
        int currentGems = GameManager.Instance.playerGems;
        int upgradeCost = CalculateUpgradeCost();

        if (currentSpeedLevel < 5 && currentGems >= upgradeCost)
        {
            // Deduct gems
            GameManager.Instance.playerGems -= upgradeCost;
            
            // Increase speed level
            GameManager.Instance.speedLevel++;

            // Save the game state
            SaveSystem.SaveGame();

            // Update the button state
            UpdateButtonState();
        }
        else if (currentSpeedLevel >= 5)
        {
            // If level 5 is reached, update the button state
            UpdateButtonState();
        }
    }

    // Update the cost text with the current upgrade cost
    private void UpdateCostText()
    {
        int currentSpeedLevel = GameManager.Instance.speedLevel;
        
        if (currentSpeedLevel >= 5)
        {
            costText.text = "MAX LEVEL";
        }
        else
        {
            costText.text = "Cost: " + CalculateUpgradeCost() + " Gems";
        }
    }

    // Update both button interactability and cost text
    private void UpdateButtonState()
    {
        int currentSpeedLevel = GameManager.Instance.speedLevel;

        // Update button interactability
        upgradeButton.interactable = currentSpeedLevel < 5;
        
        // Update cost text
        UpdateCostText();
    }

    // Calculate the current upgrade cost
    private int CalculateUpgradeCost()
    {
        return gemCostBase + (GameManager.Instance.speedLevel - 1) * gemCostIncrease;
    }
}