using UnityEngine;
using UnityEngine.UI;

public class LevelUpgrade : MonoBehaviour
{
    public Button upgradeButton; // Reference to the button
    public Text costText; // Reference to the Text component that will display the cost
    public int gemCostBase = 10; // Base cost for upgrade
    public int gemCostIncrease = 5; // Cost increase per level

    private void Start()
    {
        // Update the button's interactable state when the game starts
        UpdateButtonInteractability();
        UpdateCostText(); // Update the cost text when the game starts
    }

    public void UpgradePlayerLevel()
    {
        int currentLevel = GameManager.Instance.playerLevel;
        int currentGems = GameManager.Instance.playerGems;

        // Calculate the cost for upgrading the level
        int upgradeCost = gemCostBase + (currentLevel - 1) * gemCostIncrease;

        if (currentLevel < 5 && currentGems >= upgradeCost)
        {
            // Deduct gems
            GameManager.Instance.playerGems -= upgradeCost;
            
            // Increase player level
            GameManager.Instance.playerLevel++;

            // Save the game state
            SaveSystem.SaveGame();

            // Update the button interactability and cost text
            UpdateButtonInteractability();
            UpdateCostText();
        }
        else if (currentLevel >= 5)
        {
            // If level 5 is reached, prevent further upgrades and disable button
            UpdateButtonInteractability();
            UpdateCostText();
        }
    }

    // Update the cost text with the current upgrade cost
    private void UpdateCostText()
    {
        int currentLevel = GameManager.Instance.playerLevel;
        
        if (currentLevel >= 5)
        {
            costText.text = "MAX LEVEL";
        }
        else
        {
            int upgradeCost = gemCostBase + (currentLevel - 1) * gemCostIncrease;
            costText.text = "Cost: " + upgradeCost + " Gems";
        }
    }

    // Update the button's interactable state based on player level
    private void UpdateButtonInteractability()
    {
        int currentLevel = GameManager.Instance.playerLevel;

        if (currentLevel >= 5)
        {
            // Disable the button if the player has reached level 5
            upgradeButton.interactable = false;
        }
        else
        {
            // Enable the button if the player is below level 5
            upgradeButton.interactable = true;
        }
    }
}