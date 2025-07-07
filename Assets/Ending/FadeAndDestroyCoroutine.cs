using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FadeAndDestroyCoroutine : MonoBehaviour
{
    public float fadeDuration = 2f;
    public float delayBeforeFade = 2f;

    private RawImage rawImage;

    void Start()
    {
        rawImage = GetComponent<RawImage>();
        
        if (rawImage == null)
        {
            Debug.LogError("FadeAndDestroyCoroutine script requires a RawImage component!");
            Destroy(this);
            return;
        }

        StartCoroutine(FadeOutAndDestroy());
    }

    IEnumerator FadeOutAndDestroy()
    {
        // Wait for the initial delay
        yield return new WaitForSeconds(delayBeforeFade);

        // Fade out over the duration
        float elapsedTime = 0f;
        Color startColor = rawImage.color;
        
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            rawImage.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        // Ensure it's completely transparent before destroying
        rawImage.color = new Color(startColor.r, startColor.g, startColor.b, 0f);
        
        Destroy(gameObject);
    }
}