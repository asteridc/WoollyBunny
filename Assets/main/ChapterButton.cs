using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ChapterButton : MonoBehaviour
{
    [Tooltip("Название сцены для этой главы")]
    public string sceneName;

    [SerializeField] private float lockedScaleMultiplier = 0.94f;
    [SerializeField] private float lockedCanvasAlpha = 0.62f;
    [SerializeField] private string lockGlyph = "🔒";

    private Button button;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private readonly List<Graphic> cachedGraphics = new List<Graphic>();
    private readonly Dictionary<Graphic, Color> baseColors = new Dictionary<Graphic, Color>();
    private Vector3 baseScale = Vector3.one;
    private TextMeshProUGUI lockLabel;

    private void Awake()
    {
        button = GetComponent<Button>();
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        if (rectTransform != null)
            baseScale = rectTransform.localScale;

        CacheGraphics();
        EnsureLockLabel();
        RefreshState();
    }

    private void OnEnable()
    {
        RefreshState();
    }

    private void Start()
    {
        RefreshState();
    }

    public void OnClick()
    {
        if (!IsUnlocked())
            return;

        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogWarning("Scene name is empty on " + name);
        }
    }

    private void RefreshState()
    {
        bool unlocked = IsUnlocked();

        if (button != null)
            button.interactable = unlocked;

        if (canvasGroup != null)
        {
            canvasGroup.alpha = unlocked ? 1f : lockedCanvasAlpha;
            canvasGroup.interactable = unlocked;
            canvasGroup.blocksRaycasts = unlocked;
        }

        if (rectTransform != null)
            rectTransform.localScale = unlocked ? baseScale : baseScale * lockedScaleMultiplier;

        ApplyGraphicTint(unlocked);

        if (lockLabel != null)
            lockLabel.gameObject.SetActive(!unlocked);
    }

    private bool IsUnlocked()
    {
        if (!TryGetChapterNumber(sceneName, out int chapterNumber))
            return true;

        if (SaveManager.Instance == null)
            return chapterNumber <= 1;

        return SaveManager.Instance.IsChapterUnlocked(chapterNumber);
    }

    private void CacheGraphics()
    {
        cachedGraphics.Clear();
        baseColors.Clear();

        Graphic[] graphics = GetComponentsInChildren<Graphic>(true);
        foreach (Graphic graphic in graphics)
        {
            if (graphic == null)
                continue;

            cachedGraphics.Add(graphic);
            baseColors[graphic] = graphic.color;
        }
    }

    private void ApplyGraphicTint(bool unlocked)
    {
        foreach (Graphic graphic in cachedGraphics)
        {
            if (graphic == null || graphic == lockLabel)
                continue;

            if (!baseColors.TryGetValue(graphic, out Color baseColor))
                continue;

            graphic.color = unlocked ? baseColor : ToLockedColor(baseColor);
        }
    }

    private Color ToLockedColor(Color source)
    {
        float grayscale = source.r * 0.299f + source.g * 0.587f + source.b * 0.114f;
        return new Color(grayscale, grayscale, grayscale, source.a);
    }

    private void EnsureLockLabel()
    {
        if (lockLabel != null)
            return;

        GameObject lockObject = new GameObject("LockOverlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        lockObject.transform.SetParent(transform, false);
        lockObject.transform.SetAsLastSibling();

        RectTransform lockRect = lockObject.GetComponent<RectTransform>();
        lockRect.anchorMin = new Vector2(0.5f, 0.5f);
        lockRect.anchorMax = new Vector2(0.5f, 0.5f);
        lockRect.pivot = new Vector2(0.5f, 0.5f);
        lockRect.anchoredPosition = new Vector2(0f, 0f);
        lockRect.sizeDelta = new Vector2(140f, 140f);

        lockLabel = lockObject.GetComponent<TextMeshProUGUI>();
        lockLabel.text = lockGlyph;
        lockLabel.alignment = TextAlignmentOptions.Center;
        lockLabel.fontSize = 72f;
        lockLabel.color = Color.white;
        lockLabel.enableWordWrapping = false;
        lockLabel.overflowMode = TextOverflowModes.Overflow;
        lockLabel.raycastTarget = false;
        lockLabel.gameObject.SetActive(false);

        TextMeshProUGUI referenceText = GetReferenceText();
        if (referenceText != null)
        {
            lockLabel.font = referenceText.font;
            lockLabel.fontSharedMaterial = referenceText.fontSharedMaterial;
        }
    }

    private TextMeshProUGUI GetReferenceText()
    {
        TextMeshProUGUI[] texts = GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (TextMeshProUGUI text in texts)
        {
            if (text != null && text != lockLabel)
                return text;
        }

        return null;
    }

    private bool TryGetChapterNumber(string value, out int chapterNumber)
    {
        chapterNumber = 0;

        if (string.IsNullOrWhiteSpace(value))
            return false;

        int end = value.Length - 1;
        while (end >= 0 && char.IsDigit(value[end]))
            end--;

        if (end == value.Length - 1)
            return false;

        string digits = value.Substring(end + 1);
        return int.TryParse(digits, out chapterNumber) && chapterNumber > 0;
    }
}
