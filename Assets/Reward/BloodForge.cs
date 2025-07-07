using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(RawImage))]
public class BloodForge : MonoBehaviour, IPointerClickHandler
{
    public float bulletBonusAmount = 3f;
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

        ApplyBulletBonus();

        PlayClickEffects();
        StartCoroutine(FadeOutCanvas());
    }

    private void ApplyBulletBonus()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null) return;

        TryApplyBonus<SpreadGun>(playerObj);
        TryApplyBonus<SingleGun>(playerObj);
        TryApplyBonus<AOEGun>(playerObj);
        TryApplyBonus<StraightGun>(playerObj);
    }

   private void TryApplyBonus<T>(GameObject player) where T : MonoBehaviour
    {
        T gun = player.GetComponentInChildren<T>(true);
        if (gun != null)
        {
            var bulletBonusProperty = typeof(T).GetProperty("bulletBonus");
            if (bulletBonusProperty != null)
            {
                float currentBonus = (float)bulletBonusProperty.GetValue(gun);
                bulletBonusProperty.SetValue(gun, currentBonus + bulletBonusAmount);
                Debug.Log($"{typeof(T).Name} received +{bulletBonusAmount} bulletBonus!");
            }
            else
            {
                // Try direct bulletSpeed if bulletBonus not found
                var bulletSpeedField = typeof(T).GetField("bulletSpeed");
                if (bulletSpeedField != null)
                {
                    float currentSpeed = (float)bulletSpeedField.GetValue(gun);
                    bulletSpeedField.SetValue(gun, currentSpeed + bulletBonusAmount);
                    Debug.Log($"{typeof(T).Name} received +{bulletBonusAmount} bulletSpeed!");
                }
                else
                {
                    Debug.LogWarning($"{typeof(T).Name} has no 'bulletBonus' or 'bulletSpeed' property/field!");
                }
            }
        }
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
        canvasGroup.alpha = 1f;
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
