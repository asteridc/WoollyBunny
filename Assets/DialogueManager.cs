using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;


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

        modelLeft.rectTransform.localScale = Vector3.one;
        modelRight.rectTransform.localScale = Vector3.one;

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
                modelRight.rectTransform.localScale = line.flipSpeakerImage ? new Vector3(-1, 1, 1) : Vector3.one;
            }
            else
            {
                modelLeft.sprite = speakerSprite;
                modelLeft.color = full;
                modelLeft.rectTransform.localScale = line.flipSpeakerImage ? new Vector3(-1, 1, 1) : Vector3.one;
            }
        }

        if (listenerSprite != null)
        {
            if (line.isListenerOnRight)
            {
                modelRight.sprite = listenerSprite;
                modelRight.color = dim;
                modelRight.rectTransform.localScale = line.flipListenerImage ? new Vector3(-1, 1, 1) : Vector3.one;
            }
            else
            {
                modelLeft.sprite = listenerSprite;
                modelLeft.color = dim;
                modelLeft.rectTransform.localScale = line.flipListenerImage ? new Vector3(-1, 1, 1) : Vector3.one;
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
        Vector2[] positions = GetChoicePositions(choices.Length);

        int index = 0;
        foreach (var choice in choices)
        {
            GameObject buttonObj = Instantiate(choiceButtonPrefab, choicesContainer.transform);
            RectTransform rt = buttonObj.GetComponent<RectTransform>();
            rt.anchoredPosition = positions[index];

            TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            buttonText.text = choice.choiceText;
            Button btn = buttonObj.GetComponent<Button>();
            Image bg = buttonObj.GetComponent<Image>();

            // Устанавливаем стартовые цвета — без выделения
            buttonText.color = Color.white;
            bg.color = new Color(0.2f, 0.2f, 0.2f); // тёмный фон или твой стандарт

            // Добавляем поведение на наведение
            AddHoverEffects(btn, bg, buttonText, choice.choiceType);

            // Поведение при клике
            btn.onClick.AddListener(() => OnChoiceSelected(choice));

            buttonObj.SetActive(true);

            index++;
        }
    }

    private Vector2[] GetChoicePositions(int count)
    {
        switch (count)
        {
            case 2:
                return new Vector2[]
                {
                new Vector2(0, 60),
                new Vector2(0, -60)
                };
            case 3:
                return new Vector2[]
                {
                new Vector2(0, 80),
                new Vector2(-160, -40),
                new Vector2(160, -40)
                };
            case 4:
                return new Vector2[]
                {
                new Vector2(-100, 60),
                new Vector2(100, 60),
                new Vector2(-100, -60),
                new Vector2(100, -60)
                };
            case 5:
                return new Vector2[]
                {
                new Vector2(0, 100),
                new Vector2(-120, 30),
                new Vector2(120, 30),
                new Vector2(-80, -60),
                new Vector2(80, -60)
                };
            default:
                return new Vector2[] { Vector2.zero };
        }
    }

    public void AddHoverEffects(Button btn, Image bg, TextMeshProUGUI txt, ChoiceType choiceType)
    {
        Color baseTextColor = txt.color;
        Color baseBGColor = bg.color;

        EventTrigger trigger = btn.gameObject.AddComponent<EventTrigger>();

        // Наведение
        var pointerEnter = new EventTrigger.Entry();
        pointerEnter.eventID = EventTriggerType.PointerEnter;
        pointerEnter.callback.AddListener((eventData) => {
            if (choiceType == ChoiceType.Required)
            {
                bg.color = new Color(1f, 0.84f, 0.4f); // янтарный
                txt.color = Color.black;
            }
            else
            {
                bg.color = Color.white;
                txt.color = Color.black;
            }
        });
        trigger.triggers.Add(pointerEnter);

        // Выход
        var pointerExit = new EventTrigger.Entry();
        pointerExit.eventID = EventTriggerType.PointerExit;
        pointerExit.callback.AddListener((eventData) => {
            bg.color = baseBGColor;
            txt.color = baseTextColor;
        });
        trigger.triggers.Add(pointerExit);
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