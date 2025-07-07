using UnityEngine;
using UnityEngine.UI;

public class ButtonSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject objectToSpawn;    // Prefab to spawn
    public Transform spawnLocation;    // Spawn position
    public Canvas canvasToClose;       // Canvas to disable
    public int goldCost = 10;         // Gold cost per spawn

    [Header("Physics Effects")]
    public float pushDistance = 1f;    // How far the object slides
    public float pushSpeed = 3f;       // Initial slide speed
    public float decelerationRate = 0.9f; // Slows down over time
    public float colliderDelay = 0.5f; // Time before collider activates

    [Header("References")]
    public Button spawnButton;
    public Text costText;
    public Event7 linkedEvent;         // Optional Event7 link

    private PlayerMovement player;

    private void Start()
    {
        player = FindObjectOfType<PlayerMovement>();
        spawnButton.onClick.AddListener(OnButtonClick);

        if (costText != null)
            costText.text = $"Cost: {goldCost} Gold";
    }

    private void OnButtonClick()
    {
        if (player.gold >= goldCost)
        {
            // Deduct gold first
            player.gold -= goldCost;
            Debug.Log($"Spent {goldCost} gold. Remaining: {player.gold}");

            // Spawn with physics effects
            if (objectToSpawn != null && spawnLocation != null)
            {
                GameObject spawnedObj = Instantiate(objectToSpawn, spawnLocation.position, Quaternion.identity);
                ApplyTreasureEffects(spawnedObj);
            }

            // Close canvas
            if (canvasToClose != null)
                canvasToClose.enabled = false;

            // Update Event7 state if linked
            if (linkedEvent != null)
            {
                linkedEvent.isPlayerInside = true;
                linkedEvent.isInteracted = true;
            }
        }
        else
        {
            Debug.Log("Not enough gold!");
        }
    }

    private void ApplyTreasureEffects(GameObject obj)
    {
        // Disable all colliders initially
        Collider2D[] colliders = obj.GetComponentsInChildren<Collider2D>();
        foreach (Collider2D col in colliders)
        {
            col.enabled = false;
        }

        // Add physics mover (reusing your KinematicMover2D)
        KinematicMover2D mover = obj.GetComponent<KinematicMover2D>() ?? obj.AddComponent<KinematicMover2D>();
        mover.SetupPush(
            Random.insideUnitCircle.normalized, // Random slide direction
            pushSpeed,
            decelerationRate,
            colliders,
            colliderDelay
        );
    }
}