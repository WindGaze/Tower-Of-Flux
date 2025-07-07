using UnityEngine;
using UnityEngine.UI;

public class ShopItemUI : MonoBehaviour
{
    public int gemPrice;           // Price in gems
    public string weaponName;      // Weapon name for matching in GameManager
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
            Debug.LogError("Purchase button not found in ShopItemUI.");
        }
    }

    private void Start()
    {
        // Set the gem price text if assigned
        if (gemPriceTextUI != null)
        {
            gemPriceTextUI.text = gemPrice.ToString();
        }

        // Check if the weapon is already unlocked
        int weaponIndex = GameManager.Instance.weapons.FindIndex(w => w.name == weaponName);
        if (weaponIndex != -1)
        {
            lastUnlockedStatus = GameManager.Instance.weapons[weaponIndex].isUnlocked;
            
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
            Debug.LogError("Weapon " + weaponName + " not found in GameManager!");
        }
    }
    
    private void Update()
    {
        // Check if weapon unlock status has changed
        int weaponIndex = GameManager.Instance.weapons.FindIndex(w => w.name == weaponName);
        if (weaponIndex != -1)
        {
            bool currentUnlockedStatus = GameManager.Instance.weapons[weaponIndex].isUnlocked;
            
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
            // Use GameManager.Instance.playerGems instead of PlayerMovement.gem
            purchaseButton.interactable = GameManager.Instance.playerGems >= gemPrice;
        }
    }

    private void OnPurchaseButtonClick()
    {
        // Check if the player has enough gems using GameManager
        if (GameManager.Instance.playerGems >= gemPrice)
        {
            // Find the weapon in GameManager
            int weaponIndex = GameManager.Instance.weapons.FindIndex(w => w.name == weaponName);
            if (weaponIndex != -1)
            {
                if (!GameManager.Instance.weapons[weaponIndex].isUnlocked)
                {
                    // Deduct gems from GameManager
                    GameManager.Instance.playerGems -= gemPrice;
                    Debug.Log("Purchased " + weaponName + " for " + gemPrice + " gems. Remaining gems: " + GameManager.Instance.playerGems);

                    // Unlock weapon
                    GameManager.Instance.UnlockWeapon(weaponName);
                    
                    // Update visibility
                    UpdateUIVisibility(false);
                    
                    // Save the game
                    SaveSystem.SaveGame();
                }
                else
                {
                    Debug.Log(weaponName + " is already unlocked!");
                }
            }
            else
            {
                Debug.LogError("Weapon " + weaponName + " not found in GameManager!");
            }
        }
        else
        {
            Debug.Log("Not enough gems to purchase " + weaponName + ". Need: " + gemPrice + ", Have: " + GameManager.Instance.playerGems);
        }
    }
}