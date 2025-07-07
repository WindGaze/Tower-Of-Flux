using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(RawImage))]
public class HpReward : MonoBehaviour, IPointerClickHandler
{
    [Header("Healing Settings")]
    public int healAmount = 30;
    public Canvas targetCanvas;  // Assign in Inspector or leave for tag search
    public float fadeDuration = 0.3f; // Set to 0 for instant hide

    [Header("Visual Effects")]
    public AudioClip clickSound;
    public ParticleSystem clickParticles;

    private RawImage _rawImage;
    private bool _isInteractable = true;

    private void Awake()
    {
        _rawImage = GetComponent<RawImage>();
        _rawImage.raycastTarget = true;

        // Auto-find Canvas if not assigned
        if (targetCanvas == null)
            targetCanvas = GameObject.FindGameObjectWithTag("RewardCanvas")?.GetComponent<Canvas>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!_isInteractable) return;
        _isInteractable = false;

        // Heal player
        PlayerMovement player = FindObjectOfType<PlayerMovement>();
        if (player != null)
        {
            player.health = Mathf.Min(player.health + healAmount, player.maxHealth);
            Debug.Log($"Healed player by {healAmount} HP!");
        }

        // Play effects
        PlayClickEffects();

        // Start fade-out coroutine
        StartCoroutine(FadeOutCanvas());
    }

    private void PlayClickEffects()
    {
        // Play sound if available
        if (clickSound != null)
            AudioSource.PlayClipAtPoint(clickSound, Camera.main.transform.position);

        // Play particles if available
        if (clickParticles != null)
            Instantiate(clickParticles, transform.position, Quaternion.identity);
    }

    private IEnumerator FadeOutCanvas()
    {
        if (targetCanvas == null) yield break;

        CanvasGroup canvasGroup = targetCanvas.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = targetCanvas.gameObject.AddComponent<CanvasGroup>();

        float elapsed = 0f;
        float startAlpha = canvasGroup.alpha;

        while (elapsed < fadeDuration)
        {
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / fadeDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        canvasGroup.alpha = 0f;
        targetCanvas.gameObject.SetActive(false);

        // Reset for reuse (optional)
        canvasGroup.alpha = 1f;
    }

    // Call this if you need to reset the reward
    public void ResetReward()
    {
        _isInteractable = true;
        if (targetCanvas != null)
        {
            targetCanvas.gameObject.SetActive(true);
            var cg = targetCanvas.GetComponent<CanvasGroup>();
            if (cg != null) cg.alpha = 1f;
        }
    }
}