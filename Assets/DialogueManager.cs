using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Linq;
using DG.Tweening;


public class DialogueManager : MonoBehaviour
{
    private List<GameObject> currentChoiceButtons = new List<GameObject>();
    private HashSet<string> usedOptionalChoices = new HashSet<string>();
    private readonly Stack<int> dialogueHistory = new Stack<int>();
    private Coroutine skipCoroutine;
    private bool isSkipping = false;

    [Header("Dialogue Navigation")]
    [SerializeField] private float skipLineDelay = 0.06f;

    private int bloodthirst = 0;
    private int nobility = 0;
    private int love = 0;

    [Header("UI Elements")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;
    public Image backgroundImage;
    public Image characterImage;
    public GameObject dialoguePanel;

    [SerializeField] private Image backgroundFadeImage;
    [SerializeField] private float fadeDuration = 2f;
    [SerializeField] private bool clickToContinueAfterFade = true;
    [SerializeField] private float autoContinueDelay = 1f;
    public bool waitingForClick = false;

    [Header("Character Sprites")]
    public Image modelLeft;
    public Image modelRight;

    public Sprite transparent;
    public Sprite eliSprite;
    public Sprite kaneSprite;
    public Sprite hannaSprite;
    public Sprite keremSprite;
    public Sprite firstAssasinPeacekeeper;

    [Header("Choices UI")]
    public GameObject choicesContainer;
    public GameObject choiceButtonPrefab;

    [Header("Other Elements")]
    public CodePanelController codePanelController;
    public CodePanelUI codePanelUI;
    private int currentLineIndex = 0;
    public GameObject miniGamePanel;
    public ElectroChainManager electroChainManager;
    [SerializeField] public Image blackOverlay;
    [SerializeField] public float FadeDuration = 1.5f;
    public GameObject gameOverPanel;
    public TextMeshProUGUI respawnCountdownText;
    public float respawnTime = 13f;

    [Header("Items")]
    public ItemManager itemManager;

    [Header("Story Notification UI")]
    public GameObject storyNotificationPanel;
    public TextMeshProUGUI storyNotificationText;
    public Image storyNotificationIcon;

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

    void Update()
    {
        if (waitingForClick && Input.GetMouseButtonDown(0))
        {
            waitingForClick = false;
            dialoguePanel.SetActive(true);
            modelLeft.gameObject.SetActive(true);
            modelRight.gameObject.SetActive(true);

            ShowLine(); // Повторный показ той же строки, теперь уже с UI
        }
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
        StopSkippingIfNeeded();

        // Если выбор — не продолжаем
        if (lines[currentLineIndex].hasChoices)
            return;

        // Проверка на jump после показа строки
        if (lines[currentLineIndex].isJumpLine)
        {
            currentLineIndex = lines[currentLineIndex].gotoLineIndex - 1;
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
        ShowLine(true);
    }

    private void ShowLine(bool saveToHistory)
    {
        if (lines == null || lines.Count == 0)
            return;

        currentLineIndex = Mathf.Clamp(currentLineIndex, 0, lines.Count - 1);

        if (saveToHistory)
        {
            if (dialogueHistory.Count == 0 || dialogueHistory.Peek() != currentLineIndex)
                dialogueHistory.Push(currentLineIndex);
        }

        DialogueLine line = lines[currentLineIndex];

        if (line.changeBackground && line.backgroundSprite != null)
        {
            StartCoroutine(HandleBackgroundTransition(line));
            return;
        }

        string finalText = line.text;

        if (line.hasPathConsequences && line.pathVarients != null && line.pathVarients.Length > 0)
        {
            List<DominantPath> dominantPaths = GetDominantPaths();

            foreach (var dominant in dominantPaths)
            {
                foreach (var variant in line.pathVarients)
                {
                    if (variant.path == dominant && !string.IsNullOrEmpty(variant.overrideText))
                    {
                        finalText = variant.overrideText;

                        if (variant.showStoryNotification)
                        {
                            ShowStoryNotification(
                                variant.storyNotificationText,
                                variant.storyNotificationDuration > 0 ? variant.storyNotificationDuration : 5f,
                                variant.storyNotificationIcon,
                                variant.storyNotificationTextColor,
                                variant.storyNotificationIconColor,
                                variant.storyNotificationFontSize,
                                variant.useCustomFont ? variant.customFont : null,
                                variant.isBold,
                                variant.isItalic,
                                variant.isUppercase
                            );
                        }

                        goto End;
                    }
                }
            }
        }

    End:
        dialogueText.text = finalText;
        nameText.text = line.speakerName;

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

        if (line.changeSpeakerName)
            nameText.text = line.speakerName;

        if (line.changeCharacterSprite && line.characterSprite != null)
            characterImage.sprite = line.characterSprite;

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

        // Сюжетное уведомление
        if (line.showStoryNotification)
        {
            ShowStoryNotification(
                line.storyNotificationText,
                line.storyNotificationDuration > 0 ? line.storyNotificationDuration : 5f,
                line.storyNotificationIcon,
                line.storyNotificationTextColor,
                line.storyNotificationIconColor,
                line.storyNotificationFontSize,
                line.useCustomFont ? line.customFont : null,
                line.isBold,
                line.isItalic,
                line.isUppercase
            );
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

            if (line.extraActions.showElectroSubstationMinigame)
            {
                if (ElectroChainManager.Instance != null)
                {
                    ElectroChainManager.Instance.ShowUI();
                }
                else
                {
                    Debug.LogError("ElectroChainManager.Instance не инициализирован!");
                }
            }

            if (line.choices != null && line.choices.Length > 0)
            {
                ShowChoices(line.choices);
            }

        }

    }

    public void OnClickBack()
    {
        StopSkippingIfNeeded();

        if (dialogueHistory.Count <= 1)
        {
            Debug.Log("Назад недоступно: это первая реплика.");
            return;
        }

        dialogueHistory.Pop();
        currentLineIndex = dialogueHistory.Peek();
        choicesContainer.SetActive(false);
        ShowLine(false);
    }

    public void OnClickSkip()
    {
        if (isSkipping || lines == null || lines.Count == 0)
            return;

        skipCoroutine = StartCoroutine(SkipDialogueFast());
    }

    private IEnumerator SkipDialogueFast()
    {
        isSkipping = true;

        while (currentLineIndex < lines.Count)
        {
            DialogueLine line = lines[currentLineIndex];
            if (RequiresPlayerAction(line))
                break;

            if (!TryMoveToNextLineIndex(line))
                break;

            ShowLine();
            yield return new WaitForSeconds(skipLineDelay);
        }

        isSkipping = false;
        skipCoroutine = null;
    }

    private bool TryMoveToNextLineIndex(DialogueLine line)
    {
        if (line.isJumpLine)
        {
            currentLineIndex = Mathf.Clamp(line.gotoLineIndex - 1, 0, lines.Count - 1);
            return true;
        }

        if (currentLineIndex + 1 >= lines.Count)
            return false;

        currentLineIndex++;
        return true;
    }

    private bool RequiresPlayerAction(DialogueLine line)
    {
        if (line.hasChoices)
            return true;

        if (line.changeBackground && clickToContinueAfterFade)
            return true;

        if (line.showCollectibleView)
            return true;

        if (line.extraActions == null)
            return false;

        return line.extraActions.showCodePanel
            || line.extraActions.showNotePanel
            || line.extraActions.showElectroSubstationMinigame
            || line.extraActions.stopDialogueAfterThisLine;
    }

    private void StopSkippingIfNeeded()
    {
        if (!isSkipping)
            return;

        if (skipCoroutine != null)
            StopCoroutine(skipCoroutine);

        isSkipping = false;
        skipCoroutine = null;
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
            case "Керем": return keremSprite;
            case "МТ Ассасин": return firstAssasinPeacekeeper;
            default: return null;
        }
    }

    private void ShowChoices(DialogueLine.Choice[] choices)
    {
        // Фильтрация опциональных, которые уже были выбраны
        var filteredChoices = choices
            .Where(c => c.choiceType != ChoiceType.Optional || !usedOptionalChoices.Contains(c.choiceText))
            .ToArray();

        // Очистка старых кнопок
        foreach (var obj in currentChoiceButtons)
            Destroy(obj);
        currentChoiceButtons.Clear();

        choicesContainer.SetActive(true);

        Vector2[] positions = GetChoicePositions(filteredChoices.Length);

        for (int i = 0; i < filteredChoices.Length; i++)
        {
            var choice = filteredChoices[i];
            GameObject buttonObj = Instantiate(choiceButtonPrefab, choicesContainer.transform);
            currentChoiceButtons.Add(buttonObj);

            RectTransform rect = buttonObj.GetComponent<RectTransform>();
            rect.anchoredPosition = positions[i];

            var buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            buttonText.text = choice.choiceText;

            var bg = buttonObj.GetComponent<Image>();
            var btn = buttonObj.GetComponent<Button>();

            // Стиль по умолчанию  
            if (choice.choiceType == ChoiceType.Required)
            {
                bg.color = new Color(0.25f, 0.18f, 0.05f);
                buttonText.color = new Color(1f, 0.84f, 0.4f);
            }
            else
            {
                bg.color = new Color(0.2f, 0.2f, 0.2f);
                buttonText.color = Color.white;
            }

            // Наведение
            AddHoverEffects(btn, bg, buttonText, choice.choiceType);

            // Логика клика
            btn.onClick.AddListener(() =>
            {
                OnChoiceSelected(choice);

                if (choice.choiceType == ChoiceType.Optional)
                {
                    usedOptionalChoices.Add(choice.choiceText);
                    buttonObj.SetActive(false);
                    RepositionChoices();
                }
            });
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

    private void RepositionChoices()
    {
        var activeButtons = currentChoiceButtons
            .Where(b => b != null && b.activeSelf)
            .ToList();

        Vector2[] positions = GetChoicePositions(activeButtons.Count);

        for (int i = 0; i < activeButtons.Count; i++)
        {
            RectTransform rect = activeButtons[i].GetComponent<RectTransform>();
            rect.anchoredPosition = positions[i];
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
        pointerEnter.callback.AddListener((eventData) =>
        {
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
        pointerExit.callback.AddListener((eventData) =>
        {
            bg.color = baseBGColor;
            txt.color = baseTextColor;
        });
        trigger.triggers.Add(pointerExit);
    }

    private void OnChoiceSelected(DialogueLine.Choice choice)
    {
        switch (choice.pathReward)
        {
            case DominantPath.Bloodthirst:
                bloodthirst++;
                break;
            case DominantPath.Nobility:
                nobility++;
                break;
            case DominantPath.Love:
                love++;
                break;
            case DominantPath.None:
                break;
        }

        Debug.Log($"Выбран путь: {choice.pathReward}");
        Debug.Log($"Очки путей: Кровожадность = {bloodthirst}, Благородство = {nobility}, Любовь = {love}");

        var dominant = GetDominantPaths();
        Debug.Log("Преобладающий путь(и): " + string.Join(", ", dominant));

        if (choice.choiceType == ChoiceType.Optional)
        {
            usedOptionalChoices.Add(choice.choiceText);
        }
        currentLineIndex = choice.nextLineIndex - 1;
        choicesContainer.SetActive(false);
        ShowLine();
    }

    private List<DominantPath> GetDominantPaths()
    {
        var scores = new Dictionary<DominantPath, int>
    {
        { DominantPath.Bloodthirst, bloodthirst },
        { DominantPath.Nobility, nobility },
        { DominantPath.Love, love }
    };

        int maxScore = scores.Values.Max();

        return scores
            .Where(kv => kv.Value == maxScore)
            .Select(kv => kv.Key)
            .ToList();
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

    public TMP_FontAsset defaultFont;

    public void ShowStoryNotification(
    string text,
    float duration,
    Sprite icon = null,
    Color? textColor = null,
    Color? iconColor = null,
    float? fontSize = null,
    TMP_FontAsset font = null,
    bool isBold = false,
    bool isItalic = false,
    bool isUppercase = false)
    {
        storyNotificationPanel.SetActive(true);

        string formattedText = text;

        if (isUppercase)
            formattedText = formattedText.ToUpper();
        if (isBold)
            formattedText = $"<b>{formattedText}</b>";
        if (isItalic)
            formattedText = $"<i>{formattedText}</i>";

        storyNotificationText.text = formattedText;
        storyNotificationText.fontSize = fontSize ?? storyNotificationText.fontSize;
        storyNotificationText.color = textColor ?? Color.white;
        storyNotificationText.font = font ?? defaultFont;

        if (storyNotificationIcon != null)
        {
            storyNotificationIcon.sprite = icon;
            storyNotificationIcon.color = iconColor ?? Color.white;
            storyNotificationIcon.gameObject.SetActive(icon != null);
        }

        StartCoroutine(HideStoryNotificationAfterDelay(duration));
    }

    private IEnumerator HideStoryNotificationAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        storyNotificationPanel.SetActive(false);
    }

    private IEnumerator ChangeBackgroundWithFade(Sprite newBackground)
    {
        // Затемнение
        yield return StartCoroutine(FadeImage(0f, 1f, fadeDuration));

        // Смена фона
        backgroundImage.sprite = newBackground;

        // Осветление
        yield return StartCoroutine(FadeImage(1f, 0f, fadeDuration));

        if (clickToContinueAfterFade)
        {
            waitingForClick = true;
        }
        else
        {
            yield return new WaitForSeconds(autoContinueDelay);
            ShowNextLine(); // или твоя логика показа следующей реплики
        }
    }

    private IEnumerator FadeImage(float from, float to, float duration)
    {
        float timer = 0f;
        Color c = backgroundFadeImage.color;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(from, to, timer / duration);
            backgroundFadeImage.color = new Color(c.r, c.g, c.b, alpha);
            yield return null;
        }

        backgroundFadeImage.color = new Color(c.r, c.g, c.b, to);
    }

    private void FadeToBlackThenShowBackground(Sprite newBackground)
    {
        backgroundFadeImage.gameObject.SetActive(true);
        backgroundFadeImage.color = new Color(0, 0, 0, 0);

        backgroundFadeImage.DOFade(1f, fadeDuration).OnComplete(() =>
        {
            backgroundImage.sprite = newBackground;

            // Осветление
            backgroundFadeImage.DOFade(0f, fadeDuration).OnComplete(() =>
            {
                backgroundFadeImage.gameObject.SetActive(false);
                // Здесь либо вызываем ShowLine(), либо ждём нажатия
            });
        });
    }

    private IEnumerator HandleBackgroundTransition(DialogueLine line)
    {
        line.changeBackground = false;
        yield return StartCoroutine(FadeToBlack());

        // Скрываем UI
        dialoguePanel.SetActive(false);
        modelLeft.gameObject.SetActive(false);
        modelRight.gameObject.SetActive(false);

        // Меняем фон
        backgroundImage.sprite = line.backgroundSprite;

        yield return StartCoroutine(FadeFromBlack());

        if (clickToContinueAfterFade)
        {
            waitingForClick = true;
            yield return new WaitUntil(() => Input.GetMouseButtonDown(0));
        }
        else
        {
            yield return new WaitForSeconds(autoContinueDelay);
        }

        ShowLine();
    }

    private IEnumerator FadeToBlack()
    {
        backgroundFadeImage.raycastTarget = true;
        Color color = backgroundFadeImage.color;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            float t = elapsed / fadeDuration;
            color.a = Mathf.Lerp(0f, 1f, t);
            backgroundFadeImage.color = color;
            elapsed += Time.deltaTime;
            yield return null;
        }

        color.a = 1f;
        backgroundFadeImage.color = color;
    }

    private IEnumerator FadeFromBlack()
    {
        Color color = backgroundFadeImage.color;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            float t = elapsed / fadeDuration;
            color.a = Mathf.Lerp(1f, 0f, t);
            backgroundFadeImage.color = color;
            elapsed += Time.deltaTime;
            yield return null;
        }

        color.a = 0f;
        backgroundFadeImage.color = color;
        backgroundFadeImage.raycastTarget = false;
    }

    public static DialogueManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void ContinueDialogue(bool success)
    {
        if (success)
        {
            Debug.Log("Мини-игра пройдена. Продолжаем диалог.");
            ShowNextLine(); // Или другой метод для перехода к следующей линии
        }
        else
        {
            Debug.Log("Мини-игра провалена. Можно вызвать альтернативный диалог или конец.");
            // Например:
            // ShowFailureLine(); // или LoadFailureDialogue();
        }
    }

    private bool isGameOverActive = false;

    public void TriggerGameOver()
    {
        if (isGameOverActive) return;
        isGameOverActive = true;

        // 1. Включаем черный оверлей, делаем прозрачным
        blackOverlay.gameObject.SetActive(true);
        blackOverlay.color = new Color(0, 0, 0, 0);

        // 2. Анимируем затемнение до полной непрозрачности
        blackOverlay.DOFade(1.5f, FadeDuration).OnComplete(() =>
        {
            // 3. В момент полного затемнения — показываем экран смерти
            gameOverPanel.SetActive(true);

            // 4. Анимация осветления — плавно возвращаем прозрачность
            blackOverlay.DOFade(0f, FadeDuration).OnComplete(() =>
            {
                // 5. Скрываем оверлей после осветления
                blackOverlay.gameObject.SetActive(false);

                // 6. Запускаем таймер (показываем текст с обратным отсчетом)
                respawnCountdownText.gameObject.SetActive(true);
                StartCoroutine(RespawnCountdown());
            });
        });
    }

    IEnumerator RespawnCountdown()
    {
        float timer = respawnTime;

        while (timer > 0)
        {
            respawnCountdownText.text = "0:" + Mathf.CeilToInt(timer);
            yield return new WaitForSeconds(1f);
            timer -= 1f;
        }

        // 7. По окончании таймера скрываем экран смерти и таймер,
        // показываем мини-игру и сбрасываем флаг
        respawnCountdownText.gameObject.SetActive(false);
        gameOverPanel.SetActive(false);
        miniGamePanel.SetActive(true);

        isGameOverActive = false;

        // Дополнительно, сюда можно добавить вызов метода рестарта мини-игры
    }

    void RespawnMinigame()
    {
        // Скрываем панель смерти
        gameOverPanel.SetActive(false);
        miniGamePanel.SetActive(true);

        ElectroChainManager.Instance.StartMinigame();

        // Здесь логика сброса мини-игры (например, рестарт цепочки, попыток и т.д.)
        // ElectraChainManager.Instance.Restart(); - если есть такой метод

        Debug.Log("Мин-игра перезапущена");
    }
}
