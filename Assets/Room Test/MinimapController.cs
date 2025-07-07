using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MinimapController : MonoBehaviour
{
    public static MinimapController Instance;

    public RawImage minimapRawImage; // Assign the Raw Image displaying the minimap
    public GameObject roomIconPrefab; // Assign a UI-based prefab for rooms (e.g., an Image)
    public GameObject startRoomIconPrefab; // Assign a UI-based prefab for the start room
    public GameObject redRoomIconPrefab; // Assign a UI-based prefab for the red room (player's current room)
    public float minimapScaleFactor = 0.1f; // Scale factor to map world positions to minimap space
    public Vector2 minimapOffset = Vector2.zero; // Offset to adjust the minimap positioning

    private List<GameObject> roomIcons = new List<GameObject>();
    private GameObject currentPlayerRoomIcon; // Tracks the player's current room icon

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("MinimapController instance set.");
        }
        else
        {
            Debug.LogWarning("Duplicate MinimapController detected, destroying object.");
            Destroy(gameObject);
        }
    }

    void Start()
    {
        AutoDetectRooms(); // Detect objects in "Minimap" and "MinimapStart" layers
    }

    /// <summary>
    /// Automatically detects and registers objects on the "Minimap" and "MinimapStart" layers.
    /// </summary>
    void AutoDetectRooms()
    {
        int minimapLayer = LayerMask.NameToLayer("Minimap"); // Get the layer index for "Minimap"
        int minimapStartLayer = LayerMask.NameToLayer("MinimapStart"); // Get the layer index for "MinimapStart"
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>(); // Find all objects

        foreach (GameObject obj in allObjects)
        {
            if (obj.layer == minimapLayer) // Register objects in the "Minimap" layer
            {
                RegisterRoom(obj.transform.position, roomIconPrefab);
            }
            else if (obj.layer == minimapStartLayer) // Register objects in the "MinimapStart" layer
            {
                RegisterRoom(obj.transform.position, startRoomIconPrefab);
            }
        }
    }

    public void RegisterRoom(Vector2 worldPosition, GameObject prefab)
    {
        Debug.Log($"Registering object at world position: {worldPosition}");

        GameObject newIcon = Instantiate(prefab, minimapRawImage.rectTransform);
        Vector2 minimapPosition = ConvertWorldToMinimap(worldPosition);
        newIcon.GetComponent<RectTransform>().anchoredPosition = minimapPosition;

        Debug.Log($"Icon placed at minimap position: {minimapPosition}");

        // Disable the spawned prefab if it's for the Minimap layer
        if (prefab == roomIconPrefab)
        {
            newIcon.SetActive(false); // Disable the spawned prefab
        }

        roomIcons.Add(newIcon);
    }

    /// <summary>
    /// Enables the room icon and changes it to red when the player enters the room.
    /// </summary>
    public void EnableRoomIcon(Vector2 worldPosition)
    {
        Vector2 minimapPosition = ConvertWorldToMinimap(worldPosition);

        // Revert the previous room's icon to its original color
        if (currentPlayerRoomIcon != null)
        {
            currentPlayerRoomIcon.SetActive(false); // Disable the red icon
            GameObject originalIcon = FindRoomIcon(currentPlayerRoomIcon.GetComponent<RectTransform>().anchoredPosition);
            if (originalIcon != null)
            {
                originalIcon.SetActive(true); // Enable the original icon
            }
        }

        // Find the icon associated with this room
        GameObject icon = FindRoomIcon(minimapPosition);
        if (icon != null)
        {
            // Disable the original icon and enable the red icon
            icon.SetActive(false); // Disable the original icon
            GameObject redIcon = Instantiate(redRoomIconPrefab, minimapRawImage.rectTransform);
            redIcon.GetComponent<RectTransform>().anchoredPosition = minimapPosition;
            redIcon.SetActive(true); // Enable the red icon

            currentPlayerRoomIcon = redIcon; // Update the current player room icon
            Debug.Log($"Enabled red minimap icon at position: {minimapPosition}");
        }
    }

    /// <summary>
    /// Finds the room icon at the specified minimap position.
    /// </summary>
    private GameObject FindRoomIcon(Vector2 minimapPosition)
    {
        foreach (GameObject icon in roomIcons)
        {
            if (icon != null && icon.GetComponent<RectTransform>().anchoredPosition == minimapPosition)
            {
                return icon;
            }
        }
        return null;
    }

    /// <summary>
    /// Converts a world position to a minimap position.
    /// </summary>
   private Vector2 ConvertWorldToMinimap(Vector2 worldPosition)
{
    float spacingMultiplier = 3f; // Increase this value to add more spacing (e.g., 1.5f = 50% more spacing)
    
    Vector2 rawImageSize = minimapRawImage.rectTransform.rect.size;

    // Apply spacing multiplier to the scaled position
    Vector2 minimapPosition = new Vector2(
        worldPosition.x * minimapScaleFactor * spacingMultiplier + minimapOffset.x,
        worldPosition.y * minimapScaleFactor * spacingMultiplier + minimapOffset.y
    );

    RectTransform iconRect = roomIconPrefab.GetComponent<RectTransform>();
    Vector2 iconSize = iconRect.rect.size;

    // Clamp the position to ensure it stays within the minimap bounds
    minimapPosition.x = Mathf.Clamp(
        minimapPosition.x,
        -rawImageSize.x / 2 + iconSize.x / 2,
        rawImageSize.x / 2 - iconSize.x / 2
    );
    minimapPosition.y = Mathf.Clamp(
        minimapPosition.y,
        -rawImageSize.y / 2 + iconSize.y / 2,
        rawImageSize.y / 2 - iconSize.y / 2
    );

    return minimapPosition;
}

    /// <summary>
    /// Clears all icons from the minimap.
    /// </summary>
    public void ClearMinimap()
    {
        foreach (GameObject icon in roomIcons)
        {
            Destroy(icon);
        }
        roomIcons.Clear();
        Debug.Log("Minimap cleared.");
    }
}