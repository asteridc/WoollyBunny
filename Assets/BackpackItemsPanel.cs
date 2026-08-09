using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(CanvasGroup))]
public class BackpackItemsPanel : MonoBehaviour
{
    public static BackpackItemsPanel Instance;
    [SerializeField] private BackpackSectionController sectionController;

    [Header("Transition")]
    [SerializeField] private CanvasGroup screenFade;
    [SerializeField] private float fadeTime = 0.25f;

    [Header("Animation")]
    [SerializeField] private float fadeDuration = 0.15f;
    [SerializeField] private float scaleDuration = 0.25f;
    [SerializeField] private float switchDuration = 0.3f;

    [Header("References")]
    [SerializeField] private WeaponOverview weaponOverview;
    [SerializeField] private WeaponOverviewData overviewData;
    [SerializeField] private MeleeOverview meleeOverview;
    [SerializeField] private ResourcesOverview resourcesOverview;

    [Header("Buttons")]
    [SerializeField] private Button leftButton;
    [SerializeField] private Button rightButton;

    [Header("Section Text")]
    [SerializeField] private TextMeshProUGUI weaponsText;
    [SerializeField] private TextMeshProUGUI meleeText;
    [SerializeField] private TextMeshProUGUI resourcesText;

    [Header("Item Containers")]
    [SerializeField] private CanvasGroup weaponsContainer;
    [SerializeField] private CanvasGroup meleeContainer;
    [SerializeField] private CanvasGroup resourcesContainer;

    [Header("Item Scrollers")]
    [SerializeField] private BackpackItemScroller weaponsScroller;
    [SerializeField] private BackpackItemScroller meleeScroller;
    [SerializeField] private BackpackItemScroller resourcesScroller;

    public CanvasGroup canvasGroup;
    private RectTransform rect;

    public bool isOpen = false;
    private ItemType currentTab = ItemType.Weapons;
    private bool isSwitching = false;

