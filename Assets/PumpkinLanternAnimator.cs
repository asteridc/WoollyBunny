using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Handles only the animated visibility of the pumpkin lantern UI.
/// Keep this component on an active object outside the panel it controls.
/// </summary>
public sealed class PumpkinLanternAnimator : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button openButton;
    [SerializeField] private Button closeButton;

    [Header("Panel")]
    [SerializeField] private CanvasGroup panelCanvasGroup;
    [SerializeField] private RectTransform panelTransform;

    [Header("Animation")]
    [SerializeField, Min(0.01f)] private float animationDuration = 0.25f;
    [SerializeField, Range(0.8f, 0.99f)] private float hiddenScale = 0.94f;

    [Header("Sidney Phrases")]
    [SerializeField] private TMP_Text sidneyPhraseText;

    [TextArea(2, 4)]
    [SerializeField] private string[] sidneyPhrases;

    private int lastSidneyPhraseIndex = -1;

    private Sequence currentAnimation;
    private bool isOpen;
    private bool isAnimating;

    private void Awake()
    {
        HideInstant();
    }

    private void Update()
    {
        // Если окно закрыто или сейчас проигрывается анимация,
        // ничего не делаем.
        if (!isOpen || isAnimating)
            return;

        // Закрываем окно при нажатии Escape.
        if (Input.GetKeyDown(KeyCode.Escape))
            Close();
    }
    private void OnEnable()
    {
        if (openButton != null)
            openButton.onClick.AddListener(Open);

        if (closeButton != null)
            closeButton.onClick.AddListener(Close);
    }

    private void OnDisable()
    {
        if (openButton != null)
            openButton.onClick.RemoveListener(Open);

        if (closeButton != null)
            closeButton.onClick.RemoveListener(Close);

        currentAnimation?.Kill();
        currentAnimation = null;

        // Reset the panel if its parent UI is disabled while an animation is running.
        HideInstant();
    }

    private void OnDestroy()
    {
        currentAnimation?.Kill();
    }

    public void Open()
    {
        if (isOpen || isAnimating || !ReferencesAreValid())
            return;

        ShowRandomSidneyPhrase();

        isOpen = true;
        isAnimating = true;

        currentAnimation?.Kill();
        panelCanvasGroup.DOKill();
        panelTransform.DOKill();

        panelCanvasGroup.gameObject.SetActive(true);
        panelCanvasGroup.alpha = 0f;
        panelCanvasGroup.interactable = false;
        // Block clicks behind the panel even while it is fading in.
        panelCanvasGroup.blocksRaycasts = true;
        panelTransform.localScale = Vector3.one * hiddenScale;

        currentAnimation = DOTween.Sequence().SetUpdate(true);
        currentAnimation.Join(panelCanvasGroup.DOFade(1f, animationDuration));
        currentAnimation.Join(
            panelTransform.DOScale(1f, animationDuration)
                .SetEase(Ease.OutCubic));
        currentAnimation.OnComplete(() =>
        {
            isAnimating = false;
            if (panelCanvasGroup != null)
                panelCanvasGroup.interactable = true;
        });
    }

    public void Close()
    {
        if (!isOpen || isAnimating || !ReferencesAreValid())
            return;

        isOpen = false;
        isAnimating = true;
        panelCanvasGroup.interactable = false;
        // Keep blocking raycasts until the closing animation finishes.
        panelCanvasGroup.blocksRaycasts = true;

        currentAnimation?.Kill();
        panelCanvasGroup.DOKill();
        panelTransform.DOKill();

        currentAnimation = DOTween.Sequence().SetUpdate(true);
        currentAnimation.Join(panelCanvasGroup.DOFade(0f, animationDuration));
        currentAnimation.Join(
            panelTransform.DOScale(hiddenScale, animationDuration)
                .SetEase(Ease.InCubic));
        currentAnimation.OnComplete(() =>
        {
            HideInstant();
            isAnimating = false;
        });
    }

    private void ShowRandomSidneyPhrase()
    {
        if (sidneyPhraseText == null)
        {
            Debug.LogWarning(
                "PumpkinLanternAnimator: assign Sidney Phrase Text in the Inspector.",
                this);
            return;
        }

        if (sidneyPhrases == null || sidneyPhrases.Length == 0)
        {
            Debug.LogWarning(
                "PumpkinLanternAnimator: add phrases to Sidney Phrases in the Inspector.",
                this);
            return;
        }

        // Собираем только непустые фразы.
        List<int> candidates = new List<int>();

        for (int i = 0; i < sidneyPhrases.Length; i++)
        {
            if (!string.IsNullOrWhiteSpace(sidneyPhrases[i]) &&
                i != lastSidneyPhraseIndex)
            {
                candidates.Add(i);
            }
        }

        // Если все подходящие фразы исключены из-за прошлого выбора,
        // разрешаем выбрать любую непустую фразу.
        if (candidates.Count == 0)
        {
            for (int i = 0; i < sidneyPhrases.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(sidneyPhrases[i]))
                    candidates.Add(i);
            }
        }

        if (candidates.Count == 0)
        {
            Debug.LogWarning(
                "PumpkinLanternAnimator: all Sidney phrases are empty.",
                this);
            return;
        }

        int randomCandidate = Random.Range(0, candidates.Count);
        lastSidneyPhraseIndex = candidates[randomCandidate];

        sidneyPhraseText.text = sidneyPhrases[lastSidneyPhraseIndex];
    }
    private bool ReferencesAreValid()
    {
        if (panelCanvasGroup == null || panelTransform == null)
        {
            Debug.LogWarning(
                "PumpkinLanternPanelAnimator: assign Panel Canvas Group and Panel Transform in the Inspector.",
                this);
            return false;
        }

        if (panelCanvasGroup.gameObject == gameObject)
        {
            Debug.LogError(
                "PumpkinLanternPanelAnimator must be outside the panel it controls; otherwise disabling the panel also disables this controller.",
                this);
            return false;
        }

        return true;
    }

    private void HideInstant()
    {
        if (panelCanvasGroup == null || panelCanvasGroup.gameObject == gameObject)
            return;

        panelCanvasGroup.DOKill();
        if (panelTransform != null)
        {
            panelTransform.DOKill();
            panelTransform.localScale = Vector3.one * hiddenScale;
        }

        panelCanvasGroup.alpha = 0f;
        panelCanvasGroup.interactable = false;
        panelCanvasGroup.blocksRaycasts = false;
        panelCanvasGroup.gameObject.SetActive(false);

        isOpen = false;
        isAnimating = false;
    }
}