using DG.Tweening;
using UnityEngine;

public class SectionSwitcher : MonoBehaviour
{
    [Header("Sections")]
    [SerializeField] private CanvasGroup[] sections;
    [SerializeField] private GameObject russianTabsRoot;
    [SerializeField] private GameObject englishTabsRoot;
    [SerializeField] private SectionTabButton[] russianTabs;
    [SerializeField] private SectionTabButton[] englishTabs;
    [SerializeField] private int defaultSectionIndex;

    [Header("Animation")]
    [SerializeField] private float switchDuration = 0.18f;
    [SerializeField] private float slideDistance = 20f;
    [SerializeField] private Ease hideEase = Ease.InCubic;
    [SerializeField] private Ease showEase = Ease.OutCubic;

    private RectTransform[] sectionRects;
    private Vector2[] shownPositions;
    private SectionTabButton[] tabs;
    private int currentSectionIndex = -1;
    private bool isSwitching;

    public int CurrentSectionIndex => currentSectionIndex;
    public bool IsSwitching => isSwitching;

    private void Awake()
    {
        SetupLanguageTabs();

        if (!ValidateReferences())
        {
            enabled = false;
            return;
        }

        sectionRects = new RectTransform[sections.Length];
        shownPositions = new Vector2[sections.Length];

        for (int i = 0; i < sections.Length; i++)
        {
            CanvasGroup section = sections[i];
            RectTransform sectionRect = section.GetComponent<RectTransform>();

            sectionRects[i] = sectionRect;
            shownPositions[i] = sectionRect.anchoredPosition;
            section.gameObject.SetActive(true);

            bool selected = i == defaultSectionIndex;
            section.alpha = selected ? 1f : 0f;
            section.interactable = selected;
            section.blocksRaycasts = selected;

            tabs[i].Initialize(this, i);
            tabs[i].SetSelected(selected, true);
        }

        currentSectionIndex = defaultSectionIndex;
    }

    private void OnEnable()
    {
        LanguageManager.OnLanguageChanged += OnLanguageChanged;
    }

    private void OnDisable()
    {
        LanguageManager.OnLanguageChanged -= OnLanguageChanged;
    }

    private void SetupLanguageTabs()
    {
        bool isEnglish = LanguageManager.CurrentLanguage == Language.English;

        if (russianTabsRoot != null)
            russianTabsRoot.SetActive(!isEnglish);

        if (englishTabsRoot != null)
            englishTabsRoot.SetActive(isEnglish);

        tabs = isEnglish ? englishTabs : russianTabs;
    }

    private void OnLanguageChanged(Language language)
    {
        SetupLanguageTabs();

        if (tabs == null || sections == null || tabs.Length != sections.Length)
        {
            Debug.LogError("SectionSwitcher: число вкладок выбранного языка должно совпадать с числом секций.", this);
            return;
        }

        for (int i = 0; i < tabs.Length; i++)
        {
            if (tabs[i] == null)
            {
                Debug.LogError($"SectionSwitcher: отсутствует вкладка выбранного языка с индексом {i}.", this);
                return;
            }

            tabs[i].Initialize(this, i);
            tabs[i].SetSelected(i == currentSectionIndex, true);
        }
    }

    public void SelectSection(int targetIndex)
    {
        if (isSwitching || targetIndex < 0 || targetIndex >= sections.Length ||
            targetIndex == currentSectionIndex)
            return;

        int previousIndex = currentSectionIndex;
        CanvasGroup previousSection = sections[previousIndex];
        CanvasGroup targetSection = sections[targetIndex];
        RectTransform previousRect = sectionRects[previousIndex];
        RectTransform targetRect = sectionRects[targetIndex];

        isSwitching = true;
        KillSectionTweens(previousIndex);
        KillSectionTweens(targetIndex);

        float direction = targetIndex > previousIndex ? 1f : -1f;
        Vector2 previousHiddenPosition =
            shownPositions[previousIndex] + Vector2.left * slideDistance * direction;
        Vector2 targetStartPosition =
            shownPositions[targetIndex] + Vector2.right * slideDistance * direction;

        previousSection.interactable = false;
        previousSection.blocksRaycasts = false;
        targetSection.alpha = 0f;
        targetSection.interactable = false;
        targetSection.blocksRaycasts = false;
        targetRect.anchoredPosition = targetStartPosition;

        for (int i = 0; i < tabs.Length; i++)
            tabs[i].SetSelected(i == targetIndex);

        Sequence sequence = DOTween.Sequence().SetUpdate(true);
        sequence.Join(previousSection.DOFade(0f, switchDuration).SetEase(hideEase));
        sequence.Join(previousRect.DOAnchorPos(previousHiddenPosition, switchDuration).SetEase(hideEase));
        sequence.Join(targetSection.DOFade(1f, switchDuration).SetEase(showEase));
        sequence.Join(targetRect.DOAnchorPos(shownPositions[targetIndex], switchDuration).SetEase(showEase));

        sequence.OnComplete(() =>
        {
            previousRect.anchoredPosition = shownPositions[previousIndex];
            previousSection.alpha = 0f;
            targetRect.anchoredPosition = shownPositions[targetIndex];
            targetSection.alpha = 1f;
            targetSection.interactable = true;
            targetSection.blocksRaycasts = true;
            currentSectionIndex = targetIndex;

            for (int i = 0; i < tabs.Length; i++)
                tabs[i].SetSelected(i == targetIndex, true);

            isSwitching = false;
        });
    }

    public void SelectSectionInstant(int targetIndex)
    {
        if (targetIndex < 0 || targetIndex >= sections.Length)
            return;

        for (int i = 0; i < sections.Length; i++)
        {
            KillSectionTweens(i);

            bool selected = i == targetIndex;
            sections[i].alpha = selected ? 1f : 0f;
            sections[i].interactable = selected;
            sections[i].blocksRaycasts = selected;
            sectionRects[i].anchoredPosition = shownPositions[i];
            tabs[i].SetSelected(selected, true);
        }

        currentSectionIndex = targetIndex;
        isSwitching = false;
    }

    private bool ValidateReferences()
    {
        if (sections == null || sections.Length == 0 ||
            !AreTabsValid(russianTabs) || !AreTabsValid(englishTabs))
        {
            Debug.LogError("SectionSwitcher: число секций должно совпадать с числом русских и английских вкладок.", this);
            return false;
        }

        if (defaultSectionIndex < 0 || defaultSectionIndex >= sections.Length)
        {
            Debug.LogError("SectionSwitcher: индекс стартовой секции вне диапазона.", this);
            return false;
        }

        for (int i = 0; i < sections.Length; i++)
        {
            if (sections[i] == null || sections[i].GetComponent<RectTransform>() == null)
            {
                Debug.LogError($"SectionSwitcher: проверьте ссылку на секцию с индексом {i}.", this);
                return false;
            }
        }

        return true;
    }

    private bool AreTabsValid(SectionTabButton[] tabSet)
    {
        if (tabSet == null || tabSet.Length != sections.Length)
            return false;

        for (int i = 0; i < tabSet.Length; i++)
        {
            if (tabSet[i] == null)
                return false;
        }

        return true;
    }

    private void KillSectionTweens(int index)
    {
        sections[index].DOKill();
        sectionRects[index].DOKill();
    }
}
