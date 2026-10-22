using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NewsBanner : MonoBehaviour
{
    [Serializable]
    public class NewsItem
    {
        public string id;
        public string russianTitle;
        [TextArea] public string russianDescription;
        public string englishTitle;
        [TextArea] public string englishDescription;
    }

    [Header("News List")]
    [SerializeField] private List<NewsItem> news = new List<NewsItem>();
    [SerializeField] private float rotateInterval = 10f;

    [Header("Banner")]
    [SerializeField] private Button bannerButton;
    [SerializeField] private TMP_Text bannerTitle;
    [SerializeField] private TMP_Text bannerDescription;

    [Header("Details Frame")]
    [SerializeField] private CanvasGroup detailsGroup;
    [SerializeField] private RectTransform detailsFrame;
    [SerializeField] private TMP_Text detailsTitle;
    [SerializeField] private TMP_Text detailsDescription;
    [SerializeField] private Button closeButton;
    [SerializeField] private float animationDuration = 0.2f;
    [SerializeField] private float hiddenScale = 0.96f;

    private int currentNewsIndex;
    private float nextRotationTime;
    private bool isDetailsOpen;
    private bool isDetailsAnimating;

    private void Awake()
    {
        if (bannerButton != null)
            bannerButton.onClick.AddListener(OpenCurrentNews);

        if (closeButton != null)
            closeButton.onClick.AddListener(CloseDetails);

        HideDetailsInstant();
        ShowCurrentNews();
        nextRotationTime = Time.unscaledTime + rotateInterval;
    }

    private void OnEnable()
    {
        LanguageManager.OnLanguageChanged += OnLanguageChanged;
    }

    private void OnDisable()
    {
        LanguageManager.OnLanguageChanged -= OnLanguageChanged;
    }

    private void OnDestroy()
    {
        if (bannerButton != null)
            bannerButton.onClick.RemoveListener(OpenCurrentNews);

        if (closeButton != null)
            closeButton.onClick.RemoveListener(CloseDetails);
    }

    private void Update()
    {
        if (isDetailsOpen || isDetailsAnimating || news.Count <= 1 ||
            Time.unscaledTime < nextRotationTime)
            return;

        currentNewsIndex = (currentNewsIndex + 1) % news.Count;
        ShowCurrentNews();
        nextRotationTime = Time.unscaledTime + rotateInterval;
    }

    public void AddNews(NewsItem item)
    {
        if (item == null)
        {
            Debug.LogWarning("NewsBanner: нельзя добавить пустую новость.", this);
            return;
        }

        news.Add(item);

        if (news.Count == 1)
        {
            currentNewsIndex = 0;
            ShowCurrentNews();
        }
    }

    public bool RemoveNews(string id)
    {
        int index = news.FindIndex(item => item != null && item.id == id);
        if (index < 0)
            return false;

        news.RemoveAt(index);
        currentNewsIndex = news.Count == 0 ? 0 : currentNewsIndex % news.Count;
        ShowCurrentNews();
        return true;
    }

    public void ClearNews()
    {
        news.Clear();
        currentNewsIndex = 0;
        ShowCurrentNews();
    }

    public void OpenCurrentNews()
    {
        if (isDetailsOpen || isDetailsAnimating || news.Count == 0 ||
            detailsGroup == null || detailsFrame == null)
            return;

        isDetailsOpen = true;
        isDetailsAnimating = true;
        NewsItem item = news[currentNewsIndex];
        SetLocalizedDetails(item, LanguageManager.CurrentLanguage);

        detailsGroup.DOKill();
        detailsFrame.DOKill();
        detailsGroup.gameObject.SetActive(true);
        detailsGroup.alpha = 0f;
        detailsFrame.localScale = Vector3.one * hiddenScale;
        detailsGroup.interactable = true;
        detailsGroup.blocksRaycasts = true;

        Sequence sequence = DOTween.Sequence().SetUpdate(true);
        sequence.Join(detailsGroup.DOFade(1f, animationDuration));
        sequence.Join(detailsFrame.DOScale(1f, animationDuration).SetEase(Ease.OutCubic));
        sequence.OnComplete(() => isDetailsAnimating = false);
    }

    public void CloseDetails()
    {
        if (!isDetailsOpen || isDetailsAnimating ||
            detailsGroup == null || detailsFrame == null)
            return;

        isDetailsOpen = false;
        isDetailsAnimating = true;
        detailsGroup.interactable = false;
        detailsGroup.blocksRaycasts = false;

        Sequence sequence = DOTween.Sequence().SetUpdate(true);
        sequence.Join(detailsGroup.DOFade(0f, animationDuration));
        sequence.Join(detailsFrame.DOScale(hiddenScale, animationDuration).SetEase(Ease.InCubic));
        sequence.OnComplete(() =>
        {
            HideDetailsInstant();
            isDetailsAnimating = false;
        });
        nextRotationTime = Time.unscaledTime + rotateInterval;
    }

    private void ShowCurrentNews()
    {
        if (news.Count == 0)
        {
            if (bannerTitle != null)
                bannerTitle.text = string.Empty;
            if (bannerDescription != null)
                bannerDescription.text = string.Empty;
            if (bannerButton != null)
                bannerButton.interactable = false;
            return;
        }

        currentNewsIndex = Mathf.Clamp(currentNewsIndex, 0, news.Count - 1);
        NewsItem item = news[currentNewsIndex];
        SetLocalizedBanner(item, LanguageManager.CurrentLanguage);

        if (bannerButton != null)
            bannerButton.interactable = true;
    }

    private void OnLanguageChanged(Language language)
    {
        ShowCurrentNews();

        if (isDetailsOpen && news.Count > 0)
            SetLocalizedDetails(news[currentNewsIndex], language);
    }

    private void SetLocalizedBanner(NewsItem item, Language language)
    {
        if (item == null)
            return;

        bool english = language == Language.English;
        if (bannerTitle != null)
            bannerTitle.text = english ? item.englishTitle : item.russianTitle;
        if (bannerDescription != null)
            bannerDescription.text = english ? item.englishDescription : item.russianDescription;
    }

    private void SetLocalizedDetails(NewsItem item, Language language)
    {
        if (item == null)
            return;

        bool english = language == Language.English;
        if (detailsTitle != null)
            detailsTitle.text = english ? item.englishTitle : item.russianTitle;
        if (detailsDescription != null)
            detailsDescription.text = english ? item.englishDescription : item.russianDescription;
    }

    private void HideDetailsInstant()
    {
        if (detailsGroup == null)
            return;

        detailsGroup.DOKill();
        detailsFrame?.DOKill();
        detailsGroup.alpha = 0f;
        detailsGroup.interactable = false;
        detailsGroup.blocksRaycasts = false;
        detailsGroup.gameObject.SetActive(false);

        if (detailsFrame != null)
            detailsFrame.localScale = Vector3.one * hiddenScale;
    }
}
