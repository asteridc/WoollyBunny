using DG.Tweening;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum CollectibleCategory
{
    FamilyLetters,
    WorldLandmarks,
    DocumentsAndInstructions
}

public class CollectibleCategoriesController : MonoBehaviour
{
    [Serializable]
    private class Category
    {
        [Header("Category")]
        public string titleRu;

        public string titleEn;

        public CollectibleCategory category;

        public GameObject categoryObject;

        public RectTransform visual;

        public CanvasGroup glow;

        [Header("Content")]
        public ScrollRect scrollRect;

        public CanvasGroup contentCanvasGroup;

        [Header("Initial Collectibles")]
        public List<CollectibleItemBase> initiallyUnlockedCollectibles;

        [Header("Collected Count")]
        public TMP_Text collectedCountText;

        [Header("Animation")]
        public float selectedOffset = 10f;
    }

    [Header("Categories")]
    [SerializeField] private Category[] categories;

    [Header("Current Category Title")]
    [SerializeField] private TMP_Text categoryTitle;

    [Header("Animation")]
    [SerializeField] private float selectionDuration = 0.2f;
    [SerializeField] private Ease selectionEase = Ease.OutCubic;

    [SerializeField] private float glowDuration = 0.2f;

    [Header("Content Animation")]
    [SerializeField] private bool animateContent = false;
    [SerializeField] private float contentFadeDuration = 0.15f;

    private int currentCategoryIndex = -1;

    private void Awake()
    {
        InitializeCategories();
    }

    private void Start()
    {
        RegisterInitialCollectibles();

        if (categories == null || categories.Length == 0)
            return;

        SelectCategory(0, false);

        RefreshAllCategoryCounts();
        RefreshCategoryViews();
    }

    private void OnEnable()
    {
        if (CollectibleManager.Instance != null)
        {
            CollectibleManager.Instance.CollectibleAdded -=
                OnCollectibleAdded;

            CollectibleManager.Instance.CollectibleAdded +=
                OnCollectibleAdded;
        }

        RefreshAllCategoryCounts();
    }

    private void OnDisable()
    {
        if (CollectibleManager.Instance != null)
        {
            CollectibleManager.Instance.CollectibleAdded -=
                OnCollectibleAdded;
        }
    }

    public void RefreshAllCategoryCounts()
    {
        if (CollectibleManager.Instance == null)
            return;

        if (categories == null)
            return;

        for (int i = 0; i < categories.Length; i++)
        {
            Category category = categories[i];

            if (category == null ||
                category.collectedCountText == null)
            {
                continue;
            }

            int count =
                CollectibleManager.Instance.GetCollectedCount(
                    category.category
                );

            category.collectedCountText.text =
                count.ToString();
        }
    }

    private void RefreshCategoryViews()
    {
        CollectibleCategoryView[] views =
            FindObjectsByType<CollectibleCategoryView>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

        foreach (CollectibleCategoryView view in views)
        {
            view.Refresh();
        }
    }

    private void OnCollectibleAdded(
    CollectibleItemBase collectible)
    {
        RefreshAllCategoryCounts();
    }

    private void RegisterInitialCollectibles()
    {
        if (CollectibleManager.Instance == null)
        {
            Debug.LogWarning(
                "CollectibleCategoriesController: " +
                "CollectibleManager.Instance не найден.",
                this
            );

            return;
        }

        if (categories == null)
            return;

        for (int i = 0; i < categories.Length; i++)
        {
            Category category = categories[i];

            if (category == null ||
                category.initiallyUnlockedCollectibles == null)
            {
                continue;
            }

            for (
                int j = 0;
                j < category.initiallyUnlockedCollectibles.Count;
                j++
            )
            {
                CollectibleItemBase collectible =
                    category.initiallyUnlockedCollectibles[j];

                if (collectible == null)
                    continue;

                if (collectible.Category != category.category)
                {
                    Debug.LogWarning(
                        $"Collectible '{collectible.name}' " +
                        $"имеет категорию {collectible.Category}, " +
                        $"но находится в категории {category.category}.",
                        collectible
                    );

                    continue;
                }

                CollectibleManager.Instance
                    .RegisterCollectible(collectible);
            }
        }
    }

    /// <summary>
    /// Подготавливает категории к работе.
    /// </summary>
    private void InitializeCategories()
    {
        if (categories == null)
            return;

        for (int i = 0; i < categories.Length; i++)
        {
            Category category = categories[i];

            if (category == null)
                continue;

            // Запоминаем исходную позицию визуала.
            if (category.visual != null)
            {
                category.visual.DOKill();
            }

            // Все категории изначально неактивны.
            if (category.scrollRect != null)
            {
                category.scrollRect.gameObject.SetActive(false);
            }

            // Glow выключен.
            if (category.glow != null)
            {
                category.glow.DOKill();
                category.glow.alpha = 0f;
            }

            // CanvasGroup контента.
            if (category.contentCanvasGroup != null)
            {
                category.contentCanvasGroup.DOKill();
                category.contentCanvasGroup.alpha = 0f;
                category.contentCanvasGroup.interactable = false;
                category.contentCanvasGroup.blocksRaycasts = false;
            }
        }
    }

