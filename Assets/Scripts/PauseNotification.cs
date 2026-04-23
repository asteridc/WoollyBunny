using UnityEngine;
using TMPro;
using DG.Tweening;

public class PauseNotification : MonoBehaviour
{
    [Header("References")]
    public CanvasGroup pauseTextGroup; // CanvasGroup вместо TMP alpha
    public float delay = 1f;
    public float fadeDuration = 0.5f;
    public float scaleAmount = 1.1f;

    private void Awake()
    {
        if (pauseTextGroup != null)
        {
            pauseTextGroup.alpha = 0f;
            pauseTextGroup.transform.localScale = Vector3.one;
        }
    }

    public void ShowPauseText()
    {
        if (pauseTextGroup == null) return;
        pauseTextGroup.DOKill();
        pauseTextGroup.alpha = 0f;
        pauseTextGroup.transform.localScale = Vector3.one;

        StartCoroutine(ShowWithDelay());
    }

    private System.Collections.IEnumerator ShowWithDelay()
    {
        yield return new WaitForSecondsRealtime(delay);

        // Анимация появления и масштаба
        pauseTextGroup.DOFade(1f, fadeDuration).SetEase(Ease.OutQuad).SetUpdate(true);
        pauseTextGroup.transform.DOScale(Vector3.one * scaleAmount, fadeDuration)
            .SetEase(Ease.OutBack)
            .SetUpdate(true)
            .OnComplete(() => pauseTextGroup.transform.DOScale(Vector3.one, 0.2f).SetUpdate(true));
    }

    public void HidePauseText()
    {
        if (pauseTextGroup == null) return;

        pauseTextGroup.DOFade(0f, 0.25f).SetUpdate(true);
        pauseTextGroup.transform.localScale = Vector3.one;
    }
}
