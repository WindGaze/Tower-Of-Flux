using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(RawImage))]
public class CoinRewards : MonoBehaviour, IPointerClickHandler
{
    public int coinAmount = 10;
    public Canvas targetCanvas;
    public float fadeDuration = 0.3f;
    public AudioClip clickSound;
    public ParticleSystem clickParticles;

    private RawImage _rawImage;
    private bool _isInteractable = true;

    private void Awake()
    {
        _rawImage = GetComponent<RawImage>();
        _rawImage.raycastTarget = true;

        if (targetCanvas == null)
            targetCanvas = GameObject.FindGameObjectWithTag("RewardCanvas")?.GetComponent<Canvas>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!_isInteractable) return;
        _isInteractable = false;

        ApplyCoinReward();
        PlayEffects();
        StartCoroutine(FadeOut());
    }

    private void ApplyCoinReward()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("[CoinRewards] No GameObject tagged 'Player' found in scene!");
            return;
        }

        PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.gold += coinAmount;
            Debug.Log($"[CoinRewards] Gold increased by {coinAmount}. Player now has: {playerMovement.gold}");
        }
        else
        {
            Debug.LogWarning("[CoinRewards] PlayerMovement component not found on the Player GameObject!");
        }
    }

    private void PlayEffects()
    {
        if (clickSound != null && Camera.main != null)
            AudioSource.PlayClipAtPoint(clickSound, Camera.main.transform.position);

        if (clickParticles != null)
            Instantiate(clickParticles, transform.position, Quaternion.identity);
    }

    private IEnumerator FadeOut()
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

        targetCanvas.gameObject.SetActive(false);
        canvasGroup.alpha = 1f;
        Destroy(gameObject);
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