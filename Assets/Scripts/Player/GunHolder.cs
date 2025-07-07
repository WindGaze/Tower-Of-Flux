using UnityEngine;

public class GunHolder : MonoBehaviour
{
    [Tooltip("Assign the weapon prefab to equip")]
    public GameObject gunPrefab;

    [HideInInspector]
    public string currentWeaponName; // Optional: set for display or logic if you need

    private GameObject instantiatedGun;

    void Start()
    {
        // Only proceed if gunPrefab is assigned
        if (gunPrefab == null)
        {
            Debug.LogWarning("GunHolder: No weapon prefab assigned!");
            return;
        }

        // Optionally, set the name for logic/UI purposes
        if (string.IsNullOrEmpty(currentWeaponName))
        {
            currentWeaponName = gunPrefab.name.Replace("(Clone)", "").Trim();
        }

        // Instantiate and parent the weapon
        instantiatedGun = Instantiate(gunPrefab, transform.position, Quaternion.identity, transform);

        // Counteract parent scale for proper weapon display
        Vector3 parentScale = transform.lossyScale;
        Vector3 originalScale = gunPrefab.transform.localScale;

        instantiatedGun.transform.localScale = new Vector3(
            originalScale.x / parentScale.x,
            originalScale.y / parentScale.y,
            originalScale.z / parentScale.z
        );

        // If you want to do anything extra after spawning, add it here!
    }

    // Optional: Public method to swap weapon during runtime
    public void EquipWeapon(GameObject newWeaponPrefab)
    {
        if (instantiatedGun != null)
            Destroy(instantiatedGun);

        gunPrefab = newWeaponPrefab;

        if (gunPrefab == null)
        {
            Debug.LogWarning("GunHolder: Tried to equip a null weapon prefab.");
            currentWeaponName = "";
            instantiatedGun = null;
            return;
        }

        currentWeaponName = gunPrefab.name.Replace("(Clone)", "").Trim();
        instantiatedGun = Instantiate(gunPrefab, transform.position, Quaternion.identity, transform);

        // Adjust scaling as before
        Vector3 parentScale = transform.lossyScale;
        Vector3 originalScale = gunPrefab.transform.localScale;

        instantiatedGun.transform.localScale = new Vector3(
            originalScale.x / parentScale.x,
            originalScale.y / parentScale.y,
            originalScale.z / parentScale.z
        );
    }
}