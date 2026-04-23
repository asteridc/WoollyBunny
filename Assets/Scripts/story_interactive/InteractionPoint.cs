using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InteractionPoint : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public GameObject minigamePanel;

    public CanvasGroup canvasGroup;
    public Image iconImage;
    public Color normalColor = Color.white;
    public Color hoverColor = Color.yellow;
    public float fadeDuration = 0.5f;
    public int targetLineRu;
    public int targetLineEn;

    private bool isActive = true;
    [HideInInspector] public bool wasUsed = false;
    public bool isRepeatable = false;
    public bool isToolPoint = false;
    public bool launchesMinigame = false;


    public void OnInteract()
    {
        if (!isActive) return;

        if (!isRepeatable) // не блокируем repeatable
        {
            isActive = false;
            wasUsed = true;
        }

        SubstationRoomManager.Instance.HideAllPoints();
        Hide();

        if (isToolPoint)
        {
            SubstationRoomManager.Instance.toolsWereCollected = true;
        }


        if (launchesMinigame && minigamePanel != null)
        {
            minigamePanel.SetActive(true);
            DialogueManager.Instance.HideDialoguePanel(() =>
            {
                minigamePanel.SetActive(true);
                ElectroChainManager.Instance.ShowUI();
            });
        }
        else
        {
            DialogueManager.Instance.HideDialoguePanel(() =>
            {
                DialogueManager.Instance.JumpToLine(GetTargetLine());
                DialogueManager.Instance.ShowDialoguePanel();
            });
        }
    }

    private int GetTargetLine()
    {
        if (LanguageManager.CurrentLanguage == Language.English && targetLineEn > 0)
            return targetLineEn - 1;

        return targetLineRu - 1;
    }


    public void ShowToolsInToolBox()
    {
        foreach (var tool in ElectroChainManager.Instance.startingTools)
        {
            tool.gameObject.SetActive(true);
            // или tool.iconImage.enabled = true;
        }
    }


    public void Show()
    {
        gameObject.SetActive(true);
        canvasGroup.alpha = 0;
        canvasGroup.DOFade(1f, fadeDuration);
    }

    public void Hide()
    {
        canvasGroup.DOFade(0f, fadeDuration).OnComplete(() => gameObject.SetActive(false));
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        iconImage.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        iconImage.color = normalColor;
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
       OnInteract();
    }
}