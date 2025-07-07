using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class CameraFocusOnBoss : MonoBehaviour
{
    [Header("Focus Settings")]
    public float moveSpeed = 5f;
    public float focusDuration = 4f;
    public float checkInterval = 1f;
    public float arrivalThreshold = 0.05f;
    public Vector3 cameraOffset = new Vector3(0, 0, -10);

    [Header("Player Following")]
    public Transform playerTransform;
    public float followSmoothness = 5f;
    public Vector3 normalOffset = new Vector3(0, 0, -10);

    private Vector3 originalPosition;
    private bool isFocusing = false;
    private Coroutine focusCoroutine;
    private string currentSceneName;
    private bool hasFocusedInThisScene = false;
    private Transform originalParent;
    private bool shouldFollowPlayer = true;

    private void Awake()
    {
        originalParent = transform.parent;
        originalPosition = transform.position;
        currentSceneName = SceneManager.GetActiveScene().name;
        
        if (playerTransform == null)
        {
            playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        }
    }

    private void Start()
    {
        StartCoroutine(CheckForBossRoutine());
        SceneManager.activeSceneChanged += OnSceneChanged;
    }

    private void OnSceneChanged(Scene previousScene, Scene newScene)
    {
        currentSceneName = newScene.name;
        hasFocusedInThisScene = false;
    }

    private void OnDestroy()
    {
        SceneManager.activeSceneChanged -= OnSceneChanged;
    }

    private void LateUpdate()
    {
        if (!shouldFollowPlayer || isFocusing || playerTransform == null)
            return;

        Vector3 targetPosition = playerTransform.position + normalOffset;
        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            followSmoothness * Time.deltaTime
        );
    }


    private IEnumerator CheckForBossRoutine()
    {
        while (true)
        {
            if (!isFocusing && !hasFocusedInThisScene)
            {
                GameObject boss = FindBoss();
                if (boss != null)
                {
                    Debug.Log($"Boss found: {boss.name}. Camera focusing...");
                    focusCoroutine = StartCoroutine(FocusOnBossRoutine(boss.transform));
                    hasFocusedInThisScene = true;
                }
            }
            yield return new WaitForSeconds(checkInterval);
        }
    }

    private GameObject FindBoss()
    {
        GameObject boss = GameObject.FindGameObjectWithTag("Boss");
        if (boss == null)
        {
            var allObjects = GameObject.FindObjectsOfType<GameObject>(true);
            foreach (var obj in allObjects)
            {
                if (obj.CompareTag("Boss") && obj.scene.isLoaded)
                {
                    return obj;
                }
            }
        }
        return boss;
    }

    private IEnumerator FocusOnBossRoutine(Transform boss)
    {
        isFocusing = true;
        shouldFollowPlayer = false;
        
        // Store current position before focusing
        Vector3 startPosition = transform.position;
        
        // Move to boss position
        Vector3 targetPosition = boss.position + cameraOffset;
        yield return StartCoroutine(SmoothMoveToPosition(targetPosition));

        // Wait at boss position
        float timer = 0;
        while (timer < focusDuration && boss != null)
        {
            if (boss != null)
            {
                targetPosition = boss.position + cameraOffset;
                transform.position = Vector3.Lerp(
                    transform.position, 
                    targetPosition, 
                    moveSpeed * Time.deltaTime
                );
            }
            timer += Time.deltaTime;
            yield return null;
        }

        // Return to player following
        shouldFollowPlayer = true;
        isFocusing = false;
        focusCoroutine = null;
    }

    private IEnumerator SmoothMoveToPosition(Vector3 targetPosition)
    {
        targetPosition.z = originalPosition.z;
        
        while (Vector3.Distance(transform.position, targetPosition) > arrivalThreshold)
        {
            transform.position = Vector3.Lerp(
                transform.position, 
                targetPosition, 
                moveSpeed * Time.deltaTime
            );
            yield return null;
        }
        
        transform.position = targetPosition;
    }

    public void TriggerBossFocus(GameObject boss)
    {
        if (!isFocusing && boss != null)
        {
            if (focusCoroutine != null)
            {
                StopCoroutine(focusCoroutine);
            }
            focusCoroutine = StartCoroutine(FocusOnBossRoutine(boss.transform));
            hasFocusedInThisScene = true;
        }
    }

    private void OnDisable()
    {
        if (focusCoroutine != null)
        {
            StopCoroutine(focusCoroutine);
            isFocusing = false;
            shouldFollowPlayer = true;
        }
    }
}