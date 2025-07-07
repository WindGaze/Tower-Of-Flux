using UnityEngine;
using UnityEngine.UI;

public class VexonUpgradeShop : MonoBehaviour
{
    public int baseGemPrice = 10;
    public int priceIncreasePerLevel = 5;
    public string vexonName;
    public int maxLevel = 4;
    public Text gemPriceTextUI; // Assign this in inspector to a separate Text element

    private Button upgradeButton;
    private int vexonIndex;
    private bool lastUnlockedStatus;
    private CanvasGroup canvasGroup;
    private Text buttonText;

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
        vexonIndex = GameManager.Instance.vexons.FindIndex(v => v.name == vexonName);
        if (vexonIndex == -1)
        {
            Debug.LogError($"Vexon {vexonName} not found in GameManager!");
            return;
        }

        upgradeButton = GetComponentInChildren<Button>();
        if (upgradeButton != null)
        {
            buttonText = upgradeButton.GetComponentInChildren<Text>();
            upgradeButton.onClick.AddListener(OnUpgradeButtonClick);
            UpdateButtonState();
        }
        else
        {
            Debug.LogError("Upgrade button not found in VexonUpgradeShop.");
        }

        lastUnlockedStatus = GameManager.Instance.vexons[vexonIndex].isUnlocked;
        UpdateUIVisibility(lastUnlockedStatus);
    }

    private void Update()
    {
        if (vexonIndex != -1)
        {
            var vexon = GameManager.Instance.vexons[vexonIndex];
            bool currentUnlockedStatus = vexon.isUnlocked;
            
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
        int currentLevel = GameManager.Instance.vexons[vexonIndex].level;
        return baseGemPrice + (priceIncreasePerLevel * currentLevel);
    }

   private void OnUpgradeButtonClick()
{
    int currentPrice = CalculateCurrentPrice();
    int currentLevel = GameManager.Instance.vexons[vexonIndex].level;

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
        GameManager.Instance.UpgradeVexon(vexonName);
        UpdateButtonState();
    }
    else
    {
        Debug.Log($"Not enough gems to upgrade {vexonName}. Need {currentPrice} gems.");
    }
}

    private void UpdateButtonState()
{
    if (vexonIndex == -1 || upgradeButton == null) return;

    int currentLevel = GameManager.Instance.vexons[vexonIndex].level;

    if (currentLevel >= maxLevel)
    {
        buttonText.text = "Max Level";
        upgradeButton.interactable = false;
        if (gemPriceTextUI != null)
            gemPriceTextUI.gameObject.SetActive(false);
    }
    else
    {
        int currentPrice = CalculateCurrentPrice();

        if (gemPriceTextUI != null)
        {
            gemPriceTextUI.text = $"{currentPrice}";
            gemPriceTextUI.gameObject.SetActive(true);
        }

        upgradeButton.interactable = GameManager.Instance.playerGems >= currentPrice;
    }
}
}