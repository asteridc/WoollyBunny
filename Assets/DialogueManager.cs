using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class DialogueManager : MonoBehaviour
{
    private List<GameObject> currentChoiceButtons = new List<GameObject>();
    private HashSet<string> usedOptionalChoices = new HashSet<string>();
    private HashSet<string> unlockedChoices = new HashSet<string>();
    private readonly Stack<int> dialogueHistory = new Stack<int>();
    private Coroutine skipCoroutine;
    private bool isSkipping = false;

    private int bloodthirst = 0;
    private int nobility = 0;
    private int love = 0;
    private bool specialChoiceMade = false;

    [Header("Тестовый запуск")]
    [Tooltip("Номер строки (с 1), с которой начать диалог при запуске игры.")]
    [SerializeField] public int startLineNumber = 1;

    [Header("Runtime State")]
    [SerializeField] private string currentBackgroundId;

    [Header("Dialogue Navigation")]
    [SerializeField] private float skipLineDelay = 0.06f;
    [SerializeField] private Button skipButton;
    [SerializeField] private Graphic skipButtonGraphic;
    [SerializeField] private Graphic skipButtonIcon;
    [SerializeField] private Color skipButtonActiveColor = new Color(1f, 0.85f, 0.35f, 1f);
    [SerializeField] private Color skipButtonActiveIconColor = new Color(1f, 0.95f, 0.65f, 1f);

    private Color skipButtonInactiveColor = Color.white;
    private Color skipButtonInactiveIconColor = Color.white;

    [Header("Choice Runtime")]
    private int selectedChoiceIndex = -1;
    private DialogueLine currentChoiceLine;

    public bool skipAutoStart = true;

    [Header("UI Elements")]
    public string currentChapterId;
    public string currentChapterTitle;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;
    public Image backgroundImage;
    [SerializeField] private Image backgroundTransitionImage;
    public Image characterImage;
    public Image radioIcon;
    public GameObject dialoguePanel;

    private Coroutine typingCoroutine;
    private bool isTyping;
    private string fullCurrentLine;
    [SerializeField] private float typeDelay = 0.03f;

    [SerializeField] private Image backgroundFadeImage;
    [SerializeField] private float fadeDuration = 2f;
    [SerializeField] private bool clickToContinueAfterFade = true;
    [SerializeField] private float autoContinueDelay = 1f;
    public bool waitingForClick = false;

    [Header("Continue Hint")]
    [SerializeField] private GameObject continueHintRoot;
    [SerializeField] private Image continueHintIcon;
    [SerializeField] private Color continueHintBaseColor = Color.white;
    [SerializeField] private Color continueHintPulseColor = new Color(1f, 0.85f, 0.35f, 1f);
    [SerializeField] private float continueHintFadeDuration = 0.35f;
    [SerializeField] private float continueHintPulseDuration = 0.75f;
    [SerializeField] private float continueHintScaleMultiplier = 1.06f;
    [SerializeField] private float continueHintShowDelay = 5f;

    private CanvasGroup continueHintCanvasGroup;
    private Tween continueHintFadeTween;
    private Tween continueHintColorTween;
    private Tween continueHintScaleTween;
    private Coroutine continueHintDelayCoroutine;
    private int continueHintRequestVersion;

    [Header("Character Sprites")]
    public Image modelLeft;
    public Image modelRight;

    public Sprite transparent;
    public Sprite eliSprite;
    public Sprite kaneSprite;
    public Sprite hannaSprite;
    public Sprite keremSprite;
    public Sprite firstAssasinPeacekeeper;
    public Sprite keremRadioSprite;
    public Sprite barsSprite;
    public Sprite barsRadioSprite;
    public Sprite sapfirSprite;
    public Sprite rinaSprite;
    public Sprite addictSpriteChapter2_1;
    public Sprite addictSpriteChapter2_2;
    public Sprite nonameSpriteChapter2;
    public Sprite doerSpriteChapter2_1;
    public Sprite doerSpriteChapter2_2;
    public Sprite knightBesideTheDoor;
    public Sprite knight1_Chapter2;
    public Sprite knight2_Chapter2;
    public Sprite knight3_Chapter2;
    public Sprite knight4_Chapter2;
    public Sprite maiden1_Chapter2;
    public Sprite maulerSprite_Chapter2;

    [Header("Hide Dialogue")]
    [SerializeField] private CanvasGroup dialogueCanvasGroup;
    [SerializeField] private float hideShowDuration = 0.35f;

    [SerializeField] private Button hideButton;
    [SerializeField] private Image hideButtonIcon;
    private Color hideBaseColor;
    [SerializeField] private Color hideHoverColor = Color.gray;

    [SerializeField] private CanvasGroup hideTooltipGroup;
    [SerializeField] private float tooltipFadeDuration = 0.2f;
    [SerializeField] private float tooltipDelay = 1f;
    private Tween tooltipTween;

    private bool isDialogueHidden = false;

    [Header("Interaction")]
    public GameObject interactionPointGroup;

    [Header("Choices UI")]
    public GameObject choicesContainer;
    public GameObject choiceButtonPrefab;

    [Header("Story Hints")]
    [SerializeField] private Sprite bloodthirstHintIcon;
    [SerializeField] private Sprite nobilityHintIcon;
    [SerializeField] private Sprite loveHintIcon;
    [SerializeField] private Sprite importantChoiceHintIcon;
    [SerializeField] private Sprite transparentHintIcon;
    [SerializeField] private string choiceHintIconObjectName = "HintIcon";

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
    [SerializeField] private CanvasGroup collectibleCanvasGroup;
    public TextMeshProUGUI collectibleTitleText;
    public TextMeshProUGUI collectibleContentText;
    public Image collectibleIconImage;
    public bool isCollectibleOpen = false;

    [Header("Dialogue Data")]
    public List<DialogueChapter> chapters;
    public DialogueChapter currentChapter;

    [SerializeField] public LocalizedStory storySelector;
    public static bool IsReady;

    

    private void Awake()
    {
        hideBaseColor = hideButtonIcon.color;
        IsReady = false;
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void Start()
    {
        collectiblePanel.SetActive(false);

        InitializeSkipButtonVisuals();
        InitializeContinueHint();
        SetContinueHintVisible(false, true);

        IsReady = true;

        if (!skipAutoStart)
            LoadChapter(currentChapter, startLineNumber - 1);
    }

    public DialogueLine[] runtimeLines;

    public void LoadChapter(DialogueChapter chapter, int lineIndex)
    {
        StopSkippingIfNeeded();
        DialogueChapter resolvedChapter = ResolveChapterForCurrentLanguage(chapter);

        if (resolvedChapter == null) return;

        currentChapter = resolvedChapter;
        currentChapterId = resolvedChapter.ChapterId;
        currentChapterTitle = GetChapterLabel(resolvedChapter.ChapterNumber);

        runtimeLines = new DialogueLine[resolvedChapter.lines.Count];
        for (int i = 0; i < resolvedChapter.lines.Count; i++)
            runtimeLines[i] = resolvedChapter.lines[i].Clone();

        Debug.Log($"[LoadChapter] lineIndex = {lineIndex}");

        currentLineIndex = runtimeLines.Length > 0
            ? Mathf.Clamp(lineIndex, 0, runtimeLines.Length - 1)
            : 0;

        ShowDialoguePanel();
        ShowLine();
    }

    private DialogueChapter ResolveChapterForCurrentLanguage(DialogueChapter chapter)
    {
        DialogueChapter stableChapter = ResolveStableChapter(chapter);

        if (stableChapter == null)
            return null;

        if (storySelector == null)
            return stableChapter;

        DialogueChapter localizedChapter = storySelector.GetFor(stableChapter);
        return localizedChapter != null ? localizedChapter : stableChapter;
    }

    private DialogueChapter ResolveStableChapter(DialogueChapter chapter)
    {
        if (chapter == null)
            return GetDefaultSceneChapter();

        DialogueChapter chapterFromId = FindChapterById(chapter.ChapterId);
        if (chapterFromId != null)
            return chapterFromId;

        return chapter;
    }

    private DialogueChapter ResolveChapterFromSave(DialogueSaveData data)
    {
        if (data == null)
            return null;

        DialogueChapter stableChapter = FindChapterById(data.chapterId);

        if (stableChapter == null)
            stableChapter = FindChapterByLegacyIndex(data.chapterIndex);

        if (stableChapter == null)
            stableChapter = GetDefaultSceneChapter();

        return ResolveChapterForCurrentLanguage(stableChapter);
    }

    private DialogueChapter FindChapterById(string chapterId)
    {
        if (string.IsNullOrWhiteSpace(chapterId) || chapters == null)
            return null;

        return chapters.FirstOrDefault(chapter =>
            chapter != null && chapter.MatchesChapterId(chapterId));
    }

    private DialogueChapter FindChapterByLegacyIndex(int chapterIndex)
    {
        if (chapters == null || chapterIndex < 0 || chapterIndex >= chapters.Count)
            return null;

        return chapters[chapterIndex];
    }

    private DialogueChapter GetDefaultSceneChapter()
    {
        if (currentChapter != null)
        {
            DialogueChapter currentSceneChapter = FindChapterById(currentChapter.ChapterId);
            if (currentSceneChapter != null)
                return currentSceneChapter;

            return currentChapter;
        }

        return chapters != null && chapters.Count > 0
            ? chapters[0]
            : null;
    }

    private int GetStableChapterIndex(DialogueChapter chapter)
    {
        DialogueChapter stableChapter = ResolveStableChapter(chapter);
        return chapters != null ? chapters.IndexOf(stableChapter) : -1;
    }

    private string GetChapterLabel(int chapterNumber)
    {
        string prefix = LanguageManager.CurrentLanguage == Language.English
            ? "Chapter"
            : "Глава";

        return $"{prefix} {chapterNumber}";
    }

    void Update()
    {
        if (!isDialogueHidden) return;

        if (isCollectibleOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseCollectibleView();
            return;
        }

        if (waitingForClick && Input.GetMouseButtonDown(0))
        {
            waitingForClick = false;
            SetContinueHintVisible(false);
            ShowDialoguePanel();
            modelLeft.gameObject.SetActive(true);
            modelRight.gameObject.SetActive(true);

            Debug.Log($"Click after fade = {currentLineIndex}");
            ShowLine(); // Повторный показ той же строки, теперь уже с UI
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
            case "Рация (Керем)": return keremRadioSprite;
            case "Барс": return barsSprite;
            case "Рация (Барс)": return barsRadioSprite;
            case "Сапфир": return sapfirSprite;
            case "Рина": return rinaSprite;
            case "Зависимый 1 (глава 2)": return addictSpriteChapter2_1;
            case "Зависимый 2 (глава 2)": return addictSpriteChapter2_2;
            case "Неизвестный": return nonameSpriteChapter2;
            case "Вершитель 1 (глава 2)": return doerSpriteChapter2_1;
            case "Вершитель 2 (глава 2)": return doerSpriteChapter2_2;
            case "Рыцарь по ту сторону двери": return knightBesideTheDoor;
            case "Рыцарь 1 (глава 2)": return knight1_Chapter2;
            case "Рыцарь 2 (глава 2)": return knight2_Chapter2;
            case "Рыцарь 3 (глава 2)": return knight3_Chapter2;
            case "Рыцарь 4 (глава 2)": return knight4_Chapter2;
            case "Дева 1 (глава 2)": return maiden1_Chapter2;
            case "Силач (глава 2)": return maulerSprite_Chapter2;
            default: return null;
        }
    }

    public bool IsChoiceUnlocked(string key)
    {
        if (string.IsNullOrEmpty(key)) return true; // если ключ пустой — всегда открыт
        return unlockedChoices.Contains(key);
    }
    private void ProcessUnlockKeys(DialogueLine line)
    {
        if (line.unlockChoiceKeys == null) return;

        foreach (var key in line.unlockChoiceKeys)
        {
            if (!string.IsNullOrEmpty(key))
                unlockedChoices.Add(key); // добавляем ключ в HashSet
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
        SetContinueHintVisible(false);

        if (isTyping)
        {
            SkipTyping();
            return;
        }

        // Форсированный переход
        if (forceNextLineIndex >= 0)
        {
            currentLineIndex = forceNextLineIndex;
            forceNextLineIndex = -1;
            ShowLine();
            return;
        }
        // Если выбор — не продолжаем
        if (runtimeLines[currentLineIndex].hasChoices)
            return;

        // Проверка на jump после показа строки
        if (runtimeLines[currentLineIndex].isJumpLine)
        {
            currentLineIndex = runtimeLines[currentLineIndex].gotoLineIndex - 1;
            ShowLine();
            return;
        }

        // Переход к следующей строке по обычной логике
        currentLineIndex++;
        if (currentLineIndex >= currentChapter.lines.Count)
        {
            ShowEndOfChapterPanel();
        }

        ShowLine();
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
        if (runtimeLines == null || runtimeLines.Length == 0)
            return;

        TryCacheSkipButtonFromCurrentSelection();

        if (isSkipping)
        {
            StopSkippingIfNeeded();
            return;
        }

        skipCoroutine = StartCoroutine(SkipDialogueFast());
    }

    private IEnumerator SkipDialogueFast()
    {
        SetSkippingState(true);

        while (currentLineIndex < runtimeLines.Length)
        {
            DialogueLine line = runtimeLines[currentLineIndex];
            if (RequiresPlayerAction(line) || NextStoryStepRequiresPlayerAction(currentLineIndex))
                break;

            if (!TryMoveToNextLineIndex(line))
                break;

            ShowLine();
            yield return new WaitForSeconds(skipLineDelay);
        }

        SetSkippingState(false);
        skipCoroutine = null;
    }

    private bool TryMoveToNextLineIndex(DialogueLine line)
    {
        if (line.isJumpLine)
        {
            currentLineIndex = Mathf.Clamp(line.gotoLineIndex - 1, 0, runtimeLines.Length - 1);
            return true;
        }

        if (currentLineIndex + 1 >= runtimeLines.Length)
            return false;

        currentLineIndex++;
        return true;
    }

    private bool RequiresPlayerAction(DialogueLine line)
    {
        if (line == null)
            return true;

        if (IsScriptedInteractionLine(currentLineIndex))
            return true;

        if (line.hasChoices)
            return true;

        if (line.changeBackground)
            return true;

        if (line.showCollectibleView)
            return true;

        if (line.extraActions == null)
            return false;

        return line.extraActions.showCodePanel
            || line.extraActions.showNotePanel
            || line.extraActions.showElectroSubstationMinigame
            || line.extraActions.showInteractionPoints
            || line.extraActions.stopDialogueAfterThisLine;
    }

    private void StopSkippingIfNeeded()
    {
        if (!isSkipping && skipCoroutine == null)
            return;

        if (skipCoroutine != null)
            StopCoroutine(skipCoroutine);

        SetSkippingState(false);
        skipCoroutine = null;
    }

    private bool NextStoryStepRequiresPlayerAction(int lineIndex)
    {
        int nextLineIndex = GetNextStoryStepLineIndex(lineIndex);
        if (!IsValidRuntimeLineIndex(nextLineIndex))
            return false;

        return RequiresPlayerAction(runtimeLines[nextLineIndex], nextLineIndex);
    }

    private bool RequiresPlayerAction(DialogueLine line, int lineIndex)
    {
        if (line == null)
            return true;

        if (IsScriptedInteractionLine(lineIndex))
            return true;

        if (line.hasChoices || line.changeBackground || line.showCollectibleView)
            return true;

        if (line.extraActions == null)
            return false;

        return line.extraActions.showCodePanel
            || line.extraActions.showNotePanel
            || line.extraActions.showElectroSubstationMinigame
            || line.extraActions.showInteractionPoints
            || line.extraActions.stopDialogueAfterThisLine;
    }

    private int GetNextStoryStepLineIndex(int lineIndex)
    {
        if (!IsValidRuntimeLineIndex(lineIndex))
            return -1;

        DialogueLine line = runtimeLines[lineIndex];
        if (line != null && line.isJumpLine)
            return Mathf.Clamp(line.gotoLineIndex - 1, 0, runtimeLines.Length - 1);

        int nextLineIndex = lineIndex + 1;
        return nextLineIndex < runtimeLines.Length ? nextLineIndex : -1;
    }

    private bool IsValidRuntimeLineIndex(int lineIndex)
    {
        return runtimeLines != null
            && lineIndex >= 0
            && lineIndex < runtimeLines.Length;
    }

    private bool IsScriptedInteractionLine(int lineIndex)
    {
        if (lineIndex < 0)
            return false;

        string sceneName = SceneManager.GetActiveScene().name;

        if (sceneName == "Chapter_02" && lineIndex == 90 - 1)
            return true;

        if (sceneName != "Chapter_01")
            return false;

        if (LanguageManager.CurrentLanguage == Language.Russian && lineIndex == 39 - 1)
            return true;

        return LanguageManager.CurrentLanguage == Language.English && lineIndex == 49 - 1;
    }

    private void SetSkippingState(bool skipping)
    {
        isSkipping = skipping;
        UpdateSkipButtonVisualState();
    }

    private void InitializeSkipButtonVisuals()
    {
        ResolveSkipButtonReferences();

        if (skipButtonGraphic != null)
            skipButtonInactiveColor = skipButtonGraphic.color;

        if (skipButtonIcon != null)
            skipButtonInactiveIconColor = skipButtonIcon.color;

        UpdateSkipButtonVisualState();
    }

    private void TryCacheSkipButtonFromCurrentSelection()
    {
        if (EventSystem.current == null || EventSystem.current.currentSelectedGameObject == null)
            return;

        Button selectedButton = EventSystem.current.currentSelectedGameObject.GetComponentInParent<Button>();
        if (selectedButton == null)
            return;

        skipButton = selectedButton;
        ResolveSkipButtonReferences();

        if (!isSkipping)
        {
            if (skipButtonGraphic != null)
                skipButtonInactiveColor = skipButtonGraphic.color;

            if (skipButtonIcon != null)
                skipButtonInactiveIconColor = skipButtonIcon.color;
        }

        UpdateSkipButtonVisualState();
    }

    private void ResolveSkipButtonReferences()
    {
        if (skipButton == null)
        {
            Button[] buttons = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (Button button in buttons)
            {
                if (!IsSkipButton(button))
                    continue;

                skipButton = button;
                break;
            }
        }

        if (skipButton == null)
            return;

        if (skipButtonGraphic == null)
            skipButtonGraphic = skipButton.targetGraphic != null
                ? skipButton.targetGraphic
                : skipButton.GetComponent<Graphic>();

        if (skipButtonIcon == null)
        {
            Graphic[] graphics = skipButton.GetComponentsInChildren<Graphic>(true);
            skipButtonIcon = graphics.FirstOrDefault(graphic =>
                graphic != null
                && graphic != skipButtonGraphic
                && graphic.gameObject != skipButton.gameObject);
        }
    }

    private bool IsSkipButton(Button button)
    {
        if (button == null)
            return false;

        for (int i = 0; i < button.onClick.GetPersistentEventCount(); i++)
        {
            if (button.onClick.GetPersistentTarget(i) == this
                && button.onClick.GetPersistentMethodName(i) == nameof(OnClickSkip))
                return true;
        }

        return false;
    }

    private void UpdateSkipButtonVisualState()
    {
        if (skipButtonGraphic != null)
            skipButtonGraphic.color = isSkipping ? skipButtonActiveColor : skipButtonInactiveColor;

        if (skipButtonIcon != null)
            skipButtonIcon.color = isSkipping ? skipButtonActiveIconColor : skipButtonInactiveIconColor;
    }

    private void InitializeContinueHint()
    {
        if (continueHintRoot == null)
            return;

        continueHintCanvasGroup = continueHintRoot.GetComponent<CanvasGroup>();
        if (continueHintCanvasGroup == null)
            continueHintCanvasGroup = continueHintRoot.AddComponent<CanvasGroup>();

        if (continueHintIcon == null)
            continueHintIcon = continueHintRoot.GetComponentInChildren<Image>(true);

        continueHintCanvasGroup.alpha = 0f;
        continueHintRoot.SetActive(false);
    }

    private void SetContinueHintVisible(bool visible, bool immediate = false)
    {
        if (continueHintRoot == null)
            return;

        if (continueHintDelayCoroutine != null)
        {
            StopCoroutine(continueHintDelayCoroutine);
            continueHintDelayCoroutine = null;
        }

        continueHintRequestVersion++;

        continueHintFadeTween?.Kill();

        if (!visible)
        {
            continueHintColorTween?.Kill();
            continueHintScaleTween?.Kill();

            if (continueHintIcon != null)
            {
                continueHintIcon.color = continueHintBaseColor;
                continueHintIcon.rectTransform.localScale = Vector3.one;
            }

            if (immediate || continueHintCanvasGroup == null)
            {
                if (continueHintCanvasGroup != null)
                    continueHintCanvasGroup.alpha = 0f;
                continueHintRoot.SetActive(false);
                return;
            }

            continueHintFadeTween = continueHintCanvasGroup
                .DOFade(0f, continueHintFadeDuration)
                .OnComplete(() => continueHintRoot.SetActive(false));

            return;
        }

        if (!immediate && continueHintShowDelay > 0f)
        {
            continueHintRoot.SetActive(false);
            if (continueHintCanvasGroup != null)
                continueHintCanvasGroup.alpha = 0f;

            continueHintDelayCoroutine = StartCoroutine(ShowContinueHintWithDelay(continueHintRequestVersion, currentLineIndex));
            return;
        }

        ShowContinueHintNow(immediate);
    }

    private IEnumerator ShowContinueHintWithDelay(int requestVersion, int requestLineIndex)
    {
        yield return new WaitForSeconds(continueHintShowDelay);
        continueHintDelayCoroutine = null;

        if (requestVersion != continueHintRequestVersion)
            yield break;

        if (runtimeLines == null || currentLineIndex != requestLineIndex)
            yield break;

        if (isTyping || isDialogueHidden)
            yield break;

        ShowContinueHintNow(false);
    }

    private void ShowContinueHintNow(bool immediate)
    {
        continueHintRoot.SetActive(true);

        if (continueHintCanvasGroup != null)
        {
            continueHintCanvasGroup.alpha = immediate ? 1f : 0f;
            if (!immediate)
                continueHintFadeTween = continueHintCanvasGroup.DOFade(1f, continueHintFadeDuration);
        }

        if (continueHintIcon == null)
            return;

        continueHintIcon.color = continueHintBaseColor;
        continueHintColorTween = continueHintIcon
            .DOColor(continueHintPulseColor, continueHintPulseDuration)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);

        continueHintIcon.rectTransform.localScale = Vector3.one;
        continueHintScaleTween = continueHintIcon.rectTransform
            .DOScale(continueHintScaleMultiplier, continueHintPulseDuration)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    public void JumpToLine(int lineIndex) // в случае интерактива
    {
        if (lineIndex >= 0 && lineIndex < currentChapter.lines.Count)
        {
            currentLineIndex = lineIndex - 1;
            ShowNextLine();
        }
        else
        {
            Debug.LogWarning($"JumpToLine: недопустимый индекс строки {lineIndex}");
        }
    }

    private int forceNextLineIndex = -1;
    public GameObject guitarInteractiveUI;

    public void ShowLine()
    {
        ShowLine(true);
    }

    private void ShowLine(bool saveToHistory)
    {
        Debug.Log($"ShowLine: currentLineIndex = {currentLineIndex}");
        if (runtimeLines == null || runtimeLines.Length == 0)
            return;

        SetContinueHintVisible(false);

        currentLineIndex = Mathf.Clamp(currentLineIndex, 0, runtimeLines.Length - 1);

        if (saveToHistory)
        {
            if (dialogueHistory.Count == 0 || dialogueHistory.Peek() != currentLineIndex)
                dialogueHistory.Push(currentLineIndex);
        }

        DialogueLine line = runtimeLines[currentLineIndex]; 

        // ============= УСЛОВИЯ ДЛЯ ЗАСКРИПТОВАННЫХ МОМЕНТОВ В СЮЖЕТЕ ============== //
        // Выбор ответа - 1(2) + преобладающий путь => показ мыслей Илая после разговора с Ханной.
        if (SceneManager.GetActiveScene().name == "Chapter_02")
        {
            if (currentLineIndex == 75 - 1)
            {
                if (specialChoiceMade)
                {
                    if (bloodthirst > love && bloodthirst > nobility)
                    {
                        currentLineIndex = 76 - 1;
                        line = runtimeLines[currentLineIndex];
                        forceNextLineIndex = 78 - 1;
                    }
                    else if (love > bloodthirst && love > nobility)
                    {
                        currentLineIndex = 77 - 1;
                        line = runtimeLines[currentLineIndex];
                    }
                    else if (nobility > bloodthirst && nobility > love)
                    {
                        currentLineIndex = 77 - 1;
                        line = runtimeLines[currentLineIndex];
                    }
                }
                else
                {
                    currentLineIndex = 78 - 1;
                    line = runtimeLines[currentLineIndex];
                }
            }
        }


        
        
        // Мини-сцена с гитарой в квартире //

        if (SceneManager.GetActiveScene().name == "Chapter_02" && IsScriptedInteractionLine(currentLineIndex))
        {
            if (currentLineIndex == 90 - 1)
            {
                Debug.Log("Запуск мини-интерактива с гитарой!");

                // Останавливаем показ диалога, чтобы не шло дальше
                HideDialoguePanel();

                // Включаем UI гитарного интерактива
                guitarInteractiveUI.SetActive(true);

                // Запускаем сам интерактив через скрипт на UI
                guitarInteractiveUI.GetComponent<GuitarSceneManager>().StartGuitarScene();

                // Возвращаемся из метода, чтобы не показать текст этой строки
                return;
            }
        }



        // Показ мини-игры с кодовой панелью //
        if (SceneManager.GetActiveScene().name == "Chapter_01" && IsScriptedInteractionLine(currentLineIndex))
        {
            if (LanguageManager.CurrentLanguage == Language.Russian && currentLineIndex == 39 - 1)
            {
                Debug.Log("Показ мини-игры с кодовой панелью!");
                // Скрываем диалоговую панель
                HideDialoguePanel(() =>
                {
                    // Показываем кодовую панель
                    if (codePanelUI != null)
                    {
                        codePanelUI.ShowCodePanel();
                    }
                    else
                    {
                        Debug.LogError("codePanelUI не найден в сцене!");
                    }
                });
                // Возвращаемся из метода, чтобы не показать текст этой строки
                return;
            }

            if (LanguageManager.CurrentLanguage == Language.English && currentLineIndex == 49 - 1)
            {
                Debug.Log("Показ мини-игры с кодовой панелью!");
                // Скрываем диалоговую панель
                HideDialoguePanel(() =>
                {
                    // Показываем кодовую панель
                    if (codePanelUI != null)
                    {
                        codePanelUI.ShowCodePanel();
                    }
                    else
                    {
                        Debug.LogError("codePanelUI не найден в сцене!");
                    }
                });
                // Возвращаемся из метода, чтобы не показать текст этой строки
                return;
            }
        }

        // ============= КОНЕЦ ЗАСКРИПТОВАННЫХ УСЛОВИЙ ============= //



        if (line.changeBackground && line.backgroundSprite != null && !illustrationTransitionCompleted)
        {
            HideDialoguePanel(() =>
            {
                switch (line.backgroundTransition)
                {
                    case BackgroundTransitionType.LocationFade:
                        StartCoroutine(HandleLocationTransition(line));
                        break;

                    case BackgroundTransitionType.IllustrationFade:
                        StartCoroutine(HandleIllustrationTransition(line));
                        break;
                }
            });

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
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        nameText.text = line.speakerName;
        DialogueLine.ExtraActions extraActions = line.extraActions;

        fullCurrentLine = finalText;
        typingCoroutine = StartCoroutine(TypeLine(fullCurrentLine));


        if (extraActions != null && extraActions.showCodePanel)
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

        if (extraActions != null && extraActions.showInteractionPoints)
        {
            ShowInteractionPoints();
        }

        // Показываем персонажей
        ShowCharacters(line);

        // Отображение иконки рации у говорящего в определенных сюжетных моментах
        if (radioIcon != null)
        {
            if (line.isRadio)
            {
                radioIcon.gameObject.SetActive(true);
            }
            else
            {
                radioIcon.gameObject.SetActive(false);
            }
        }

        ProcessUnlockKeys(line);

        // Показываем выбор
        if (line.hasChoices)
            ShowChoices(line.choices);

        // Дополнительные действия
        if (extraActions != null)
        {
            if (extraActions.showCodePanel)
            {
                if (codePanelUI != null)
                {
                    codePanelUI.ShowCodePanel();
                }
                else
                {
                    Debug.LogError("codePanelUI is not assigned in DialogueManager!");
                }

                if (extraActions.stopDialogueAfterThisLine)
                    return;
            }

            if (extraActions.showNotePanel)
            {
                CodePanelUI codePanel = FindFirstObjectByType<CodePanelUI>();
                if (codePanel != null)
                {
                    codePanel.ToggleNotePanel();
                }
            }

            if (extraActions.objectToActivate != null)
            {
                extraActions.objectToActivate.SetActive(true);
            }

            if (extraActions.showElectroSubstationMinigame)
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

            if (line.isEndOfChapter)
            {
                ShowEndOfChapterPanel();
            }
            

            bool canGoToNextReplicaByClick = !line.hasChoices
        && !line.showCollectibleView
        && (extraActions == null || (!extraActions.showCodePanel
            && !extraActions.showNotePanel
            && !extraActions.showElectroSubstationMinigame
            && !extraActions.showInteractionPoints
            && !extraActions.stopDialogueAfterThisLine));

            SetContinueHintVisible(canGoToNextReplicaByClick);

        }
        illustrationTransitionCompleted = false;
    }

    private void StartTypewriter(string line)
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        fullCurrentLine = line;
        typingCoroutine = StartCoroutine(TypeLine(line));
    }

    private IEnumerator TypeLine(string line)
    {
        isTyping = true;
        dialogueText.text = "";

        for (int i = 1; i <= line.Length; i++)
        {
            dialogueText.text = line.Substring(0, i);
            yield return new WaitForSeconds(typeDelay);
        }

        isTyping = false;
        typingCoroutine = null;
    }
    public void SkipTyping()
    {
        if (!isTyping) return;

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        dialogueText.text = fullCurrentLine ?? "";
        isTyping = false;
    }

    private Tween dialogueTween;
    [SerializeField] public CanvasGroup dialogueGroup;

    public void ShowDialoguePanel()
    {
        isDialogueHidden = false;
        dialoguePanel.SetActive(true);

        dialogueTween?.Kill();

        dialogueGroup.interactable = true;
        dialogueGroup.blocksRaycasts = true;

        dialogueGroup.alpha = 0f;
        dialogueGroup.transform.localScale = Vector3.one * 0.98f;

        dialogueTween = DOTween.Sequence()
            .Append(dialogueGroup.DOFade(1f, 0.25f))
            .Join(dialogueGroup.transform.DOScale(1f, 0.25f).SetEase(Ease.OutQuad));
    }

    public void HideDialoguePanel(System.Action onComplete = null)
    {
        StopSkippingIfNeeded();
        isDialogueHidden = true;
        dialogueTween?.Kill();

        dialogueGroup.interactable = false;
        dialogueGroup.blocksRaycasts = false;


        dialogueTween = DOTween.Sequence()
            .Append(dialogueGroup.DOFade(0f, 0.2f))
            .Join(dialogueGroup.transform.DOScale(0.98f, 0.2f).SetEase(Ease.InQuad))
            .OnComplete(() =>
            {
                dialoguePanel.SetActive(false);
                onComplete?.Invoke();
            });
    }

    [SerializeField] public GameObject hideClickCatcher;

    public void OnGlobalScreenClick()
    {
        if (!isDialogueHidden) return;

        hideClickCatcher.SetActive(false);
        ShowDialoguePanel();
    }

    public void OnHideButtonClicked()
    {
        HideDialoguePanel();
        hideClickCatcher.SetActive(true);
    }


    public void ShowInteractionPoints()
    {
        if (interactionPointGroup == null)
        {
            Debug.LogWarning("interactionPointGroup не назначен");
            return;
        }

        HideDialoguePanel(() =>
        {
            interactionPointGroup.SetActive(true);

            var points = interactionPointGroup.GetComponentsInChildren<InteractionPoint>(true);

            int shownCount = 0;
            foreach (var point in points)
            {
                if (!point.wasUsed || point.isRepeatable)
                {
                    point.Show();
                    shownCount++;
                }
                else
                {
                    point.Hide();
                }
            }

            Debug.Log($"Показано точек: {shownCount}");
        });
    }


    [SerializeField] private CanvasGroup groupLeft;
    [SerializeField] private CanvasGroup groupRight;

    private string prevSpeakerName = "";
    private string prevListenerName = "";

    private void ShowCharacters(DialogueLine line)
    {
        Sprite speakerSprite = GetSpriteByName(line.characterName);
        Sprite listenerSprite = GetSpriteByName(line.listenerCharacterName);

        Color dim = new Color(1f, 1f, 1f, 0.55f);

        bool leftUsed =
            (!line.isOnRight && speakerSprite != null) ||
            (!line.isListenerOnRight && !string.IsNullOrEmpty(line.listenerCharacterName) && listenerSprite != null);

        bool rightUsed =
            (line.isOnRight && speakerSprite != null) ||
            (line.isListenerOnRight && !string.IsNullOrEmpty(line.listenerCharacterName) && listenerSprite != null);

        if (!leftUsed) RemoveCharacter(false);
        if (!rightUsed) RemoveCharacter(true);

        void AnimateCharacter(Image img, CanvasGroup grp, Sprite sprite, float scaleMagnitude, float targetAlpha, bool skipAnim, bool flip)
        {
            img.transform.DOKill();
            grp.DOKill();

            img.sprite = sprite;

            // ---- FIX: flip только один раз, без анимации ----
            Vector3 currentScale = img.rectTransform.localScale;
            currentScale.x = flip ? -Mathf.Abs(currentScale.x) : Mathf.Abs(currentScale.x);
            img.rectTransform.localScale = currentScale;

            if (skipAnim)
                return;

            // ---- Анимация — только изменение размера, без вращения ----
            Vector3 targetScale = new Vector3(
                (flip ? -1f : 1f) * scaleMagnitude,
                scaleMagnitude,
                1f
            );

            img.rectTransform.DOScale(targetScale, 0.3f).SetEase(Ease.OutQuad);
            grp.DOFade(targetAlpha, 0.3f).SetEase(Ease.OutQuad);
        }

        // -------- ГОВОРЯЩИЙ --------
        if (speakerSprite != null)
        {
            Image img = line.isOnRight ? modelRight : modelLeft;
            CanvasGroup grp = line.isOnRight ? groupRight : groupLeft;

            bool sameSpeaker = line.characterName == prevSpeakerName;

            AnimateCharacter(
                img,
                grp,
                speakerSprite,
                1f,           // активный размер
                1f,           // полная альфа
                sameSpeaker,
                line.flipSpeakerImage
            );
        }

        // -------- СЛУШАЮЩИЙ --------
        if (!string.IsNullOrEmpty(line.listenerCharacterName) && listenerSprite != null)
        {
            Image img = line.isListenerOnRight ? modelRight : modelLeft;
            CanvasGroup grp = line.isListenerOnRight ? groupRight : groupLeft;

            bool sameListener = line.listenerCharacterName == prevListenerName;

            AnimateCharacter(
                img,
                grp,
                listenerSprite,
                0.95f,        // чуть меньше
                0.55f,        // тусклый
                sameListener,
                line.flipListenerImage
            );
        }

        prevSpeakerName = line.characterName;
        prevListenerName = line.listenerCharacterName;
    }




    // --- Метод удаления персонажей ---
    private void RemoveCharacter(bool isRight)
    {
        Image targetImage = isRight ? modelRight : modelLeft;
        CanvasGroup targetGroup = isRight ? groupRight : groupLeft;

        if (targetImage.sprite == transparent || targetGroup.alpha <= 0f) return;

        targetGroup.DOFade(0f, 0.2f);
        targetImage.rectTransform.DOScale(Vector3.zero, 0.35f).SetEase(Ease.InQuad)
            .OnComplete(() =>
            {
                targetImage.sprite = transparent;
                targetImage.rectTransform.localScale = Vector3.one;
            });
    }



    private void ShowChoices(DialogueLine.Choice[] choices)
    {
        // Защитная проверка
        if (choices == null || choices.Length == 0)
        {
            choicesContainer.SetActive(false);
            return;
        }

        // Текущая линия (для фоллбэков и проверки)
        var currentLine = runtimeLines[currentLineIndex];
        currentChoiceLine = currentLine;
        selectedChoiceIndex = -1;


        // 1) Логическая проверка: есть ли хоть один логически доступный выбор?
        bool anyUnlocked = currentLine.choices.Any(c => IsChoiceUnlocked(c) && (c.choiceType != ChoiceType.Optional || !usedOptionalChoices.Contains(c.choiceText)));
        if (!anyUnlocked)
        {
            // Ничего доступного — делаем фоллбэк (если включён)
            if (currentLine.useFallbackIfNoRequired)
            {
                Debug.Log("ShowChoices: нет доступных выборов -> фоллбэк");
                choicesContainer.SetActive(false);
                currentLineIndex = currentLine.fallbackLineIndex - 1;
                ShowLine();
                return;
            }
            else
            {
                // Если фолбэк не включён — просто не показываем кнопки
                choicesContainer.SetActive(false);
                return;
            }
        }

        // 2) Фильтруем визуально: не показываем optional, которые уже использованы
        var filteredChoices = currentLine.choices
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
            var bg = buttonObj.GetComponent<Image>();
            var btn = buttonObj.GetComponent<Button>();

            // LockImage внутри префаба (если есть)
            var lockImage = buttonObj.transform.Find("LockImage")?.GetComponent<Image>();
            if (lockImage != null) lockImage.gameObject.SetActive(false);

            // Проверяем, логически разблокирован ли выбор
            bool isUnlocked = IsChoiceUnlocked(choice);

            if (!isUnlocked)
            {
                // Показываем как заблокированную кнопку (замок) — но НЕ учитываем её как доступную
                buttonText.text = ""; // скрываем основной текст
                btn.interactable = false;
                bg.color = new Color(0.15f, 0.15f, 0.15f);

                if (lockImage != null)
                {
                    // Если в Choice задан lockIcon — используем его, иначе доверяем префабному спрайту
                    if (choice.lockImage != null)
                        lockImage.sprite = choice.lockImage;

                    lockImage.gameObject.SetActive(true);
                    // центрируем, если нужно
                    lockImage.rectTransform.anchoredPosition = Vector2.zero;
                }

                continue;
            }

            // Открытый вариант — отображаем полностью
            buttonText.text = choice.choiceText;
            btn.interactable = true;

            ConfigureChoiceHintIcon(buttonObj, choice, currentLine);

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

            AddHoverEffects(btn, bg, buttonText, choice.choiceType);

            // Важно: захватываем локальную переменную для лямбды
            var capturedChoice = choice;
            var capturedButtonObj = buttonObj;

            btn.onClick.AddListener(() =>
            {
                OnChoiceSelected(capturedChoice);

                if (capturedChoice.choiceType == ChoiceType.Optional)
                {
                    usedOptionalChoices.Add(capturedChoice.choiceText);
                    capturedButtonObj.SetActive(false);
                    RepositionChoices();
                }

                // После выбора проверяем фоллбэк на текущей линии
                CheckFallbackCondition();
            });
        }
    }


    // Возвращает, считается ли выбор логически разблокированным (для принятия решений)
    private bool IsChoiceUnlocked(DialogueLine.Choice c)
    {
        if (c == null) return false;
        if (!c.isLocked) return true;
        if (!string.IsNullOrEmpty(c.unlockKey) && unlockedChoices.Contains(c.unlockKey)) return true;
        return false;
    }

    // Возвращает список "доступных" choices для логики (не учитываем уже выбранные optional)
    private List<DialogueLine.Choice> GetAvailableChoicesForLogic(DialogueLine line)
    {
        if (line == null || line.choices == null) return new List<DialogueLine.Choice>();

        return line.choices
            .Where(c =>
                // не учитываем уже выбранные optional
                (c.choiceType != ChoiceType.Optional || !usedOptionalChoices.Contains(c.choiceText))
                // и учитываем только логически разблокированные
                && IsChoiceUnlocked(c)
            )
            .ToList();
    }

    private void ConfigureChoiceHintIcon(GameObject buttonObj, DialogueLine.Choice choice, DialogueLine line)
    {
        Image hintImage = buttonObj.transform.Find(choiceHintIconObjectName)?.GetComponent<Image>();
        if (hintImage == null)
            return;

        if (!StoryHintsSettings.IsEnabled)
        {
            hintImage.enabled = false;
            hintImage.sprite = transparentHintIcon != null ? transparentHintIcon : transparent;
            return;
        }

        Sprite hintSprite = GetChoiceHintSprite(choice, line);
        hintImage.enabled = true;
        hintImage.sprite = hintSprite != null
            ? hintSprite
            : (transparentHintIcon != null ? transparentHintIcon : transparent);
    }

    private Sprite GetChoiceHintSprite(DialogueLine.Choice choice, DialogueLine line)
    {
        if (line != null && line.extraActions != null && line.extraActions.isImportantStoryChoice)
            return importantChoiceHintIcon;

        DominantPath rewardPath = ResolveChoiceRewardPath(choice);
        switch (rewardPath)
        {
            case DominantPath.Bloodthirst:
                return bloodthirstHintIcon;
            case DominantPath.Nobility:
                return nobilityHintIcon;
            case DominantPath.Love:
                return loveHintIcon;
            default:
                return transparentHintIcon != null ? transparentHintIcon : transparent;
        }
    }

    private DominantPath ResolveChoiceRewardPath(DialogueLine.Choice choice)
    {
        if (choice == null)
            return DominantPath.None;

        if (choice.pathReward != DominantPath.None)
            return choice.pathReward;

        int blood = Mathf.Max(0, choice.pathPointsBloodthirsty);
        int noble = Mathf.Max(0, choice.pathPointsNoble);
        int lovePoints = Mathf.Max(0, choice.pathPointsLove);

        int maxPoints = Mathf.Max(blood, noble, lovePoints);
        if (maxPoints <= 0)
            return DominantPath.None;

        int maxCount = 0;
        if (blood == maxPoints) maxCount++;
        if (noble == maxPoints) maxCount++;
        if (lovePoints == maxPoints) maxCount++;

        if (maxCount > 1)
            return DominantPath.None;

        if (blood == maxPoints) return DominantPath.Bloodthirst;
        if (noble == maxPoints) return DominantPath.Nobility;
        return DominantPath.Love;
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
                new Vector2(-160, 60),
                new Vector2(160, 60),
                new Vector2(-160, -60),
                new Vector2(160, -60)
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

        if (activeButtons.Count == 0)
        {
            CheckFallbackCondition();
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

    private void RemoveHoverEffects(Button btn)
    {
        var trigger = btn.GetComponent<EventTrigger>();
        if (trigger != null)
            Destroy(trigger);
    }


    public void OnChoiceSelected(DialogueLine.Choice choice)
    {
        selectedChoiceIndex = Array.IndexOf(
        runtimeLines[currentLineIndex].choices,
        choice
        );
        DominantPath rewardPath = ResolveChoiceRewardPath(choice);

        switch (rewardPath)
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

        Debug.Log($"Выбран путь: {rewardPath}");
        Debug.Log($"Очки путей: Кровожадность = {bloodthirst}, Благородство = {nobility}, Любовь = {love}");

        var dominant = GetDominantPaths();
        Debug.Log("Преобладающий путь(и): " + string.Join(", ", dominant));

        // Проверка "условного" выбора
        if (choice.isSpecialChoice)
        {
            specialChoiceMade = true;
            Debug.Log("Особый выбор совершен игроком");
        }

        if (choice.choiceType == ChoiceType.Optional)
        {
            usedOptionalChoices.Add(choice.choiceText);
        }
        currentLineIndex = choice.nextLineIndex - 1;
        choicesContainer.SetActive(false);
        ShowLine();
        CheckFallbackCondition();
    }

    private void CheckFallbackCondition()
    {
        // Берём текущую строку (ту, в которой мы ожидаем выбор)
        var line = runtimeLines[currentLineIndex];

        // Если фолбэк не включён — ничего не делаем
        if (!line.useFallbackIfNoRequired)
            return;

        // Список доступных для логики (не учитывая закрытые и уже выбранные optional)
        var available = GetAvailableChoicesForLogic(line);

        bool hasRequired = line.choices.Any(c => c.choiceType == ChoiceType.Required && IsChoiceUnlocked(c));

        // Условие: нет доступных required и нет доступных (разблокированных и не выбранных) optional
        if (!hasRequired && available.Count == 0)
        {
            Debug.Log("🟣 Fallback: нет доступных выборов — переход на " + line.fallbackLineIndex);
            // Скрываем UI выбора на всякий случай
            choicesContainer.SetActive(false);

            // Переходим на указанную в инспекторе строку
            currentLineIndex = line.fallbackLineIndex;
            ShowLine();
        }
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

    [SerializeField] private GameObject[] characterObjects;

    private void ShowCollectibleView(string title, string content, Sprite icon)
    {
        isCollectibleOpen = true;
        // 1️⃣ Скрываем диалог с анимацией, а после запускаем открытие коллекции
        HideDialoguePanel(() =>
        {
            // 2️⃣ Подготовка панели коллекции
            collectiblePanel.SetActive(true);
            collectibleCanvasGroup.alpha = 0f;
            collectibleCanvasGroup.interactable = false;
            collectibleCanvasGroup.blocksRaycasts = false;

            // 3️⃣ Устанавливаем содержимое
            collectibleTitleText.text = title;
            collectibleContentText.text = content;
            collectibleIconImage.sprite = icon;

            // 5️⃣ Плавное появление текста, иконки и крестика с задержкой
            DOTween.Sequence()
                .AppendInterval(0.25f) // задержка, чтобы скрытие диалога успело завершиться
                .Append(collectibleCanvasGroup.DOFade(1f, 0.25f))
                .OnComplete(() =>
                {
                    collectibleCanvasGroup.interactable = true;
                    collectibleCanvasGroup.blocksRaycasts = true;
                });
        });
    }

    private void CloseCollectibleView()
    {
        if (!isCollectibleOpen) return; // защита от двойного закрытия

        collectibleCanvasGroup.interactable = false;
        collectibleCanvasGroup.blocksRaycasts = false;

        collectibleCanvasGroup.DOFade(0f, 0.25f).OnComplete(() =>
        {
            collectiblePanel.SetActive(false);
            isCollectibleOpen = false;
            // 3️⃣ После скрытия коллекции — показываем диалог с анимацией
            ShowDialoguePanel();
        });
    }






    public TMP_FontAsset defaultFont;

    [SerializeField] private CanvasGroup storyNotificationGroup;
    [SerializeField] private float notificationFadeDuration = 0.25f;

    private Coroutine storyNotificationCoroutine;


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
        // --- Форматирование текста ---
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

        // --- Подготовка панели ---
        storyNotificationPanel.SetActive(true);

        storyNotificationGroup.DOKill();
        storyNotificationGroup.alpha = 0f;

        // (необязательно, но приятно)
        storyNotificationPanel.transform.DOKill();
        storyNotificationPanel.transform.localScale = Vector3.one * 0.95f;

        // --- Анимация появления ---
        storyNotificationGroup.DOFade(1f, notificationFadeDuration).SetEase(Ease.OutQuad);
        storyNotificationPanel.transform
            .DOScale(1f, notificationFadeDuration)
            .SetEase(Ease.OutQuad);

        // --- Планируем скрытие ---
        if (storyNotificationCoroutine != null)
        {
            StopCoroutine(storyNotificationCoroutine);
            storyNotificationCoroutine = null;
        }

        storyNotificationCoroutine =
            StartCoroutine(HideStoryNotificationAfterDelay(duration));
    }

    private IEnumerator HideStoryNotificationAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        storyNotificationGroup.DOKill();
        storyNotificationPanel.transform.DOKill();

        storyNotificationGroup
            .DOFade(0f, notificationFadeDuration)
            .SetEase(Ease.InQuad)
            .OnComplete(() =>
            {
                storyNotificationPanel.SetActive(false);
            });
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

    private IEnumerator HandleLocationTransition(DialogueLine line)
    {
        line.changeBackground = false;

        yield return StartCoroutine(FadeToBlack());

        HideDialoguePanel();
        modelLeft.gameObject.SetActive(false);
        modelRight.gameObject.SetActive(false);

        backgroundImage.sprite = line.backgroundSprite;
        currentBackgroundId = line.backgroundId; // 🔥 ВАЖНО

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

    private bool illustrationTransitionCompleted = false;

    private IEnumerator HandleIllustrationTransition(DialogueLine line)
    {
        Debug.Log($"Fade line = {currentLineIndex}");
        backgroundTransitionImage.DOKill();
        backgroundImage.DOKill();

        backgroundTransitionImage.sprite = line.backgroundSprite;

        Color top = backgroundTransitionImage.color;
        top.a = 0f;
        backgroundTransitionImage.color = top;

        Color bottom = backgroundImage.color;
        bottom.a = 1f;
        backgroundImage.color = bottom;

        Sequence sequence = DOTween.Sequence();

        sequence.Join(backgroundTransitionImage.DOFade(1f, 1.2f).SetEase(Ease.InOutSine));
        sequence.Join(backgroundImage.DOFade(0f, 1.2f).SetEase(Ease.InOutSine));

        yield return sequence.WaitForCompletion();
        illustrationTransitionCompleted = true;

        backgroundImage.sprite = line.backgroundSprite;

        backgroundImage.color = Color.white;

        Color c = backgroundTransitionImage.color;
        c.a = 0f;
        backgroundTransitionImage.color = c;

        ShowDialoguePanel();
        nameText.text = line.speakerName;

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        fullCurrentLine = line.text;
        typingCoroutine = StartCoroutine(TypeLine(fullCurrentLine));

        ShowCharacters(line);

        if (line.changeCharacterSprite && line.characterSprite != null)
            characterImage.sprite = line.characterSprite;

        if (clickToContinueAfterFade)
        {
            waitingForClick = true;
        }
        else
        {
            yield return new WaitForSeconds(autoContinueDelay);
            ShowLine();
        }
        Debug.Log($"Fade finished = {currentLineIndex}");
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

    public bool isGameOverActive = false;

    private void ResetBlackOverlay()
    {
        if (blackOverlay == null)
            return;

        if (!blackOverlay.gameObject.activeSelf)
            blackOverlay.gameObject.SetActive(true);

        Color color = blackOverlay.color;
        color.a = 0f;
        blackOverlay.color = color;
        blackOverlay.raycastTarget = false;
    }

    public void TriggerGameOver()
    {
        if (isGameOverActive) return;
        isGameOverActive = true;

        ResetBlackOverlay();

        if (respawnCountdownText != null)
            respawnCountdownText.gameObject.SetActive(true);

        StartCoroutine(RespawnCountdown());
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
        ElectroChainManager.Instance.ResetMinigameState();
        gameOverPanel.SetActive(false);
        miniGamePanel.SetActive(true);
        ResetBlackOverlay();

        ResetGameOverFlag();

        // Дополнительно, сюда можно добавить вызов метода рестарта мини-игры
    }

    public void ResetGameOverFlag()
    {
        isGameOverActive = false;
    }

    public GameObject endOfChapterPanel;

    public void ShowEndOfChapterPanel()
    {
        StartCoroutine(ShowEndOfChapterRoutine());
    }

    public void OnContinueToMainMenu()
    {
        StartCoroutine(OnContinueToNextChapter());
    }

    private IEnumerator ShowEndOfChapterRoutine()
    {
        // Плавное затемнение
        blackOverlay.gameObject.SetActive(true);
        yield return blackOverlay.DOFade(1f, FadeDuration).WaitForCompletion();

        // Отключаем диалог и выборы
        HideDialoguePanel();
        choicesContainer.SetActive(false);

        // Показываем панель конца главы
        endOfChapterPanel.SetActive(true);

        // Плавное осветление
        yield return blackOverlay.DOFade(0f, FadeDuration).WaitForCompletion();

        blackOverlay.gameObject.SetActive(false);
    }

    public IEnumerator OnContinueToNextChapter()
    {
        float fadeDurationToMenu = 6f; // другая длительность для этой анимации
        yield return blackOverlay.DOFade(1f, fadeDurationToMenu).WaitForCompletion();
        SceneManager.LoadScene("woollybunny_PC");
    }

    public DialogueSaveData CaptureDialogueState()
    {
        DialogueChapter stableChapter = ResolveStableChapter(currentChapter);

        var data = new DialogueSaveData
        {
            chapterId = stableChapter != null ? stableChapter.ChapterId : string.Empty,
            chapterNumber = stableChapter != null ? stableChapter.ChapterNumber : 0,
            chapterIndex = GetStableChapterIndex(stableChapter),
            lineIndex = currentLineIndex,
            backgroundId = currentBackgroundId,
            choiceState = CaptureChoiceState(),

            nobility = nobility,
            bloodthirst = bloodthirst,
            love = love
        };

        return data;
    }

    public void RestoreDialogueState(DialogueSaveData data)
    {
        if (data == null)
            return;

        DialogueChapter restoredChapter = ResolveChapterFromSave(data);
        if (restoredChapter == null)
        {
            Debug.LogError("[SAVE] Failed to resolve chapter for save data.");
            return;
        }

        nobility = data.nobility;
        bloodthirst = data.bloodthirst;
        love = data.love;

        LoadChapter(restoredChapter, data.lineIndex);

        if (!string.IsNullOrEmpty(data.backgroundId))
            SetBackgroundInstant(data.backgroundId);

        if (data.choiceState != null)
            RestoreChoiceState(data.choiceState);
    }


    private ChoiceSaveState CaptureChoiceState()
    {
        if (!choicesContainer.activeSelf)
            return null;

        return new ChoiceSaveState
        {
            lineIndex = currentLineIndex,
            selectedIndex = selectedChoiceIndex
        };
    }

    private void RestoreChoiceState(ChoiceSaveState state)
    {
        if (runtimeLines == null || state.lineIndex < 0 || state.lineIndex >= runtimeLines.Length)
            return;

        DialogueLine line = runtimeLines[state.lineIndex];

        ShowChoices(line.choices);

        if (state.selectedIndex < 0)
            return;

        for (int i = 0; i < currentChoiceButtons.Count; i++)
        {
            var btn = currentChoiceButtons[i].GetComponent<Button>();
            btn.interactable = false;

            if (i == state.selectedIndex)
            {
                // визуально выделить выбранный
                var img = currentChoiceButtons[i].GetComponent<Image>();
                img.color = Color.white;
            }
        }
    }






    public void SetBackgroundInstant(string backgroundId)
    {
        if (string.IsNullOrEmpty(backgroundId))
            return;

        Sprite bg = FindBackgroundById(backgroundId);
        if (bg == null)
        {
            Debug.LogWarning($"Background not found: {backgroundId}");
            return;
        }

        backgroundImage.sprite = bg;
        currentBackgroundId = backgroundId;
    }

    private Sprite FindBackgroundById(string id)
    {
        foreach (var chapter in chapters)
        {
            foreach (var line in chapter.lines)
            {
                if (line.backgroundId == id && line.backgroundSprite != null)
                    return line.backgroundSprite;
            }
        }
        return null;
    }

    public void OnHideHoverEnter()
    {
        hideButtonIcon.DOKill();
        hideButtonIcon.DOColor(hideHoverColor, 0.2f);

        tooltipTween?.Kill();

        hideTooltipGroup.gameObject.SetActive(true);
        hideTooltipGroup.alpha = 0f;

        tooltipTween = DOTween.Sequence()
            .AppendInterval(tooltipDelay)
            .Append(hideTooltipGroup.DOFade(1f, tooltipFadeDuration));
    }

    public void OnHideHoverExit()
    {
        hideButtonIcon.DOKill();
        hideButtonIcon.DOColor(hideBaseColor, 0.2f);

        tooltipTween?.Kill();

        hideTooltipGroup.DOKill();
        hideTooltipGroup.DOFade(0f, 0.1f)
            .OnComplete(() =>
            {
                hideTooltipGroup.gameObject.SetActive(false);
            });
    }
}
