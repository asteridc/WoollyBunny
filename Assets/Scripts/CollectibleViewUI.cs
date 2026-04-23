using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CollectibleViewUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI itemTitleText;
    [SerializeField] private TextMeshProUGUI itemContentText;
    [SerializeField] private Image itemIcon;
    [SerializeField] private GameObject panel;
    [SerializeField] private CanvasGroup panelCanvasGroup;
    [SerializeField] private float hideDuration = 0.25f;

    private bool isHiding = false;

    private CollectibleItemBase currentItem;

    private void Update()
    {
        // Ловим Escape
        if (panel.activeSelf && !isHiding && Input.GetKeyDown(KeyCode.Escape))
        {
            Hide();
        }
    }

    private void OnEnable()
    {
        LanguageManager.OnLanguageChanged += Refresh;
    }

    private void OnDisable()
    {
        LanguageManager.OnLanguageChanged -= Refresh;
    }

    public void Show(CollectibleItemBase item)
    {
        if (item == null) return;

        currentItem = item;

        // Скрываем диалог с анимацией
        DialogueManager.Instance.HideDialoguePanel(() =>
        {
            // Подготовка панели
            panel.SetActive(true);
            panelCanvasGroup.alpha = 0f;
            panelCanvasGroup.interactable = false;
            panelCanvasGroup.blocksRaycasts = false;

            // Устанавливаем содержимое в зависимости от типа
            switch (item.type)
            {
                case CollectibleType.Note:
                case CollectibleType.Document:
                    ShowTextItem(item as CollectibleTextItem); // твой метод для текста
                    break;

                default:
                    Debug.LogWarning($"Collectible type {item.type} not supported yet");
                    break;
            }

            // Плавное появление панели
            DOTween.Sequence()
                .AppendInterval(0.25f) // ждем, пока диалог скроется
                .Append(panelCanvasGroup.DOFade(1f, 0.25f))
                .OnComplete(() =>
                {
                    panelCanvasGroup.interactable = true;
                    panelCanvasGroup.blocksRaycasts = true;
                    DialogueManager.Instance.isCollectibleOpen = true;
                });
        });
    }


    public void Hide()
    {
        if (isHiding) return; // чтобы не запускать несколько анимаций
        isHiding = true;

        // Анимация скрытия через CanvasGroup
        panelCanvasGroup.interactable = false;
        panelCanvasGroup.blocksRaycasts = false;

        panelCanvasGroup.DOFade(0f, hideDuration)
            .OnComplete(() =>
            {
                panel.SetActive(false);
                currentItem = null;
                isHiding = false;
                DialogueManager.Instance.isCollectibleOpen = false; // сбрасываем флаг, можно снова показывать
            });
    }

    private void Refresh(Language _)
    {
        if (currentItem == null) return;
        Show(currentItem);
    }

    private void ShowTextItem(CollectibleTextItem item)
    {
        if (item == null) return;

        itemTitleText.text = item.GetTitle();
        itemContentText.text = item.GetContent();
        itemIcon.sprite = item.icon;
    }
}
