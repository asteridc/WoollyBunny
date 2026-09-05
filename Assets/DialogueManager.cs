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


public partial class DialogueManager : MonoBehaviour
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

    public (int bloodthirst, int nobility, int love) GetPathPoints()
    {
        return (bloodthirst, nobility, love);
    }

    public void SetPathPoints(int bloodthirst, int nobility, int love)
    {
        this.bloodthirst = bloodthirst;
        this.nobility = nobility;
        this.love = love;
    }

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

    private bool goingToNextChapter = false;

    public bool IsDialogueTransitioning { get; private set; }



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
        StartCoroutine(LoadChapterTransitionData());
    }

    private IEnumerator LoadChapterTransitionData()
    {
        while (SaveManager.Instance == null)
            yield return null;

        SaveManager.Instance.LoadChapterTransition(2);
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

}
