using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(RawImage))]
public class WeaponReward : MonoBehaviour, IPointerClickHandler
{
    [Header("Damage Boost Settings")]
    public int damageBonus = 1; 
    public Canvas targetCanvas;  
    public float fadeDuration = 0.3f; 

    [Header("Visual Effects")]
    public AudioClip clickSound;
    public ParticleSystem clickParticles;

    private RawImage _rawImage;
    private bool _isInteractable = true;
    private SpreadGun _activeGun; // Reference to the gun script
    private int _activeBulletBonus = 0; // Stores the temporary bonus

    private void Awake()
    {
        _rawImage = GetComponent<RawImage>();
        _rawImage.raycastTarget = true;

        if (targetCanvas == null)
            targetCanvas = GameObject.FindGameObjectWithTag("RewardCanvas")?.GetComponent<Canvas>();

        // Find the active gun via GunHolder
        GunHolder gunHolder = FindObjectOfType<GunHolder>();
        if (gunHolder != null)
        {
            _activeGun = gunHolder.GetComponentInChildren<SpreadGun>();
            if (_activeGun == null)
                Debug.LogError("SpreadGun not found in GunHolder!");
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!_isInteractable || _activeGun == null) return;
        _isInteractable = false;

        // Activate the temporary bullet bonus
        _activeBulletBonus = damageBonus;
        Debug.Log($"Next bullets will deal +{damageBonus} bonus damage!");

        PlayClickEffects();
        StartCoroutine(FadeOutCanvas());
    }

    // === BULLET MODIFICATION ===
    // Call this from your gun's shooting logic (e.g., in SpreadGun.cs)
    public int GetActiveBulletBonus()
    {
        int bonus = _activeBulletBonus;
        _activeBulletBonus = 0; // Reset after applying
        return bonus;
    }
    private void PlayClickEffects()
    {
        if (clickSound != null)
            AudioSource.PlayClipAtPoint(clickSound, Camera.main.transform.position);

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
        canvasGroup.alpha = 1f; // Reset for reuse
    }

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