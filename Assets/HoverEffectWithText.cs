using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class HoverEffectWithText : MonoBehaviour
{
    [SerializeField] private float showDuration = 0.35f;
    [SerializeField] private Button hoverButton;
    [SerializeField] private Image buttonIcon;
    private Color hideBaseColor = new Color (0.5215687f, 0.4980392f, 0.4313726f, 0.2117647f);
    [SerializeField] private Color hoverColor = Color.gray;

    [SerializeField] private CanvasGroup textGroup;
    [SerializeField] private float textFadeDuration = 0.2f;
    [SerializeField] private float textDelay = 1f;
    private Tween textTween;

    public void OnHideHoverEnter()
    {
        buttonIcon.DOKill();
        buttonIcon.DOColor(hoverColor, 0.2f);

        textTween?.Kill();

        textGroup.gameObject.SetActive(true);
        textGroup.alpha = 0f;

        textTween = DOTween.Sequence()
            .AppendInterval(textDelay)
            .Append(textGroup.DOFade(1f, textFadeDuration));
    }

    public void OnHideHoverExit()
    {
        buttonIcon.DOKill();
        buttonIcon.DOColor(hideBaseColor, 0.2f);

        textTween?.Kill();

        textGroup.DOKill();
        textGroup.DOFade(0f, 0.1f)
            .OnComplete(() =>
            {
                textGroup.gameObject.SetActive(false);
            });
    }
}