    /// <summary>
    /// Выбирает категорию по индексу.
    /// </summary>
    public void SelectCategory(int index)
    {
        SelectCategory(index, true);
    }

    private void SelectCategory(int index, bool animate)
    {
        if (categories == null ||
            categories.Length == 0)
        {
            return;
        }

        if (index < 0 || index >= categories.Length)
        {
            Debug.LogWarning(
                $"CollectibleCategoriesController: invalid category index {index}.",
                this
            );

            return;
        }

        if (currentCategoryIndex == index)
            return;

        int previousIndex = currentCategoryIndex;
        currentCategoryIndex = index;

        // Сначала снимаем выделение с предыдущей категории.
        if (previousIndex >= 0)
        {
            SetCategorySelected(
                categories[previousIndex],
                false,
                animate
            );
        }

        // Скрываем содержимое предыдущих категорий.
        HideAllContents(index, animate);

        Category selectedCategory = categories[index];

        // Показываем нужный ScrollRect.
        ShowCategoryContent(selectedCategory, animate);

        // Меняем название справа.
        UpdateCategoryTitle(selectedCategory);

        // Выделяем новую категорию.
        SetCategorySelected(
            selectedCategory,
            true,
            animate
        );
    }

    /// <summary>
    /// Переключает визуальное состояние категории.
    /// </summary>
    private void SetCategorySelected(
        Category category,
        bool selected,
        bool animate)
    {
        if (category == null)
            return;

        // ---------------------------------------------------------
        // VISUAL POSITION
        // ---------------------------------------------------------

        if (category.visual != null)
        {
            category.visual.DOKill();

            float targetX = selected
                ? -category.selectedOffset
                : 0f;

            if (animate)
            {
                category.visual
                    .DOAnchorPosX(
                        targetX,
                        selectionDuration
                    )
                    .SetEase(selectionEase)
                    .SetUpdate(true);
            }
            else
            {
                Vector2 position = category.visual.anchoredPosition;
                position.x = targetX;
                category.visual.anchoredPosition = position;
            }
        }

        // ---------------------------------------------------------
        // GLOW
        // ---------------------------------------------------------

        if (category.glow != null)
        {
            category.glow.DOKill();

            float targetAlpha = selected ? 1f : 0f;

            if (animate)
            {
                category.glow
                    .DOFade(
                        targetAlpha,
                        glowDuration
                    )
                    .SetEase(Ease.OutQuad)
                    .SetUpdate(true);
            }
            else
            {
                category.glow.alpha = targetAlpha;
            }
        }
    }

    /// <summary>
    /// Выключает все ScrollRect кроме выбранного.
    /// </summary>
    private void HideAllContents(int selectedIndex, bool animate)
    {
        for (int i = 0; i < categories.Length; i++)
        {
            if (i == selectedIndex)
                continue;

            Category category = categories[i];

            if (category == null)
                continue;

            HideCategoryContent(category, animate);
        }
    }

    private void HideCategoryContent(
        Category category,
        bool animate)
    {
        if (category == null)
            return;

        if (category.contentCanvasGroup != null &&
            animateContent &&
            category.scrollRect != null &&
            category.scrollRect.gameObject.activeSelf)
        {
            CanvasGroup canvasGroup = category.contentCanvasGroup;

            canvasGroup.DOKill();

            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            canvasGroup
                .DOFade(0f, contentFadeDuration)
                .SetEase(Ease.InQuad)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    if (category.scrollRect != null)
                        category.scrollRect.gameObject.SetActive(false);
                });

            return;
        }

        if (category.scrollRect != null)
        {
            category.scrollRect.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Показывает ScrollRect выбранной категории.
    /// </summary>
    private void ShowCategoryContent(
        Category category,
        bool animate)
    {
        if (category == null ||
            category.scrollRect == null)
        {
            return;
        }

        GameObject scrollObject =
            category.scrollRect.gameObject;

        scrollObject.SetActive(true);

        if (category.contentCanvasGroup != null &&
            animateContent)
        {
            CanvasGroup canvasGroup =
                category.contentCanvasGroup;

            canvasGroup.DOKill();

            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            canvasGroup
                .DOFade(1f, contentFadeDuration)
                .SetEase(Ease.OutQuad)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    canvasGroup.interactable = true;
                    canvasGroup.blocksRaycasts = true;
                });
        }
        else if (category.contentCanvasGroup != null)
        {
            category.contentCanvasGroup.alpha = 1f;
            category.contentCanvasGroup.interactable = true;
            category.contentCanvasGroup.blocksRaycasts = true;
        }

        // При переключении категории начинаем сверху.
        category.scrollRect.verticalNormalizedPosition = 1f;
        category.scrollRect.horizontalNormalizedPosition = 0f;
    }

    /// <summary>
    /// Меняет название текущей категории справа.
    /// </summary>
    private void UpdateCategoryTitle(Category category)
    {
        if (categoryTitle == null ||
            category == null)
        {
            return;
        }

        if (LanguageManager.CurrentLanguage == Language.English)
        {
            categoryTitle.text = category.titleEn;
            return;
        }
        else
        {
            categoryTitle.text = category.titleRu;
        }
    }
}