    public bool IsOpen => isOpen;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rect = GetComponent<RectTransform>();

        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        rect.localScale = Vector3.one * 0.95f;
    }

    private void Start()
    {
        if (leftButton != null)
            leftButton.onClick.AddListener(ScrollLeft);

        if (rightButton != null)
            rightButton.onClick.AddListener(ScrollRight);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            Toggle();
        }

        if (Input.GetKeyDown(KeyCode.Escape) && isOpen)
        {
            Hide();
        }

        if (Input.GetKeyDown(KeyCode.U) && isOpen)
        {
            BackpackSectionController.Instance.SelectSection(1);
        }

        if (isOpen)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                SelectPreviousTab();
            }

            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                SelectNextTab();
            }
        }
    }

    public void Toggle()
    {
        if (isOpen)
            Hide();
        else
            Show();
    }

    public void Show()
    {
        isOpen = true;

        screenFade.DOKill();
        canvasGroup.DOKill();
        rect.DOKill();

        screenFade.alpha = 0;

        screenFade.DOFade(1f, fadeTime)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                gameObject.SetActive(true);

                if (sectionController != null)
                    sectionController.OpenItemsSection();

                HideAllOverviews();
                HideAllTexts();

                currentTab = ItemType.Weapons;
                InitializeTab(currentTab);


                canvasGroup.alpha = 0;
                rect.localScale = Vector3.one * 0.95f;

                canvasGroup.DOFade(1, fadeTime)
                    .SetUpdate(true);

                rect.DOScale(1, scaleDuration)
                    .SetEase(Ease.OutBack)
                    .SetUpdate(true);

                screenFade.DOFade(0, fadeTime)
                    .SetDelay(0.1f)
                    .SetUpdate(true);
            });


        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    public void Hide()
    {
        isOpen = false;

        HideAllOverviews();
        HideAllTexts();

        screenFade.DOKill();
        canvasGroup.DOKill();
        rect.DOKill();


        screenFade.DOFade(1f, fadeTime)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                canvasGroup.DOFade(0, fadeTime)
                    .SetUpdate(true);

                rect.DOScale(0.95f, scaleDuration)
                    .SetEase(Ease.InBack)
                    .SetUpdate(true)
                    .OnComplete(() =>
                    {
                        screenFade.DOFade(0, fadeTime)
                            .SetUpdate(true);
                    });
            });



        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    public void ScrollLeft()
    {
        if (!isSwitching)
            SelectPreviousTab();
    }

    public void ScrollRight()
    {
        if (!isSwitching)
            SelectNextTab();
    }

    private void SelectPreviousTab()
    {
        if (isSwitching)
            return;

        currentTab--;
        if (currentTab < ItemType.Weapons)
            currentTab = ItemType.Resources;

        SelectTab(currentTab);
    }

    private void SelectNextTab()
    {
        if (isSwitching)
            return;

        currentTab++;
        if (currentTab > ItemType.Resources)
            currentTab = ItemType.Weapons;

        SelectTab(currentTab);
    }

    private void InitializeTab(ItemType tab)
    {
        HideAllOverviews();
        HideAllTexts();
        HideAllContainers();

        currentTab = tab;

        switch (tab)
        {
            case ItemType.Weapons:
                if (weaponsText != null)
                    weaponsText.gameObject.SetActive(true);
                if (weaponsContainer != null)
                    InitializeContainer(weaponsContainer);
                if (weaponsScroller != null)
                    weaponsScroller.SetItemsData(ItemType.Weapons);
                break;

            case ItemType.Melee:
                if (meleeText != null)
                    meleeText.gameObject.SetActive(true);
                if (meleeContainer != null)
                    InitializeContainer(meleeContainer);
                if (meleeScroller != null)
                    meleeScroller.SetItemsData(ItemType.Melee);
                break;

            case ItemType.Resources:
                if (resourcesText != null)
                    resourcesText.gameObject.SetActive(true);
                if (resourcesContainer != null)
                    InitializeContainer(resourcesContainer);
                if (resourcesScroller != null)
                    resourcesScroller.SetItemsData(ItemType.Resources);
                break;
        }
    }

    private void InitializeContainer(CanvasGroup container)
    {
        if (container == null)
            return;

        container.DOKill();
        container.gameObject.SetActive(true);
        container.alpha = 1f;
        container.interactable = true;
        container.blocksRaycasts = true;
    }

    private void SelectTab(ItemType tab)
    {
        isSwitching = true;

        HideAllOverviews();
        HideAllTexts();
        HideAllContainers();

        currentTab = tab;

        switch (tab)
        {
            case ItemType.Weapons:
                if (weaponsText != null)
                    weaponsText.gameObject.SetActive(true);

                if (weaponsContainer != null)
                    ShowContainer(weaponsContainer);

                if (weaponsScroller != null)
                {
                    ShowScroller(weaponsScroller);
                    weaponsScroller.SetItemsData(ItemType.Weapons);
                }
                break;

            case ItemType.Melee:
                if (meleeText != null)
                    meleeText.gameObject.SetActive(true);

                if (meleeContainer != null)
                    ShowContainer(meleeContainer);

                if (meleeScroller != null)
                {
                    ShowScroller(meleeScroller);
                    meleeScroller.SetItemsData(ItemType.Melee);
                }
                break;

            case ItemType.Resources:
                if (resourcesText != null)
                    resourcesText.gameObject.SetActive(true);

                if (resourcesContainer != null)
                    ShowContainer(resourcesContainer);

                if (resourcesScroller != null)
                {
                    ShowScroller(resourcesScroller);
                    resourcesScroller.SetItemsData(ItemType.Resources);
                }
                break;
        }

        DOVirtual.DelayedCall(switchDuration, () => { isSwitching = false; }, true);
    }

    private void ShowContainer(CanvasGroup container)
    {
        if (container == null)
            return;

        container.gameObject.SetActive(true);

        container.DOKill();

        container.blocksRaycasts = true;
        container.interactable = false;

        container.alpha = 0;

        container.DOFade(1, switchDuration)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                container.interactable = true;
            });
    }

    private void ShowScroller(BackpackItemScroller scroller)
    {
        if (weaponsScroller != null)
            weaponsScroller.gameObject.SetActive(false);

        if (meleeScroller != null)
            meleeScroller.gameObject.SetActive(false);

        if (resourcesScroller != null)
            resourcesScroller.gameObject.SetActive(false);

        if (scroller != null)
            scroller.gameObject.SetActive(true);
    }

    private void HideAllContainers()
    {
        HideContainer(weaponsContainer);
        HideContainer(meleeContainer);
        HideContainer(resourcesContainer);
    }

    private void HideContainer(CanvasGroup container)
    {
        if (container == null)
            return;

        container.DOKill();

        container.interactable = false;
        container.blocksRaycasts = false;

        container.DOFade(0, switchDuration).SetUpdate(true)
            .OnComplete(() =>
            {
               container.gameObject.SetActive(false);
            });
    }

    private void HideAllTexts()
    {
        if (weaponsText != null)
            weaponsText.gameObject.SetActive(false);

        if (meleeText != null)
            meleeText.gameObject.SetActive(false);

        if (resourcesText != null)
            resourcesText.gameObject.SetActive(false);
    }

    private void HideAllOverviews()
    {
        if (weaponOverview != null)
            weaponOverview.Hide();

        if (meleeOverview != null)
            meleeOverview.Hide();

        if (resourcesOverview != null)
            resourcesOverview.Hide();
    }
}