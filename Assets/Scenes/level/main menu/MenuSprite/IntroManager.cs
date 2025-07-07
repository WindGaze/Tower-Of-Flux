using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class IntroManager : MonoBehaviour
{
    [Header("Display Settings")]
    public GameObject[] introPanels;
    public float panelDuration = 7f;
    public float fadeTime = 1f;
    public float endWaitTime = 2f;

    [Header("System Control")]
    public bool blockInput = true;
    [SerializeField] private bool seen = false;

    [Header("Post-Intro Actions")]
    [Tooltip("This object will be deactivated after all intro panels are done")]
    public GameObject objectToDeactivate;

    private int currentPanel = 0;

    void Start()
    {
        // Load saved state
        seen = SaveSystem.LoadIntroSeenState();

        if (seen)
        {
            // If intro already seen, deactivate the object immediately and disable intro
            if (objectToDeactivate != null)
            {
                objectToDeactivate.SetActive(false);
            }
            gameObject.SetActive(false);
            return;
        }

        if (introPanels == null || introPanels.Length == 0)
        {
            Debug.LogWarning("No intro panels assigned!");
            return;
        }

        InitializeIntro();
        StartCoroutine(PlayIntroSequence());
    }

    void InitializeIntro()
    {
        if (blockInput)
        {
            foreach (var raycaster in FindObjectsOfType<GraphicRaycaster>())
                raycaster.enabled = false;
        }

        // Initialize all panels
        foreach (var panel in introPanels)
        {
            panel.SetActive(false);
            var canvasGroup = panel.GetComponent<CanvasGroup>() ?? panel.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
        }
    }

    IEnumerator PlayIntroSequence()
    {
        // Fade in first panel
        yield return FadePanel(introPanels[currentPanel], 0f, 1f);

        // Main sequence
        while (currentPanel < introPanels.Length - 1)
        {
            yield return new WaitForSeconds(panelDuration);
            yield return FadePanel(introPanels[currentPanel], 1f, 0f);
            currentPanel++;
            yield return FadePanel(introPanels[currentPanel], 0f, 1f);
        }

        // Final panel
        yield return new WaitForSeconds(panelDuration);
        yield return FadePanel(introPanels[currentPanel], 1f, 0f);
        
        // Mark as seen and save
        seen = true;
        SaveSystem.SaveIntroSeenState(true);
        
        // Restore systems
        RestoreSystems();
        
        // Deactivate the assigned object after all panels are done
        if (objectToDeactivate != null)
        {
            objectToDeactivate.SetActive(false);
            Debug.Log("Deactivated object: " + objectToDeactivate.name);
        }
        
        // Wait then disable everything
        yield return new WaitForSeconds(endWaitTime);
        gameObject.SetActive(false);
    }

    IEnumerator FadePanel(GameObject panel, float startAlpha, float endAlpha)
    {
        panel.SetActive(true);
        CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();

        float elapsed = 0f;
        while (elapsed < fadeTime)
        {
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / fadeTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        canvasGroup.alpha = endAlpha;
        panel.SetActive(endAlpha > 0);
    }

    void RestoreSystems()
    {
        // Restore input
        if (blockInput)
        {
            foreach (var raycaster in FindObjectsOfType<GraphicRaycaster>())
                raycaster.enabled = true;
        }
    }
}