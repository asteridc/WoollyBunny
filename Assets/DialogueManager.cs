using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;
    public Image backgroundImage;
    public Image characterImage;

    [Header("Character Sprites")]
    public Image modelLeft;
    public Image modelRight;

    public Sprite transparent;
    public Sprite eliSprite;
    public Sprite kaneSprite;
    public Sprite hannaSprite;
    public Sprite firstAssasinPeacekeeper;

    [Header("Choices UI")]
    public GameObject choicesContainer;
    public GameObject choiceButtonPrefab;

    [Header("Other Elements")]
    public CodePanelController codePanelController;
    public CodePanelUI codePanelUI;
    private int currentLineIndex = 0;

    [Header("Items")]
    public ItemManager itemManager;

    [Header("Collectible View")]
    public GameObject collectiblePanel;
    public TextMeshProUGUI collectibleTitleText;
    public TextMeshProUGUI collectibleContentText;
    public Image collectibleIconImage;
    public Button collectibleCloseButton;

    public List<DialogueLine> lines = new List<DialogueLine>();

    void Start()
    {
        collectiblePanel.SetActive(false);
        if (collectibleCloseButton != null)
            collectibleCloseButton.onClick.AddListener(CloseCollectibleView);

        ShowLine();
    }

    public void ShowNextLine()
    {
        currentLineIndex++;
        Debug.Log("🟡 DialogueManager: Переход на строку " + currentLineIndex);

        if (codePanelController != null)
        {
            codePanelController.OnLineChanged(currentLineIndex);
        }
        else
        {
            Debug.LogError("❌ codePanelController = null!");
        }

        if (codePanelController == null || !codePanelController.IsCodePanelActive())
        {
            Debug.Log("🟢 DialogueManager: Показываем строку напрямую");
            ShowLine();
        }
        else
        {
            Debug.Log("🟠 DialogueManager: Панель активна, ждём код");
        }
    }

    public void OnClickNext()
    {
        // Если выбор — не продолжаем
        if (lines[currentLineIndex].hasChoices)
            return;

        // Проверка на jump после показа строки
        if (lines[currentLineIndex].isJumpLine)
        {
            currentLineIndex = lines[currentLineIndex].gotoLineIndex;
            ShowLine();
            return;
        }

        // Переход к следующей строке по обычной логике
        currentLineIndex++;
        if (currentLineIndex >= lines.Count)
        {
            Debug.Log("Диалог завершён");
            return;
        }

        ShowLine();
    }

    private void ShowLine()
    {
        DialogueLine line = lines[currentLineIndex];

        if (line.extraActions.showCodePanel)
        {
            Debug.Log("Показываем кодовую панель");
            if (codePanelUI != null)
            {
                codePanelUI.ShowCodePanel();
            }
            else
            {
                Debug.LogError("CodePanelUI не найден в сцене!");
            }
            return;
        }

        dialogueText.text = line.text;
        if (line.changeSpeakerName)
            nameText.text = line.speakerName;

        if (line.changeCharacterSprite && line.characterSprite != null)
            characterImage.sprite = line.characterSprite;

        if (line.changeBackground && line.backgroundSprite != null)
            backgroundImage.sprite = line.backgroundSprite;

        // Уведомление о получении предмета
        if (line.showItemNotification && itemManager != null)
        {
            var item = new ItemNotification
            {
                itemName = line.itemName,
                itemType = line.itemType,
                rarity = line.itemRarity,
                itemIcon = line.itemIcon
            };

            itemManager.ShowItemNotification(item);
        }

        // Просмотр коллекционного предмета (письмо и т. д.)
        if (line.showCollectibleView && collectiblePanel != null)
        {
            ShowCollectibleView(line.collectibleTitle, line.collectibleContent, line.collectibleIcon);
        }

        // Показываем персонажей
        ShowCharacters(line);

        // Показываем выбор
        if (line.hasChoices)
            ShowChoices(line.choices);

        // Дополнительные действия
        if (line.extraActions != null)
        {
            if (line.extraActions.showCodePanel)
            {
                if (codePanelUI != null)
                {
                    codePanelUI.ShowCodePanel();
                }
                else
                {
                    Debug.LogError("codePanelUI is not assigned in DialogueManager!");
                }

                if (line.extraActions.stopDialogueAfterThisLine)
                    return;
            }

            if (line.extraActions.showNotePanel)
            {
                CodePanelUI codePanel = FindObjectOfType<CodePanelUI>();
                if (codePanel != null)
                {
                    codePanel.ToggleNotePanel();
                }
            }

            if (line.extraActions.objectToActivate != null)
            {
                line.extraActions.objectToActivate.SetActive(true);
            }
        }

    }

    private void ShowCharacters(DialogueLine line)
    {
        modelLeft.sprite = null;
        modelRight.sprite = null;
        modelLeft.color = new Color(1f, 1f, 1f, 0f);
        modelRight.color = new Color(1f, 1f, 1f, 0f);

        Sprite speakerSprite = GetSpriteByName(line.characterName);
        Sprite listenerSprite = GetSpriteByName(line.listenerCharacterName);

        Color full = Color.white;
        Color dim = new Color(1f, 1f, 1f, 0.55f);

        if (speakerSprite != null)
        {
            if (line.isOnRight)
            {
                modelRight.sprite = speakerSprite;
                modelRight.color = full;
            }
            else
            {
                modelLeft.sprite = speakerSprite;
                modelLeft.color = full;
            }
        }

        if (listenerSprite != null)
        {
            if (line.isListenerOnRight)
            {
                modelRight.sprite = listenerSprite;
                modelRight.color = dim;
            }
            else
            {
                modelLeft.sprite = listenerSprite;
                modelLeft.color = dim;
            }
        }
    }

    private Sprite GetSpriteByName(string name)
    {
        switch (name)
        {
            case "Дисклеймер": return transparent;
            case "Илай": return eliSprite;
            case "Кейн": return kaneSprite;
            case "Ханна": return hannaSprite;
            case "МТ Ассасин": return firstAssasinPeacekeeper;
            default: return null;
        }
    }

    private void ShowChoices(DialogueLine.Choice[] choices)
    {

        Debug.Log("Показываем выборы: " + choices.Length);
        foreach (Transform child in choicesContainer.transform)
        {
            Destroy(child.gameObject);
        }


        choicesContainer.SetActive(true);

        foreach (var choice in choices)
        {
            GameObject buttonObj = Instantiate(choiceButtonPrefab, choicesContainer.transform);
            TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            buttonText.text = choice.choiceText;

            Button btn = buttonObj.GetComponent<Button>();
            btn.onClick.AddListener(() => OnChoiceSelected(choice));
            // buttonObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
            buttonObj.SetActive(true);

            var image = buttonObj.GetComponent<Image>();
            if (image != null)
            {
                image.enabled = true;
                Debug.Log("Имэдж включен");
            }

            buttonObj.GetComponent<Button>();
            if (btn != null)
            {
                btn.enabled = true;
                Debug.Log("Баттен включен");
            }
            var tmp = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.enabled = true;
                Debug.Log("Текст включен");
            }
        }

    }

    private void OnChoiceSelected(DialogueLine.Choice choice)
    {
        currentLineIndex = choice.nextLineIndex;
        choicesContainer.SetActive(false);
        ShowLine();
    }

    private void ShowCollectibleView(string title, string content, Sprite icon)
    {
        collectiblePanel.SetActive(true);
        collectibleTitleText.text = title;
        collectibleContentText.text = content;
        collectibleIconImage.sprite = icon;
    }

    private void CloseCollectibleView()
    {
        collectiblePanel.SetActive(false);
    }
}