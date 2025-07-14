using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class ElectroChainManager : MonoBehaviour
{
    public ChainSlot[] chainSlots; // Все 4 слота цепочки
    public ToolType[] correctSequence; // Правильный порядок инструментов

    public int attemptsLeft = 3;
    public TextMeshProUGUI attemptsText;
    public Button exitButton; // Кнопка "Уйти"
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

  
    public static ElectroChainManager Instance { get; private set; }

    void Awake()
    {
        Debug.Log("Awake: ElectroChainManager загружен");
        Instance = this;
        if (electroChainUIPanel != null)
            electroChainUIPanel.SetActive(false); // скрываем при старте
        else
            Debug.LogWarning("electroChainUIPanel не назначен в инспекторе!");
    }

    void Start()
    {
        foreach (var slot in chainSlots)
            slot.chainManager = this;
        exitButton.gameObject.SetActive(false);
    }

    public void ShowUI()
    {
        Debug.Log("Показ панели инструкций");

        // Скрываем всё остальное
        dialoguePanel.SetActive(false);
        electroChainUIPanel.SetActive(true);
        instructionsPanel.SetActive(true);
        gamePanel.SetActive(false);
        gameOverPanel.SetActive(false);

        StartCoroutine(InstructionSequence());
    }

    [SerializeField] private TMP_Text instructionCountdownText;
    [SerializeField] private float waitBeforeCountdown = 2f;
    [SerializeField] private float instructionCountdownDuration = 15f;

    private IEnumerator InstructionSequence()
    {
        // Ждём перед показом таймера
        yield return new WaitForSeconds(waitBeforeCountdown);

        // Показываем таймер
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
        StartMinigame(); // запуск самой мини-игры
    }

    public void StartMinigame()
    {
        instructionsPanel.SetActive(false);
        gamePanel.SetActive(true);
        attemptsLeft = 3;
        UpdateAttemptsUI();
    }

    void UpdateAttemptsUI()
    {
        if (attemptsText != null)
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
        // Или автоскрытие через задержку
        StartCoroutine(FinishMinigame());
    }

    void OnChainFail()
    {
        Debug.Log("Цепочка неправильная. Попробуйте снова.");

        // 1. Уменьшаем количество попыток
        attemptsLeft--;
        UpdateAttemptsUI(); // предполагается, что есть UI-текст для отображения попыток

        // 2. Возвращаем все инструменты на начальные позиции
        foreach (var slot in chainSlots)
        {
            if (slot.toolInSlot != null)
            {
                ToolIcon icon = slot.toolInSlot;

                // Удаляем ссылку из текущего слота
                slot.toolInSlot = null;

                // Обнуляем ссылку на текущий слот в инструменте
                icon.CurrentSlot = null;

                // Возврат на исходную ячейку
                if (icon.originalParent != null)
                {
                    icon.transform.SetParent(icon.originalParent);
                    if (icon.RectTransform != null)
                        icon.RectTransform.anchoredPosition = Vector2.zero;
                    else
                        icon.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

                    // Обновляем оригинальный слот (если есть)
                    if (icon.originalParentSlot != null)
                    {
                        icon.originalParentSlot.toolInSlot = icon;
                        icon.CurrentSlot = icon.originalParentSlot;
                    }
                }
                else
                {
                    Debug.LogWarning("originalParent у ToolIcon не задан!");
                }
                icon.iconImage.sprite = icon.iconSprite;
                icon.iconImage.enabled = true;
                Debug.Log($"Возвращен {icon.toolType} на {icon.originalParent.name}");
            }
        }

        // 3. Очистка всех целевых слотов цепи
        foreach (var slot in chainSlots)
        {
            slot.ClearTool(); // очищает .currentTool и .toolIconImage
        }

        // 4. Проверка на проигрыш
        if (attemptsLeft <= 0)
        {
            Debug.Log("Игрок проиграл. Все попытки использованы.");
            StartCoroutine(HandlePlayerDeath());
        }
    }

    IEnumerator HandlePlayerDeath()
    {
        yield return StartCoroutine(FadeFromBlack());
        gameOverPanel.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        yield return StartCoroutine(FadeToBlack());

        DialogueManager.Instance.TriggerGameOver();
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

    IEnumerator FinishMinigame()
    {
        yield return new WaitForSeconds(2f);
        gamePanel.SetActive(false);
        DialogueManager.Instance.ContinueDialogue(true);
        ClosePanelAndContinue();
    }

    public void ClosePanelAndContinue()
    {
        // Логика скрытия панели и продолжения сюжета
        gameObject.SetActive(false);
        // Вызов события или метода для продолжения сюжета
    }
}