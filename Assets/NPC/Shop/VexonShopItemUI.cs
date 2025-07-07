using UnityEngine;
using UnityEngine.UI;

public class VexonShopItemUI : MonoBehaviour
{
    public int gemPrice;           // Price in gems
    public string vexonName;       // Vexon name for matching in GameManager
    public Text gemPriceTextUI;    // Optional UI Text to show gem price

    private Button purchaseButton;
    private bool lastUnlockedStatus;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        // Get or add a CanvasGroup component
        canvasGroup = gameObject.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // Setup purchase button
        purchaseButton = GetComponentInChildren<Button>();
        if (purchaseButton == null)
        {
            Debug.LogError("Purchase button not found in VexonShopItemUI.");
        }
    }

    private void Start()
    {
        // Set the gem price text if assigned
        if (gemPriceTextUI != null)
        {
            gemPriceTextUI.text = gemPrice.ToString();
        }

        // Check if the vexon is already unlocked
        int vexonIndex = GameManager.Instance.vexons.FindIndex(v => v.name == vexonName);
        if (vexonIndex != -1)
        {
            lastUnlockedStatus = GameManager.Instance.vexons[vexonIndex].isUnlocked;
            
            // Instead of destroying, we'll hide if already unlocked
            if (lastUnlockedStatus)
            {
                UpdateUIVisibility(false);
            }
            else
            {
                UpdateUIVisibility(true);
                // Setup purchase button listener
                if (purchaseButton != null)
                {
                    purchaseButton.onClick.AddListener(OnPurchaseButtonClick);
                }
            }
        }
        else
        {
            Debug.LogError("Vexon " + vexonName + " not found in GameManager!");
        }
    }
    
    private void Update()
    {
        // Check if vexon unlock status has changed
        int vexonIndex = GameManager.Instance.vexons.FindIndex(v => v.name == vexonName);
        if (vexonIndex != -1)
        {
            bool currentUnlockedStatus = GameManager.Instance.vexons[vexonIndex].isUnlocked;
            
            // Check if unlock status has changed
            if (currentUnlockedStatus != lastUnlockedStatus)
            {
                UpdateUIVisibility(!currentUnlockedStatus); // Show if NOT unlocked
                lastUnlockedStatus = currentUnlockedStatus;
            }
        }
        
        // Check if player has enough gems to purchase
        UpdateButtonState();
    }
    
    private void UpdateUIVisibility(bool isVisible)
    {
        canvasGroup.alpha = isVisible ? 1f : 0f;
        canvasGroup.interactable = isVisible;
        canvasGroup.blocksRaycasts = isVisible;
    }
    
    private void UpdateButtonState()
    {
        if (purchaseButton != null && !lastUnlockedStatus)
        {
            purchaseButton.interactable = GameManager.Instance.playerGems >= gemPrice;
        }
    }

    public void OnPurchaseButtonClick()
    {
        // Use GameManager to check and deduct gems
        if (GameManager.Instance.playerGems >= gemPrice)
        {
            // Find the vexon in GameManager
            int vexonIndex = GameManager.Instance.vexons.FindIndex(v => v.name == vexonName);
            if (vexonIndex != -1)
            {
                if (!GameManager.Instance.vexons[vexonIndex].isUnlocked)
                {
                    // Deduct gems
                    GameManager.Instance.playerGems -= gemPrice;
                    Debug.Log("Purchased " + vexonName + " for " + gemPrice + " gems. Remaining gems: " + GameManager.Instance.playerGems);

                    // Unlock vexon
                    GameManager.Instance.UnlockVexon(vexonName);

                    // Update visibility
                    UpdateUIVisibility(false);
                }
                else
                {
                    Debug.Log(vexonName + " is already unlocked!");
                }
            }
            else
            {
                Debug.LogError("Vexon " + vexonName + " not found in GameManager!");
            }
        }
        else
        {
            Debug.Log("Not enough gems to purchase " + vexonName);
        }
    }
}