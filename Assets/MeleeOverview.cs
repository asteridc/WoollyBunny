using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class MeleeOverview : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private float showDuration = 0.35f;
    [SerializeField] private float hiddenOffset = 500f;

    private RectTransform rect;
    private CanvasGroup canvasGroup;

    private Vector2 shownPosition;
    private Vector2 hiddenPosition;

    [SerializeField] private TMPro.TextMeshProUGUI weaponNameRu;
    [SerializeField] private TMPro.TextMeshProUGUI weaponNameEn;
    [SerializeField] private TMPro.TextMeshProUGUI damage;
    [SerializeField] private TMPro.TextMeshProUGUI rarityRu;
    [SerializeField] private TMPro.TextMeshProUGUI rarityEn;
    [SerializeField] private Color colorRarity;
    [SerializeField] private TMPro.TextMeshProUGUI typeRu;
    [SerializeField] private TMPro.TextMeshProUGUI typeEn;
    [SerializeField] private TMPro.TextMeshProUGUI descriptionRu;
    [SerializeField] private TMPro.TextMeshProUGUI descriptionEn;
    [SerializeField] private UnityEngine.UI.Image icon;
    
    [Header("Abilities")]
    [SerializeField] private Image iconAbility;
    [SerializeField] private TooltipTrigger abilityTrigger;


    [SerializeField] private GameObject thirdModule;

    private bool isVisible;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        shownPosition = rect.anchoredPosition;
        hiddenPosition = shownPosition + Vector2.right * hiddenOffset;

        rect.anchoredPosition = hiddenPosition;

        canvasGroup.alpha = 0;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }

    public void Show(MeleeOverviewData data)
    {
        if (data == null)
        {
            Debug.LogError("[MeleeOverview] Получены пустые данные оружия.");
            return;
        }

        if (isVisible)
            Hide();

        isVisible = true;

        int scaledDamage = data.damage;

        if (AccountManager.Instance != null)
        {
            scaledDamage =
                AccountManager.Instance.GetScaledWeaponDamage(data.damage);
        }

        damage.text = scaledDamage.ToString();

        if (LanguageManager.CurrentLanguage == Language.Russian)
        {
            weaponNameRu.text = data.weaponNameRu;
            typeRu.text = data.typeRu;
            rarityRu.text = data.rarityRu;
            rarityRu.color = data.rarityColor;
            descriptionRu.text = data.descriptionRu;
        }
        else
        {
            weaponNameEn.text = data.weaponNameEn;
            typeEn.text = data.typeEn;
            rarityEn.text = data.rarityEn;
            rarityEn.color = data.rarityColor;
            descriptionEn.text = data.descriptionEn;
        }
        icon.sprite = data.icon;
        iconAbility.sprite = data.iconAbility;
        abilityTrigger.SetTooltip(data.tooltipAbility);

        thirdModule.SetActive(data.ModuleCount >= 3);

        rect.DOKill();
        canvasGroup.DOKill();

        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;

        rect.DOAnchorPos(shownPosition, showDuration)
            .SetEase(Ease.OutExpo).SetUpdate(true);

        canvasGroup.DOFade(1, showDuration).SetUpdate(true);
    }

    public void Hide()
    {
        isVisible = false;

        rect.DOKill();
        canvasGroup.DOKill();

        rect.DOAnchorPos(hiddenPosition, showDuration)
            .SetEase(Ease.InExpo).SetUpdate(true);

        canvasGroup.DOFade(0, showDuration)
            .OnComplete(() =>
            {
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
            }).SetUpdate(true);
    }
}
