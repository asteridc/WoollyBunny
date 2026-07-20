using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class ChapterButton : MonoBehaviour
{
    [Tooltip("Название сцены для этой главы")]
    public string sceneName;

    [Header("Lock Visuals")]
    [SerializeField] private Image lockImage;
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Sprite lockSprite;

    [SerializeField] private float lockedScaleMultiplier = 0.94f;
    [SerializeField] private float lockedCanvasAlpha = 0.5f;

    private Button button;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private readonly List<Graphic> cachedGraphics = new List<Graphic>();
    private readonly Dictionary<Graphic, Color> baseColors = new Dictionary<Graphic, Color>();
    private Vector3 baseScale = Vector3.one;

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
        RefreshLockImage(unlocked);
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
            if (graphic == null || graphic == lockImage)
                continue;

            cachedGraphics.Add(graphic);
            baseColors[graphic] = graphic.color;
        }
    }

    private void ApplyGraphicTint(bool unlocked)
    {
        foreach (Graphic graphic in cachedGraphics)
        {
            if (graphic == null)
                continue;

            if (!baseColors.TryGetValue(graphic, out Color baseColor))
                continue;

            graphic.color = unlocked ? baseColor : ToLockedColor(baseColor);
        }
    }

    private void RefreshLockImage(bool unlocked)
    {
        if (lockImage == null || text == null)
            return;

        if (unlocked || lockSprite == null || text == null)
        {
            lockImage.gameObject.SetActive(false);
            text.gameObject.SetActive(false);
            return;
        }

        lockImage.sprite = lockSprite;
        lockImage.color = Color.white;
        lockImage.gameObject.SetActive(true);
        lockImage.transform.SetAsLastSibling();
    }

    private Color ToLockedColor(Color source)
    {
        float grayscale = source.r * 0.299f + source.g * 0.587f + source.b * 0.114f;
        return new Color(grayscale, grayscale, grayscale, source.a);
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
