using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class VoskResultText : MonoBehaviour
{
    public VoskSpeechToText VoskSpeechToText;
    public Text ResultText;
    public bool isSummon = false;
    public Image uiImageToFade;
    [Range(0.1f, 1f)] public float fadeAlpha = 0.5f;
    private float originalAlpha;

    private Coroutine processCoroutine;

    void Awake()
    {
        VoskSpeechToText.OnTranscriptionResult += OnTranscriptionResult;
        if (uiImageToFade != null)
        {
            originalAlpha = uiImageToFade.color.a;
        }
    }

    void Update()
    {
        if (uiImageToFade != null)
        {
            Color currentColor = uiImageToFade.color;
            float targetAlpha = isSummon ? fadeAlpha : originalAlpha;
            currentColor.a = Mathf.Lerp(currentColor.a, targetAlpha, Time.deltaTime * 5f);
            uiImageToFade.color = currentColor;
        }
    }

    private void OnTranscriptionResult(string obj)
    {
        if (isSummon) return;

        Debug.Log(obj);
        var result = new RecognitionResult(obj);
        for (int i = 0; i < result.Phrases.Length; i++)
        {
            if (i > 0)
            {
                ResultText.text += ", ";
            }
            ResultText.text += result.Phrases[i].Text;
        }
        ResultText.text += "\n";

        if (processCoroutine != null)
            StopCoroutine(processCoroutine);
        processCoroutine = StartCoroutine(ClearTextAfterDelay());
    }

    private IEnumerator ClearTextAfterDelay()
    {
        yield return new WaitForSeconds(0.5f);
        ResultText.text = string.Empty;
    }
}