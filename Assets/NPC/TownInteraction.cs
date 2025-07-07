using UnityEngine;
using UnityEngine.UI;

public class TownInteraction : MonoBehaviour
{
    public GameObject canvasObject; // Reference to the Canvas
    public GameObject player;       // Reference to the Player
    public Button closeButton;      // Reference to the UI Button that closes the canvas

    private bool playerInRange = false;
    private bool isCanvasActive = false;

    private PlayerMovement playerMovementScript;
    private Rigidbody2D playerRb;
    private RigidbodyConstraints2D originalConstraints;

    private GameObject gunHolderObject; // Holds the object that contains GunHolder

    void Start()
    {
        playerMovementScript = player.GetComponent<PlayerMovement>();
        playerRb = player.GetComponent<Rigidbody2D>();
        if (playerRb != null)
        {
            originalConstraints = playerRb.constraints;
        }

        if (canvasObject != null)
            canvasObject.SetActive(false);

        // Listen for button click and unfreeze when clicked
        if (closeButton != null)
            closeButton.onClick.AddListener(OnCloseButtonClicked);
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            ToggleCanvas();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }

    void ToggleCanvas()
    {
        isCanvasActive = !isCanvasActive;

        // Find GunHolder object as needed
        if (gunHolderObject == null)
        {
            GunHolder gunHolderScript = FindObjectOfType<GunHolder>();
            if (gunHolderScript != null)
            {
                gunHolderObject = gunHolderScript.gameObject;
            }
        }

        if (canvasObject != null)
            canvasObject.SetActive(isCanvasActive);

        if (isCanvasActive)
        {
            if (playerMovementScript != null)
                playerMovementScript.enabled = false;

            if (playerRb != null)
            {
                playerRb.velocity = Vector2.zero;
                playerRb.angularVelocity = 0f;
                playerRb.constraints = RigidbodyConstraints2D.FreezeAll;
            }

            if (gunHolderObject != null)
                gunHolderObject.SetActive(false);

            Debug.Log("Canvas Activated. Player FULLY frozen (constraints + input, gun disabled).");
        }
        else
        {
            UnfreezePlayer();
        }
    }

    // Called when the button is clicked
    void OnCloseButtonClicked()
    {
        // Only unfreeze if canvas is active
        if (isCanvasActive)
        {
            isCanvasActive = false;
            if (canvasObject != null)
                canvasObject.SetActive(false);

            UnfreezePlayer();
        }
    }

    // Handles unfreezing logic
    void UnfreezePlayer()
    {
        if (playerRb != null)
        {
            playerRb.constraints = originalConstraints;
            playerRb.velocity = Vector2.zero;
            playerRb.angularVelocity = 0f;
        }
        if (playerMovementScript != null)
            playerMovementScript.enabled = true;

        if (gunHolderObject != null)
            gunHolderObject.SetActive(true);

        Debug.Log("Canvas Deactivated by button. Player input, physics, and gun restored.");
    }
}