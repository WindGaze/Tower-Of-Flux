using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndGame : MonoBehaviour
{
    [Header("Scene Transition Settings")]
    [Tooltip("Enter the name of the next scene to load")]
    [SerializeField] private string nextSceneName;
    [SerializeField] private GameObject transitionCanvas;
    [SerializeField] private Text transitionText;
    [SerializeField] private float delayBeforeTransition = 2f;

    [Header("Bind Status Check")]
    [SerializeField] private bool requireBindCheck = true;
    [SerializeField] private bool useTrigger = true;
    [SerializeField] private bool useCollision = true;

    private bool isTransitioning = false;
    private PlayerMovement player;
    private GameManager gameManager;

    private void Awake()
    {
        player = FindObjectOfType<PlayerMovement>();
        gameManager = GameManager.Instance;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (useTrigger && other.CompareTag("Player") && !isTransitioning)
        {
            Debug.Log("Player trigger detected in EndGame");
            StartTransition();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (useCollision && collision.gameObject.CompareTag("Player") && !isTransitioning)
        {
            Debug.Log("Player collision detected in EndGame");
            StartTransition();
        }
    }

    private void StartTransition()
    {
        // Check bind status if required
        if (requireBindCheck && player != null)
        {
            UpdateBindStatusInGameManager();
            
            if (transitionText != null)
            {
                string bindStatus = $"Wail: {player.wailBind}, Soul: {player.soulBind}, Heart: {player.heartBind}";
                transitionText.text = $"Binds detected:\n{bindStatus}";
            }
        }

        isTransitioning = true;
        
        if (transitionCanvas != null)
        {
            transitionCanvas.SetActive(true);
            
            if (transitionText != null && !requireBindCheck)
            {
                transitionText.text = "Game ending...";
            }
        }
        
        StartCoroutine(LoadNextSceneAfterDelay());
    }

    private void UpdateBindStatusInGameManager()
    {
        if (player != null && gameManager != null)
        {
            gameManager.wailBind = player.wailBind;
            gameManager.soulBind = player.soulBind;
            gameManager.heartBind = player.heartBind;

            Debug.Log($"Bind status updated in GameManager - " +
                     $"Wail: {gameManager.wailBind}, " +
                     $"Soul: {gameManager.soulBind}, " +
                     $"Heart: {gameManager.heartBind}");
        }
    }

    private System.Collections.IEnumerator LoadNextSceneAfterDelay()
    {
        yield return new WaitForSeconds(delayBeforeTransition);
        
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogError("No next scene specified on " + gameObject.name);
            if (transitionCanvas != null)
            {
                transitionCanvas.SetActive(false);
            }
            isTransitioning = false;
        }
    }
}