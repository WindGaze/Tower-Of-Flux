using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TextToObjectSpawner : MonoBehaviour
{
    [Header("UI and Spawn Settings")]
    public Text canvasText;             // Assign your UI Text here
    public GameObject objectPrefab;     // Prefab to summon
    public Transform spawnPoint;        // Where to spawn the prefab

    [Header("Voice Text Results")]
    public VoskResultText voskResultText;

    private string lastDetectedText = "";
    private Coroutine resetSummonCoroutine;

    void Start()
    {
        // Display the prefab name at the start
        UpdatePrefabNameUI();

        // Load previous state if needed (optional)
        SaveSystem.LoadGame(this);
    }

    void Update()
    {
        // Always display the current prefab name on the UI every frame
        UpdatePrefabNameUI();

        string heard = (voskResultText != null && voskResultText.ResultText != null)
            ? voskResultText.ResultText.text
            : "";

        // Check for prefab name in speech
        if (!string.IsNullOrEmpty(heard) && objectPrefab != null)
        {
            string prefabName = objectPrefab.name.ToLower();
            string heardLower = heard.ToLower();

            if (heardLower.Contains(prefabName) && heard != lastDetectedText)
            {
                SpawnObject();
                lastDetectedText = heard;
            }
        }

        // Optionally, save with F5
        if (Input.GetKeyDown(KeyCode.F5))
        {
            SaveSystem.SaveGame(this);
        }
    }

    private void UpdatePrefabNameUI()
    {
        if (canvasText != null && objectPrefab != null)
        {
            canvasText.text = objectPrefab.name;
        }
        else if (canvasText != null)
        {
            canvasText.text = "No prefab assigned.";
        }
    }

    private void SpawnObject()
    {
        if (objectPrefab != null && spawnPoint != null)
        {
            Instantiate(objectPrefab, spawnPoint.position, spawnPoint.rotation);

            Debug.Log(objectPrefab.name + " has been spawned at " + spawnPoint.position);

            // Let the user know an object was summoned
            if (canvasText != null)
                canvasText.text = $"Summoned: \"{objectPrefab.name}\"";

            if (voskResultText != null)
            {
                voskResultText.isSummon = true;
                voskResultText.ResultText.text = string.Empty;

                if (resetSummonCoroutine != null)
                    StopCoroutine(resetSummonCoroutine);

                resetSummonCoroutine = StartCoroutine(ResetSummonAfterDelay());
            }
        }
        else
        {
            Debug.LogError("Prefab or Spawn Point is not assigned!");
        }
    }

    private IEnumerator ResetSummonAfterDelay()
    {
        yield return new WaitForSeconds(30f);
        if (voskResultText != null)
            voskResultText.isSummon = false;
    }
}