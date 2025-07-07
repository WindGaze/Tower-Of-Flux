using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(RawImage))]
public class AngelGrace : MonoBehaviour, IPointerClickHandler
{
    public Canvas TargetCanvas;
    public float FadeDuration = 0.3f;
    public AudioClip ClickSound;
    public ParticleSystem ClickParticles;

    private RawImage _rawImage;
    private bool _isInteractable = true;

    private void Awake()
    {
        _rawImage = GetComponent<RawImage>();
        _rawImage.raycastTarget = true;

        if (TargetCanvas == null)
            TargetCanvas = GameObject.FindGameObjectWithTag("RewardCanvas")?.GetComponent<Canvas>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!_isInteractable) return;
        _isInteractable = false;

        ApplyReviveEffect();
        PlayClickEffects();
        StartCoroutine(FadeOutCanvas());
    }

    private void ApplyReviveEffect()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("Player not found!");
            return;
        }

        PlayerMovement pm = player.GetComponent<PlayerMovement>();
        if (pm != null)
        {
            pm.isRevive = true;
            Debug.Log("AngelGrace: isRevive set to true on Player");
        }
        else
        {
            Debug.LogWarning("PlayerMovement component not found on Player!");
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
        canvasGroup.alpha = 1f;
    }

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