using UnityEngine;
using UnityEngine.UI;

public class UpgradeShop : MonoBehaviour
{
    public int baseGemPrice = 10;
    public int priceIncreasePerLevel = 5;
    public string weaponName;
    public int maxLevel = 4;

    public Text gemPriceTextUI; // Only this text will show the gem price

    private Button upgradeButton;
    private int weaponIndex;
    private bool lastUnlockedStatus;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = gameObject.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    private void Start()
    {
        weaponIndex = GameManager.Instance.weapons.FindIndex(w => w.name == weaponName);
        if (weaponIndex == -1)
        {
            Debug.LogError($"Weapon {weaponName} not found in GameManager!");
            return;
        }

        upgradeButton = GetComponentInChildren<Button>();
        if (upgradeButton != null)
        {
            upgradeButton.onClick.AddListener(OnUpgradeButtonClick);
            UpdateButtonState();
        }
        else
        {
            Debug.LogError("Upgrade button not found in UpgradeShop.");
        }

        lastUnlockedStatus = GameManager.Instance.weapons[weaponIndex].isUnlocked;
        UpdateUIVisibility(lastUnlockedStatus);
    }

    private void Update()
    {
        if (weaponIndex != -1)
        {
            var weapon = GameManager.Instance.weapons[weaponIndex];
            bool currentUnlockedStatus = weapon.isUnlocked;

            if (currentUnlockedStatus != lastUnlockedStatus)
            {
                UpdateUIVisibility(currentUnlockedStatus);
                lastUnlockedStatus = currentUnlockedStatus;
            }

            UpdateButtonState();
        }
    }

    private void UpdateUIVisibility(bool isUnlocked)
    {
        canvasGroup.alpha = isUnlocked ? 1f : 0f;
        canvasGroup.interactable = isUnlocked;
        canvasGroup.blocksRaycasts = isUnlocked;
    }

    private int CalculateCurrentPrice()
    {
        int currentLevel = GameManager.Instance.weapons[weaponIndex].level;
        return baseGemPrice + (priceIncreasePerLevel * currentLevel);
    }

    private void OnUpgradeButtonClick()
    {
        int currentPrice = CalculateCurrentPrice();
        int currentLevel = GameManager.Instance.weapons[weaponIndex].level;

        if (currentLevel >= maxLevel)
        {
            UpdateUIVisibility(false);
            if (gemPriceTextUI != null)
                gemPriceTextUI.gameObject.SetActive(false);
            return;
        }

        if (GameManager.Instance.playerGems >= currentPrice)
        {
            GameManager.Instance.playerGems -= currentPrice;
            GameManager.Instance.UpgradeWeapon(weaponName);
            UpdateButtonState();
        }
        else
        {
            Debug.Log($"Not enough gems to upgrade {weaponName}. Need {currentPrice} gems.");
        }
    }

    private void UpdateButtonState()
    {
        if (weaponIndex == -1 || upgradeButton == null) return;

        int currentLevel = GameManager.Instance.weapons[weaponIndex].level;

        if (currentLevel >= maxLevel)
        {
            if (gemPriceTextUI != null)
                gemPriceTextUI.gameObject.SetActive(false);
            upgradeButton.interactable = false;
        }
        else
        {
            int currentPrice = CalculateCurrentPrice();

            if (gemPriceTextUI != null)
            {
                gemPriceTextUI.text = currentPrice.ToString();
                gemPriceTextUI.gameObject.SetActive(true);
            }

            upgradeButton.interactable = GameManager.Instance.playerGems >= currentPrice;
        }
    }
}