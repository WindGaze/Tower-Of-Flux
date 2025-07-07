using UnityEngine;
using UnityEngine.UI;

public class InvisUpgrade : MonoBehaviour
{
    public Button upgradeButton;
    public int gemCostBase = 50;
    public int gemCostIncrease = 10;

    private void Start()
    {
        UpdateButtonInteractability();
    }

    public void UpgradeInvincibility()
    {
        int currentInvincibilityLevel = GameManager.Instance.invincibilityLevel;
        int currentGems = GameManager.Instance.playerGems; // Get gems directly from GameManager

        int upgradeCost = gemCostBase + (currentInvincibilityLevel - 1) * gemCostIncrease;

        Debug.Log("Current Invincibility Level: " + currentInvincibilityLevel);
        Debug.Log("Current Gems: " + currentGems);
        Debug.Log("Upgrade Cost: " + upgradeCost);

        if (currentInvincibilityLevel < 5 && currentGems >= upgradeCost)
        {
            Debug.Log("Upgrading invincibility...");

            // Deduct gems directly from GameManager
            GameManager.Instance.playerGems -= upgradeCost;

            // Increase invincibility level in GameManager
            GameManager.Instance.invincibilityLevel++;

            Debug.Log("New Invincibility Level: " + GameManager.Instance.invincibilityLevel);
            Debug.Log("Remaining Gems: " + GameManager.Instance.playerGems); // Log remaining gems

            SaveSystem.SaveGame();
            UpdateButtonInteractability();
        }
        else if (currentInvincibilityLevel >= 5)
        {
            Debug.Log("Max invincibility level reached.");
            UpdateButtonInteractability();
        }
        else
        {
            Debug.Log("Insufficient gems or max level reached.");
        }
    }

    private void UpdateButtonInteractability()
    {
        int currentInvincibilityLevel = GameManager.Instance.invincibilityLevel;

        if (currentInvincibilityLevel >= 5)
        {
            upgradeButton.interactable = false;
        }
        else
        {
            upgradeButton.interactable = true;
        }
    }
}