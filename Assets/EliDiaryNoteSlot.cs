using System;
using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EliDiaryNoteSlot : MonoBehaviour, IPointerClickHandler
{
    public static event Action<string> NoteUnlocked;

    [SerializeField] private string noteId;
    [SerializeField] private bool initiallyUnlocked;
    [SerializeField] private GameObject noteBackground;
    [SerializeField] private TMP_Text noteName;
    [SerializeField] private GameObject lockIcon;
    [SerializeField] private CanvasGroup selectedGlow;
    [SerializeField] private GameObject unreadMark;
    [SerializeField] private Image emittedLight;
    [SerializeField] private string titleRu;
    [SerializeField] private string titleEn;
    [SerializeField, TextArea] private string contentRu;
    [SerializeField, TextArea] private string contentEn;
    [SerializeField] private Sprite contentBackground;
    [SerializeField] private float glowDuration = 0.2f;

    private Tween selectedTween;
    private Tween unreadMarkTween;
    private Tween emittedLightTween;
    private Vector3 unreadMarkScale;
    private Vector2 emittedLightPosition;

    public event Action<EliDiaryNoteSlot> Selected;

    public string Title => LanguageManager.CurrentLanguage == Language.English
        ? titleEn
        : titleRu;
    public string Content => LanguageManager.CurrentLanguage == Language.English
        ? contentEn
        : contentRu;
    public Sprite ContentBackground => contentBackground;

    private string UnlockedKey => "ELI_DIARY_UNLOCKED_" + noteId;
    private string UnreadKey => "ELI_DIARY_UNREAD_" + noteId;

    private void Awake()
    {
        if (unreadMark != null)
            unreadMarkScale = unreadMark.transform.localScale;
        if (emittedLight != null)
            emittedLightPosition = emittedLight.rectTransform.anchoredPosition;
    }

    private void OnEnable()
    {
        RefreshAvailability();
    }

    private void OnDisable()
    {
        selectedTween?.Kill();
        StopUnreadAnimation();
    }

    public static bool UnlockFromStory(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            Debug.LogWarning("EliDiaryNoteSlot: story unlock note ID is empty.");
            return false;
        }

        string unlockedKey = "ELI_DIARY_UNLOCKED_" + id;
        if (PlayerPrefs.GetInt(unlockedKey, 0) != 0)
            return false;

        PlayerPrefs.SetInt(unlockedKey, 1);
        PlayerPrefs.SetInt("ELI_DIARY_UNREAD_" + id, 1);
        PlayerPrefs.Save();
        NoteUnlocked?.Invoke(id);
        return true;
    }

    public void RefreshAvailability()
    {
        bool unlocked = initiallyUnlocked || PlayerPrefs.GetInt(UnlockedKey, 0) != 0;
        bool unread = !initiallyUnlocked && PlayerPrefs.GetInt(UnreadKey, 0) != 0;

        if (noteBackground != null)
            noteBackground.SetActive(unlocked);
        if (noteName != null)
        {
            noteName.gameObject.SetActive(unlocked);
            noteName.text = LanguageManager.CurrentLanguage == Language.English
                ? titleEn
                : titleRu;
        }
        if (lockIcon != null)
            lockIcon.SetActive(!unlocked);
        if (unreadMark != null)
            unreadMark.SetActive(unlocked && unread);
        if (emittedLight != null)
            emittedLight.gameObject.SetActive(unlocked && unread);

        if (!unlocked)
            SetSelected(false);
        if (unlocked && unread)
            StartUnreadAnimation();
        else
            StopUnreadAnimation();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        SelectNote();
    }

    public void SelectNote()
    {
        if (!IsUnlocked())
            return;

        PlayerPrefs.SetInt(UnreadKey, 0);
        PlayerPrefs.Save();
        RefreshAvailability();
        Selected?.Invoke(this);
    }

    public void SetSelected(bool selected, bool animate = true)
    {
        if (selectedGlow == null)
            return;

        selectedTween?.Kill();
        if (!animate)
        {
            selectedGlow.alpha = selected ? 1f : 0f;
            selectedGlow.gameObject.SetActive(selected);
            return;
        }

        selectedGlow.gameObject.SetActive(selected || selectedGlow.alpha > 0f);
        selectedTween = selectedGlow.DOFade(selected ? 1f : 0f, glowDuration)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                if (!selected)
                    selectedGlow.gameObject.SetActive(false);
            });
    }

    private bool IsUnlocked()
    {
        return initiallyUnlocked || PlayerPrefs.GetInt(UnlockedKey, 0) != 0;
    }

    private void StartUnreadAnimation()
    {
        if (unreadMark != null && unreadMarkTween == null)
        {
            unreadMark.transform.localScale = unreadMarkScale;
            unreadMarkTween = unreadMark.transform.DOScale(unreadMarkScale * 1.12f, 0.7f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetUpdate(true);
        }

        if (emittedLight == null || emittedLightTween != null)
            return;

        emittedLight.rectTransform.anchoredPosition = emittedLightPosition;
        Color color = emittedLight.color;
        emittedLight.color = new Color(color.r, color.g, color.b, 0.65f);
        Sequence lightSequence = DOTween.Sequence().SetUpdate(true).SetLoops(-1, LoopType.Restart);
        lightSequence.Append(emittedLight.rectTransform.DOAnchorPos(emittedLightPosition + new Vector2(0f, 18f), 0.65f));
        lightSequence.Join(emittedLight.DOFade(0f, 0.65f));
        lightSequence.OnStepComplete(() =>
        {
            if (emittedLight != null)
            {
                emittedLight.rectTransform.anchoredPosition = emittedLightPosition;
                emittedLight.color = new Color(color.r, color.g, color.b, 0.65f);
            }
        });
        emittedLightTween = lightSequence;
    }

    private void StopUnreadAnimation()
    {
        unreadMarkTween?.Kill();
        unreadMarkTween = null;
        emittedLightTween?.Kill();
        emittedLightTween = null;
        if (unreadMark != null)
            unreadMark.transform.localScale = unreadMarkScale;
        if (emittedLight != null)
        {
            emittedLight.rectTransform.anchoredPosition = emittedLightPosition;
            emittedLight.color = new Color(emittedLight.color.r, emittedLight.color.g, emittedLight.color.b, 0f);
        }
    }
}
