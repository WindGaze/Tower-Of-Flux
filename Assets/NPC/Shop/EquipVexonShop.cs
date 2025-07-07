using UnityEngine;
using UnityEngine.UI;

public class EquipVexonShop : MonoBehaviour
{
    public string vexonName;
    public TextToObjectSpawner textToObjectSpawner;
    private Button equipButton;
    private bool lastUnlockedStatus;
    private Image buttonImage;
    private Text buttonText; // Optional: if you want to modify the button text

    private void Awake()
    {
        // Get the Button component
        equipButton = GetComponent<Button>();
        if (equipButton == null)
        {
            Debug.LogWarning("Button component not found on this GameObject.");
        }

        // Get the Image component for visual feedback
        buttonImage = GetComponent<Image>();
        
        // Optional: Get the Text component if you want to modify the text
        buttonText = GetComponentInChildren<Text>();
    }

    private void Start()
    {
        if (textToObjectSpawner == null)
        {
            textToObjectSpawner = FindObjectOfType<TextToObjectSpawner>();
        }
        
        if (GameManager.Instance != null)
        {
            int vexonIndex = GameManager.Instance.vexons.FindIndex(v => v.name == vexonName);
            if (vexonIndex != -1)
            {
                lastUnlockedStatus = GameManager.Instance.vexons[vexonIndex].isUnlocked;
                UpdateButtonState(lastUnlockedStatus);
            }
        }
    }
    
    private void OnEnable()
    {
        if (textToObjectSpawner == null)
        {
            textToObjectSpawner = FindObjectOfType<TextToObjectSpawner>();
        }
    }
    
    private void Update()
    {
        if (GameManager.Instance != null)
        {
            int vexonIndex = GameManager.Instance.vexons.FindIndex(v => v.name == vexonName);
            if (vexonIndex != -1)
            {
                bool currentUnlockedStatus = GameManager.Instance.vexons[vexonIndex].isUnlocked;
                
                if (currentUnlockedStatus != lastUnlockedStatus)
                {
                    UpdateButtonState(currentUnlockedStatus);
                    lastUnlockedStatus = currentUnlockedStatus;
                }
            }
        }
        
        CheckIfCurrentlyEquipped();
    }
    
    private void UpdateButtonState(bool isUnlocked)
    {
        if (equipButton != null)
        {
            equipButton.interactable = isUnlocked;
            
            // Visual feedback for locked state
            if (buttonImage != null)
            {
                buttonImage.color = isUnlocked ? Color.white : new Color(0.5f, 0.5f, 0.5f, 1f);
            }
            
            // Optional: Change button text
            if (buttonText != null)
            {
                buttonText.text = isUnlocked ? "Equip" : "Locked";
                buttonText.color = isUnlocked ? Color.white : new Color(0.7f, 0.7f, 0.7f, 1f);
            }
        }
    }
    
    private void CheckIfCurrentlyEquipped()
    {
        if (equipButton == null || textToObjectSpawner == null || GameManager.Instance == null)
            return;

        int vexonIndex = GameManager.Instance.vexons.FindIndex(v => v.name == vexonName);
        if (vexonIndex == -1)
            return;

        bool isCurrentVexon = textToObjectSpawner.objectPrefab == GameManager.Instance.vexons[vexonIndex].prefab;
        
        // Update interactability and appearance
        equipButton.interactable = !isCurrentVexon;
        
        if (buttonText != null)
        {
            buttonText.text = isCurrentVexon ? "Equipped" : "Equip";
            buttonText.color = isCurrentVexon ? new Color(0.7f, 0.7f, 0.7f, 1f) : Color.white;
        }
        
        if (buttonImage != null)
        {
            buttonImage.color = isCurrentVexon ? 
                new Color(0.8f, 0.8f, 0.8f, 0.8f) : // Grayed out when equipped
                Color.white; // Normal when not equipped
        }
    }

    public void EquipVexon()
    {
        Debug.Log("EquipVexon method called!");

        int vexonIndex = GameManager.Instance.vexons.FindIndex(v => v.name == vexonName);

        if (vexonIndex != -1 && GameManager.Instance.vexons[vexonIndex].isUnlocked)
        {
            GameObject vexonPrefab = GameManager.Instance.vexons[vexonIndex].prefab;

            if (vexonPrefab != null && textToObjectSpawner != null)
            {
                textToObjectSpawner.objectPrefab = vexonPrefab;
                Debug.Log("Vexon changed to: " + vexonName);
                
                // Update button state after equipping
                CheckIfCurrentlyEquipped();
            }
        }
    }
}