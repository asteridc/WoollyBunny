using DG.Tweening;
using UnityEngine;

public class BackpackSectionController : MonoBehaviour
{
    public static BackpackSectionController Instance;

    [Header("Input")]
    [SerializeField] private GameObject backpackRoot;

    [Header("Sections")]
    [Tooltip("CanvasGroup секций в том же порядке, что и вкладки.")]
    [SerializeField] private CanvasGroup[] sections;

    [Header("Tabs")]
    [Tooltip("Кнопки вкладок в том же порядке, что и секции.")]
    [SerializeField] private BackpackTabButton[] tabs;

    [Header("Initial Section")]
    [Tooltip("0 — Дневник, 1 — Коллекции, 2 — Вещи, 3 - Крафтинг, 4 - Сюжет")]
    [SerializeField] private int defaultSectionIndex = 2;

    [Header("Animation")]
    [SerializeField] private float switchDuration = 0.18f;
    [SerializeField] private float slideDistance = 20f;
    [SerializeField] private Ease hideEase = Ease.InCubic;
    [SerializeField] private Ease showEase = Ease.OutCubic;

    private RectTransform[] sectionRects;
    private Vector2[] shownPositions;

    private int currentSectionIndex = -1;
    private bool isSwitching;

    public int CurrentSectionIndex => currentSectionIndex;
    public bool IsSwitching => isSwitching;

    private void Awake()
    {
        Instance = this;
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

            // Все секции остаются активными.
            // Скрытые секции просто не видны и не принимают ввод.
            section.gameObject.SetActive(true);

            bool isDefault = i == defaultSectionIndex;

            section.alpha = isDefault ? 1f : 0f;
            section.interactable = isDefault;
            section.blocksRaycasts = isDefault;

            sectionRect.anchoredPosition = shownPositions[i];

            tabs[i].Initialize(this, i);
            tabs[i].SetSelected(isDefault, true);
        }

        currentSectionIndex = defaultSectionIndex;
    }

    private void Update()
    {
        if (isSwitching)
            return;

        if (backpackRoot == null || !backpackRoot.activeInHierarchy)
            return;

        if (Input.GetKeyDown(KeyCode.Q))
        {
            SelectPreviousSection();
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            SelectNextSection();
        }
    }

    private void SelectPreviousSection()
    {
        int previousIndex = currentSectionIndex - 1;

        if (previousIndex < 0)
            previousIndex = sections.Length - 1;

        SelectSection(previousIndex);
    }

    private void SelectNextSection()
    {
        int nextIndex = currentSectionIndex + 1;

        if (nextIndex >= sections.Length)
            nextIndex = 0;

        SelectSection(nextIndex);
    }

    public void OpenItemsSection()
    {
        SelectSectionInstant(defaultSectionIndex);
    }

    public void OpenCollectiblesSection()
    {
        OpenItemsSection();
        SelectSectionInstant(1);
    }

    public void SelectSection(int targetIndex)
    {
        if (isSwitching)
            return;

        if (targetIndex < 0 || targetIndex >= sections.Length)
        {
            Debug.LogWarning(
                $"BackpackSectionController: индекс секции {targetIndex} вне диапазона.",
                this);

            return;
        }

        // При любом переключении секции закрываем
        // открытый Collectible Overview.
        HideCollectibleOverview();

        if (targetIndex == currentSectionIndex)
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
            shownPositions[previousIndex] +
            Vector2.left * slideDistance * direction;

        Vector2 targetStartPosition =
            shownPositions[targetIndex] +
            Vector2.right * slideDistance * direction;

        previousSection.interactable = false;
        previousSection.blocksRaycasts = false;

        targetSection.alpha = 0f;
        targetSection.interactable = false;
        targetSection.blocksRaycasts = false;
        targetRect.anchoredPosition = targetStartPosition;

        for (int i = 0; i < tabs.Length; i++)
            tabs[i].SetSelected(i == targetIndex);

        Sequence sequence = DOTween.Sequence()
            .SetUpdate(true);

        sequence.Join(
            previousSection
                .DOFade(0f, switchDuration)
                .SetEase(hideEase));

        sequence.Join(
            previousRect
                .DOAnchorPos(previousHiddenPosition, switchDuration)
                .SetEase(hideEase));

        sequence.Join(
            targetSection
                .DOFade(1f, switchDuration)
                .SetEase(showEase));

        sequence.Join(
            targetRect
                .DOAnchorPos(shownPositions[targetIndex], switchDuration)
                .SetEase(showEase));

        sequence.OnComplete(() =>
        {
            previousRect.anchoredPosition = shownPositions[previousIndex];
            previousSection.alpha = 0f;

            targetRect.anchoredPosition = shownPositions[targetIndex];
            targetSection.alpha = 1f;
            targetSection.interactable = true;
            targetSection.blocksRaycasts = true;

            currentSectionIndex = targetIndex;
            isSwitching = false;

            SyncCollectiblesPanelState(currentSectionIndex);
        });
    }

    public void SelectSectionInstant(int targetIndex)
    {
        if (targetIndex < 0 || targetIndex >= sections.Length)
            return;

        HideCollectibleOverview();

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

        SyncCollectiblesPanelState(currentSectionIndex);
    }

    private void KillSectionTweens(int index)
    {
        sections[index].DOKill();
        sectionRects[index].DOKill();
    }

    private void HideCollectibleOverview()
    {
        if (BackpackCollectiblesPanel.Instance == null)
            return;

        BackpackCollectiblesPanel.Instance.CloseCollectibleOverview();
    }

    private bool ValidateReferences()
    {
        if (sections == null || sections.Length == 0)
        {
            Debug.LogError(
                "BackpackSectionController: массив Sections не заполнен.",
                this);

            return false;
        }

        if (tabs == null || tabs.Length == 0)
        {
            Debug.LogError(
                "BackpackSectionController: массив Tabs не заполнен.",
                this);

            return false;
        }

        if (sections.Length != tabs.Length)
        {
            Debug.LogError(
                "BackpackSectionController: количество секций и вкладок должно совпадать.",
                this);

            return false;
        }

        if (defaultSectionIndex < 0 ||
            defaultSectionIndex >= sections.Length)
        {
            Debug.LogError(
                "BackpackSectionController: Default Section Index вне диапазона.",
                this);

            return false;
        }

        for (int i = 0; i < sections.Length; i++)
        {
            if (sections[i] == null)
            {
                Debug.LogError(
                    $"BackpackSectionController: секция с индексом {i} не назначена.",
                    this);

                return false;
            }

            if (tabs[i] == null)
            {
                Debug.LogError(
                    $"BackpackSectionController: вкладка с индексом {i} не назначена.",
                    this);

                return false;
            }
        }

        return true;
    }


    private void SyncCollectiblesPanelState(int sectionIndex)
    {
        if (BackpackCollectiblesPanel.Instance == null)
            return;

        BackpackCollectiblesPanel.Instance.SetSectionOpenState(sectionIndex == 1);
    }
}