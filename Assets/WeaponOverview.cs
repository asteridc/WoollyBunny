using DG.Tweening;
using System;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class WeaponOverview : MonoBehaviour
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
    [SerializeField] private TMPro.TextMeshProUGUI headMultiplier;
    [SerializeField] private TMPro.TextMeshProUGUI torsoMultiplier;
    [SerializeField] private TMPro.TextMeshProUGUI armsMultiplier;
    [SerializeField] private TMPro.TextMeshProUGUI legsMultiplier;

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

    private int GetDisplayedDamage(WeaponOverviewData data)
    {
        if (data == null)
            return 0;

        if (AccountManager.Instance == null)
            return data.damage;

        return AccountManager.Instance.GetScaledWeaponDamage(data.damage);
    }

    public void Show(WeaponOverviewData data)
    {
        if (data == null)
        {
            Debug.LogError("[WeaponOverview] Получены пустые данные оружия.");
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

        Debug.Log(
$"[WeaponOverview] Base damage: {data.damage}, " +
$"Level: {AccountManager.Instance?.GetCurrentLevel()}, " +
$"Multiplier: {AccountManager.Instance?.GetWeaponDamageMultiplier()}, " +
$"Final damage: {scaledDamage}");


        if (LanguageManager.CurrentLanguage == Language.Russian)
        {
            weaponNameRu.text = data.weaponNameRu;
            rarityRu.text = data.rarityRu;
            rarityRu.color = data.rarityColor;
            typeRu.text = data.typeRu;
            descriptionRu.text = data.descriptionRu;
        }
        else
        {
            weaponNameEn.text = data.weaponNameEn;
            rarityEn.text = data.rarityEn;
            rarityEn.color = data.rarityColor;
            typeEn.text = data.typeEn;
            descriptionEn.text = data.descriptionEn;
        }
        icon.sprite = data.icon;
        headMultiplier.text = data.headMultiplier.ToString();
        torsoMultiplier.text = data.torsoMultiplier.ToString();
        armsMultiplier.text = data.armsMultiplier.ToString();
        legsMultiplier.text = data.legsMultiplier.ToString();

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