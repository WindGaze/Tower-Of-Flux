using UnityEngine;
using UnityEngine.UI;

public class EquipShop : MonoBehaviour
{
    public string weaponName;
    public GunHolder gunHolder;
    private Button equipButton;
    private bool lastUnlockedStatus;
    private CanvasGroup canvasGroup;  // For UI fading/hiding

    private void Awake()
    {
        // Get or add a CanvasGroup component
        canvasGroup = gameObject.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // Get the Button component
        equipButton = GetComponent<Button>();
        if (equipButton == null)
        {
            Debug.LogWarning("Button component not found on this GameObject.");
        }
    }

    private void Start()
    {
        if (gunHolder == null)
        {
            gunHolder = FindObjectOfType<GunHolder>();
        }

        if (GameManager.Instance != null)
        {
            int weaponIndex = GameManager.Instance.weapons.FindIndex(w => w.name == weaponName);
            if (weaponIndex != -1)
            {
                lastUnlockedStatus = GameManager.Instance.weapons[weaponIndex].isUnlocked;
                UpdateUIVisibility(lastUnlockedStatus);
            }
        }
    }

    private void OnEnable()
    {
        if (gunHolder == null)
        {
            gunHolder = FindObjectOfType<GunHolder>();
        }
    }

    private void Update()
    {
        if (GameManager.Instance != null)
        {
            int weaponIndex = GameManager.Instance.weapons.FindIndex(w => w.name == weaponName);
            if (weaponIndex != -1)
            {
                bool currentUnlockedStatus = GameManager.Instance.weapons[weaponIndex].isUnlocked;
                
                // Detect unlock status change
                if (currentUnlockedStatus != lastUnlockedStatus)
                {
                    UpdateUIVisibility(currentUnlockedStatus);
                    lastUnlockedStatus = currentUnlockedStatus;
                }
            }
        }

        // Still check button interactability (for equipped status)
        CheckButtonState();
    }

    private void UpdateUIVisibility(bool isUnlocked)
    {
        canvasGroup.alpha = isUnlocked ? 1f : 0f;
        canvasGroup.interactable = isUnlocked;
        canvasGroup.blocksRaycasts = isUnlocked;
    }

    private void CheckButtonState()
    {
        if (equipButton != null && gunHolder != null && GameManager.Instance != null)
        {
            int weaponIndex = GameManager.Instance.weapons.FindIndex(w => w.name == weaponName);
            if (weaponIndex != -1)
            {
                bool isCurrentWeapon = gunHolder.currentWeaponName == weaponName;
                equipButton.interactable = !isCurrentWeapon;
            }
            else
            {
                equipButton.interactable = false;
            }
        }
    }

    public void EquipWeapon()
    {
        Debug.Log("EquipWeapon method called!");

        int weaponIndex = GameManager.Instance.weapons.FindIndex(w => w.name == weaponName);

        if (weaponIndex != -1 && GameManager.Instance.weapons[weaponIndex].isUnlocked)
        {
            GameObject weaponPrefab = GameManager.Instance.weapons[weaponIndex].prefab;

            if (weaponPrefab != null && gunHolder != null)
            {
                // Destroy previous weapon if present
                if (gunHolder.transform.childCount > 0)
                {
                    Destroy(gunHolder.transform.GetChild(0).gameObject);
                }

                GameObject newWeapon = Instantiate(weaponPrefab, gunHolder.transform);

                // Adjust scale to match parent expectations
                Vector3 parentScale = gunHolder.transform.lossyScale;
                Vector3 originalScale = weaponPrefab.transform.localScale;

                newWeapon.transform.localScale = new Vector3(
                    originalScale.x / parentScale.x,
                    originalScale.y / parentScale.y,
                    originalScale.z / parentScale.z
                );

                newWeapon.transform.localPosition = Vector3.zero;
                newWeapon.transform.localRotation = Quaternion.identity;

                gunHolder.gunPrefab = weaponPrefab;
                gunHolder.currentWeaponName = weaponName;

                Debug.Log("Weapon changed to: " + weaponName);

                // Update button interactability after equipping
                CheckButtonState();
            }
            else
            {
                Debug.LogWarning("GunHolder or weaponPrefab is null.");
            }
        }
        else
        {
            Debug.LogWarning("Weapon either not found or is locked!");
        }
    }
}