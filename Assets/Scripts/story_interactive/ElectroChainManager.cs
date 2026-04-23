using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class ElectroChainManager : MonoBehaviour
{
    public ChainSlot[] chainSlots; // Все 4 слота цепочки
    public ToolType[] correctSequence; // Правильный порядок инструментов
    public ToolIcon[] startingTools;

    public int attemptsLeft = 3;
    public TextMeshProUGUI attemptsText;
    public CanvasGroup miniGameGroup;
    public Button exitButton; // Кнопка "Уйти"
    public GameObject likeIcon; // Иконка лайка
    public float autoCloseDelay = 5f;

    public GameObject electroChainUIPanel;
    public GameObject instructionsPanel;
    public GameObject gamePanel;
    public GameObject dialoguePanel;
    public GameObject gameOverPanel;

    [SerializeField] private Image backgroundFadeImage;
    [SerializeField] private float fadeDuration = 1.5f;
    [SerializeField] private bool clickToContinueAfterFade = true;
    [SerializeField] private float autoContinueDelay = 1f;
    public bool waitingForClick = false;

    public int triggerLineAfterWin = 0;

    public static ElectroChainManager Instance { get; private set; }

    void Awake()
    {
        Debug.Log("Awake: ElectroChainManager загружен");
        Instance = this;
        if (electroChainUIPanel != null)
            electroChainUIPanel.SetActive(false);
        else
            Debug.LogWarning("electroChainUIPanel не назначен в инспекторе!");
    }

    void Start()
    {
        foreach (var slot in chainSlots)
            slot.chainManager = this;

        foreach (var tool in startingTools)
        {
            if (!SubstationRoomManager.Instance.toolsWereCollected)
            {
                tool.gameObject.SetActive(false);
            }
        }
        exitButton.gameObject.SetActive(false);
    }

    public void ShowUI()
    {
        Debug.Log(">>>>> ShowUI ВЫЗВАН <<<<<");
        ResetFadeOverlay();

        dialoguePanel.SetActive(false);
        electroChainUIPanel.SetActive(true);
        gamePanel.SetActive(false);
        gameOverPanel.SetActive(false);

        if (!SubstationRoomManager.Instance.toolsWereCollected)
        {
            foreach (var tool in startingTools)
            {
                tool.gameObject.SetActive(false);
                tool.iconImage.enabled = false;
            }
        }
        else
        {
            foreach (var tool in startingTools)
            {
                tool.gameObject.SetActive(true);
                tool.iconImage.enabled = true;
            }
        }

        if (!instructionShown)
        {
            instructionsPanel.SetActive(true);
            StartCoroutine(InstructionSequence());
            instructionShown = true;
        }
        else
        {
            instructionsPanel.SetActive(false);
            gamePanel.SetActive(true);
        }
    }

    [SerializeField] private TMP_Text instructionCountdownText;
    [SerializeField] private float waitBeforeCountdown = 2f;
    [SerializeField] private float instructionCountdownDuration = 15f;

    private bool instructionShown = false;

    private IEnumerator InstructionSequence()
    {
        yield return new WaitForSeconds(waitBeforeCountdown);

        instructionCountdownText.gameObject.SetActive(true);

        float timeLeft = instructionCountdownDuration;
        while (timeLeft > 0)
        {
            instructionCountdownText.text = "" + Mathf.CeilToInt(timeLeft);
            yield return new WaitForSeconds(1f);
            timeLeft -= 1f;
        }

        instructionCountdownText.gameObject.SetActive(false);
        instructionsPanel.SetActive(false);
        StartMinigame();
    }

    public void StartMinigame()
    {
        ResetFadeOverlay();
        instructionsPanel.SetActive(false);
        gamePanel.SetActive(true);
        attemptsLeft = 3;
        UpdateAttemptsUI();
    }

    public void UpdateAttemptsUI()
    {
        if (LanguageManager.CurrentLanguage == Language.English && attemptsText != null)
            attemptsText.text = "Attempts: " + attemptsLeft.ToString();
        if (LanguageManager.CurrentLanguage == Language.Russian && attemptsText != null)
            attemptsText.text = "Попытки: " + attemptsLeft.ToString();
    }

    public void CheckChainComplete()
    {
        Debug.Log("CheckChainComplete вызван");
        foreach (var slot in chainSlots)
        {
            Debug.Log($"Проверка слота: {slot.name}, инструмент: {slot.currentTool}");
            if (slot.currentTool == ToolType.None)
            {
                Debug.Log("Цепочка не полная: найден пустой слот");
                return;
            }
        }

        bool correct = true;
        for (int i = 0; i < chainSlots.Length; i++)
        {
            Debug.Log($"Проверка слота {i}: текущий инструмент = {chainSlots[i].currentTool}, ожидаемый = {correctSequence[i]}");
            if (chainSlots[i].currentTool != correctSequence[i])
            {
                correct = false;
                break;
            }
        }

        if (correct)
        {
            Debug.Log("Цепочка правильная");
            OnChainSuccess();
        }
        else
        {
            Debug.Log("Цепочка неправильная");
            OnChainFail();
        }
    }

    void OnChainSuccess()
    {
        Debug.Log("Цепочка правильная!");
        exitButton.gameObject.SetActive(true);
        StartCoroutine(ShowLikeAndFadeUI());
    }

    void OnChainFail()
    {
        Debug.Log("Цепочка неправильная. Попробуйте снова.");

        attemptsLeft--;
        UpdateAttemptsUI();

        foreach (var slot in chainSlots)
        {
            if (slot.toolInSlot != null)
            {
                ToolIcon icon = slot.toolInSlot;
                slot.toolInSlot = null;
                slot.currentTool = ToolType.None;
                icon.ReturnToOriginalSlot();
            }
        }

        foreach (var slot in chainSlots)
        {
            slot.ClearTool();
        }

        if (attemptsLeft <= 0)
        {
            Debug.Log("Игрок проиграл. Все попытки использованы.");
            StartCoroutine(HandlePlayerDeath());
        }
    }

    IEnumerator HandlePlayerDeath()
    {
        ResetFadeOverlay();
        yield return StartCoroutine(FadeToBlack());
        gameOverPanel.SetActive(true);
        yield return StartCoroutine(FadeFromBlack());
        DialogueManager.Instance.TriggerGameOver();
    }

    private void ResetFadeOverlay()
    {
        if (backgroundFadeImage == null)
            return;

        backgroundFadeImage.gameObject.SetActive(true);
        Color color = backgroundFadeImage.color;
        color.a = 0f;
        backgroundFadeImage.color = color;
        backgroundFadeImage.raycastTarget = false;
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
        backgroundFadeImage.gameObject.SetActive(false);
    }

    private IEnumerator ShowLikeAndFadeUI()
    {
        likeIcon.SetActive(true);
        var likeCanvas = likeIcon.GetComponent<CanvasGroup>() ?? likeIcon.AddComponent<CanvasGroup>();
        likeCanvas.alpha = 0;

        float elapsed = 0f;
        float startAlpha = miniGameGroup.alpha;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;

            likeCanvas.alpha = Mathf.Lerp(0f, 1f, t);
            miniGameGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);

            yield return null;
        }

        likeCanvas.alpha = 1f;
        miniGameGroup.alpha = 0f;
        miniGameGroup.interactable = false;
        miniGameGroup.blocksRaycasts = false;

        exitButton.interactable = true;
    }

    public void ResetMinigameState()
    {
        Debug.Log("Сброс мини-игры");
        ResetFadeOverlay();

        attemptsLeft = 3;
        UpdateAttemptsUI();

        foreach (var slot in chainSlots)
        {
            if (slot.toolInSlot != null)
            {
                ToolIcon icon = slot.toolInSlot;
                slot.toolInSlot = null;
                slot.currentTool = ToolType.None;
                icon.ReturnToOriginalSlot();
            }
        }

        foreach (var slot in chainSlots)
            slot.ClearTool();

        gameOverPanel.SetActive(false);
        gamePanel.SetActive(true);
    }

    public void ClosePanelAndContinue()
    {
        gameObject.SetActive(false);

        if (LanguageManager.CurrentLanguage == Language.Russian)
            DialogueManager.Instance.JumpToLine(triggerLineAfterWin - 1);
        else
            DialogueManager.Instance.JumpToLine(302 - 1);
    }

    public void OnExitButtonClicked()
    {
        ClosePanelAndContinue();
    }
}
