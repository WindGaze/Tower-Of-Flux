using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(RawImage))] // Requires a UI Image for clicking
public class IronVial : MonoBehaviour, IPointerClickHandler
{
    public GameObject Shield; // The shield prefab to instantiate
    public int healthBonus = 30; // Health to add if shield exists
    public Canvas TargetCanvas; // The UI canvas (for fade-out effect)
    public float FadeDuration = 0.3f; // How fast the UI fades out

    public AudioClip ClickSound; // Sound when clicked
    public ParticleSystem ClickParticles; // Visual effect when clicked

    private RawImage _rawImage;
    private bool _isInteractable = true;

    private void Awake()
    {
        _rawImage = GetComponent<RawImage>();
        _rawImage.raycastTarget = true; // Make sure it can be clicked

        // Auto-find the target canvas if not assigned
        if (TargetCanvas == null)
            TargetCanvas = GameObject.FindGameObjectWithTag("RewardCanvas")?.GetComponent<Canvas>();
    }

    // Called when the player clicks/taps this UI element
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!_isInteractable) return;
        _isInteractable = false;

        ApplyShieldEffect();
        PlayClickEffects();
        StartCoroutine(FadeOutCanvas());
    }

    private void ApplyShieldEffect()
    {
        // Find the player (instead of using OnTriggerEnter2D)
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("Player not found!");
            return;
        }

        // Check if player already has a shield
        ShieldPlayer existingShield = player.GetComponentInChildren<ShieldPlayer>();
        
        if (existingShield != null)
        {
            // Increase existing shield's health
            existingShield.health += healthBonus;
            Debug.Log($"Increased existing shield health to {existingShield.health}");
        }
        else
        {
            // Spawn new shield if none exists
            if (Shield != null)
            {
                Instantiate(Shield, player.transform.position, Quaternion.identity, player.transform);
                Debug.Log("Created new shield");
            }
            else
            {
                Debug.LogWarning("Shield prefab not assigned!");
            }
        }
    }

    private void PlayClickEffects()
    {
        if (ClickSound != null)
            AudioSource.PlayClipAtPoint(ClickSound, Camera.main.transform.position);

        if (ClickParticles != null)
            Instantiate(ClickParticles, transform.position, Quaternion.identity);
    }

    private IEnumerator FadeOutCanvas()
    {
        if (TargetCanvas == null) yield break;

        CanvasGroup canvasGroup = TargetCanvas.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = TargetCanvas.gameObject.AddComponent<CanvasGroup>();

        float elapsed = 0f;
        float startAlpha = canvasGroup.alpha;

        while (elapsed < FadeDuration)
        {
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / FadeDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        canvasGroup.alpha = 0f;
        TargetCanvas.gameObject.SetActive(false);
        canvasGroup.alpha = 1f; // Reset for next time
    }

    // Call this method if you want to reset the reward (e.g., for reuse)
    public void ResetReward()
    {
        _isInteractable = true;
        if (TargetCanvas != null)
        {
            TargetCanvas.gameObject.SetActive(true);
            var cg = TargetCanvas.GetComponent<CanvasGroup>();
            if (cg != null) cg.alpha = 1f;
        }
    }
}