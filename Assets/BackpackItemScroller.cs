using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;

public class BackpackItemScroller : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform contentContainer;

    [Header("Section Headers")]
    [SerializeField] private RectTransform availableHeader;
    [SerializeField] private RectTransform unavailableHeader;

    [Header("Item Groups")]
    [SerializeField] private RectTransform availableItemsContainer;
    [SerializeField] private RectTransform unavailableItemsContainer;

    [Header("Settings")]
    [SerializeField] private float headerTransitionSpeed = 0.2f;

    private CanvasGroup availableHeaderCanvas;
    private CanvasGroup unavailableHeaderCanvas;
    private float availableItemsHeight;
    private float unavailableItemsHeight;

    private void Start()
    {
        if (scrollRect == null)
        {
            enabled = false;
            return;
        }

        // Получаем или добавляем CanvasGroup у заголовков и отключаем блокировку лучей
        if (availableHeader != null)
        {
            availableHeaderCanvas = availableHeader.GetComponent<CanvasGroup>();
            if (availableHeaderCanvas == null)
                availableHeaderCanvas = availableHeader.gameObject.AddComponent<CanvasGroup>();
            availableHeaderCanvas.blocksRaycasts = false; // Заголовок не должен мешать кликам по предметам
        }

        if (unavailableHeader != null)
        {
            unavailableHeaderCanvas = unavailableHeader.GetComponent<CanvasGroup>();
            if (unavailableHeaderCanvas == null)
                unavailableHeaderCanvas = unavailableHeader.gameObject.AddComponent<CanvasGroup>();
            unavailableHeaderCanvas.blocksRaycasts = false; // Заголовок не должен мешать кликам по предметам
        }

        scrollRect.onValueChanged.AddListener(OnScroll);
        SetHeaderVisibility(true, false);
    }

    private void OnDestroy()
    {
        if (scrollRect != null)
            scrollRect.onValueChanged.RemoveListener(OnScroll);
    }

    private void OnScroll(Vector2 scrollPosition)
    {
        // Проверяем необходимые ссылки, чтобы не было NullReferenceException
        if (scrollRect == null || scrollRect.content == null || scrollRect.viewport == null)
            return;

        UpdateStickyHeadersVisibility();
    }

    private void UpdateStickyHeadersVisibility()
    {
        if (availableHeader == null || unavailableHeader == null)
            return;

        if (scrollRect.content == null || scrollRect.viewport == null)
            return;

        if (availableItemsContainer == null || unavailableItemsContainer == null)
            return;

        float contentTop = scrollRect.content.rect.yMax;
        float contentBottom = scrollRect.content.rect.yMin;
        float viewportTop = scrollRect.viewport.rect.yMax;
        float viewportBottom = scrollRect.viewport.rect.yMin;

        if (availableItemsContainer != null)
            availableItemsHeight = availableItemsContainer.rect.height;
        if (unavailableItemsContainer != null)
            unavailableItemsHeight = unavailableItemsContainer.rect.height;

        float availableTopInContent = availableItemsContainer.anchoredPosition.y;
        float availableBottomInContent = availableTopInContent - availableItemsHeight;

        float unavailableTopInContent = unavailableItemsContainer.anchoredPosition.y;
        float unavailableBottomInContent = unavailableTopInContent - unavailableItemsHeight;

        float scrollY = scrollRect.content.anchoredPosition.y;

        bool showAvailable = scrollY < availableTopInContent + availableItemsHeight;
        bool showUnavailable = scrollY > availableTopInContent + availableItemsHeight - 100f ||
                              scrollY > unavailableTopInContent - availableItemsHeight;

        SetHeaderVisibility(showAvailable, showUnavailable);
    }

    private void SetHeaderVisibility(bool showAvailable, bool showUnavailable)
    {
        if (availableHeaderCanvas != null)
        {
            availableHeaderCanvas.DOKill();
            availableHeaderCanvas.DOFade(showAvailable ? 1f : 0f, headerTransitionSpeed).SetUpdate(true);
            availableHeader.gameObject.SetActive(showAvailable);
            availableHeaderCanvas.blocksRaycasts = false; // Гарантируем, что заголовок не блокирует
        }

        if (unavailableHeaderCanvas != null)
        {
            unavailableHeaderCanvas.DOKill();
            unavailableHeaderCanvas.DOFade(showUnavailable ? 1f : 0f, headerTransitionSpeed).SetUpdate(true);
            unavailableHeader.gameObject.SetActive(showUnavailable);
            unavailableHeaderCanvas.blocksRaycasts = false; // Гарантируем, что заголовок не блокирует
        }
    }

    public void ResetScroll()
    {
        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 1f;
            SetHeaderVisibility(true, false);
        }
    }

    public void SetItemsData(ItemType itemType)
    {
        ResetScroll();
    }
}