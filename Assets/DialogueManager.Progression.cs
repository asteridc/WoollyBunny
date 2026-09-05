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
    [SerializeField] private GameObject[] characterObjects;

    private void ShowCollectibleView(CollectibleItemBase collectible)
    {
        if (collectible == null)
        {
            Debug.LogWarning(
                "ShowCollectibleView: collectible == null.",
                this
            );

            return;
        }

        if (collectiblePanel == null ||
            collectibleCanvasGroup == null)
        {
            Debug.LogWarning(
                "ShowCollectibleView: collectible UI не настроен.",
                this
            );

            return;
        }

        isCollectibleOpen = true;

        // 1. Скрываем диалог с анимацией.
        HideDialoguePanel(() =>
        {
            // 2. Подготавливаем панель.
            collectiblePanel.SetActive(true);

            collectibleCanvasGroup.DOKill();

            collectibleCanvasGroup.alpha = 0f;
            collectibleCanvasGroup.interactable = false;
            collectibleCanvasGroup.blocksRaycasts = false;

            // 3. Получаем данные непосредственно из ScriptableObject.
            collectibleTitleText.text = collectible.GetTitle();
            collectibleContentText.text = collectible.GetContent();
            collectibleIconImage.sprite = collectible.GetIcon();

            collectibleIconImage.enabled =
                collectible.GetIcon() != null;

            // 4. Показываем панель.
            DOTween.Sequence()
                .AppendInterval(0.25f)
                .Append(
                    collectibleCanvasGroup
                        .DOFade(1f, 0.25f)
                        .SetEase(Ease.OutQuad)
                )
                .SetUpdate(true)
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
        Debug.Log("START TRANSITION");
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
    public void OnContinueToNextChapterButton()
    {
        goingToNextChapter = true;
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
        Debug.Log("[DialogueManager] Переход в следующую главу.");

        if (currentChapter == null)
        {
            Debug.LogError("[DialogueManager] currentChapter == null!");
            yield break;
        }

        if (SaveManager.Instance == null)
        {
            Debug.LogError("[DialogueManager] SaveManager.Instance == null!");
            yield break;
        }

        // Сохраняем весь постоянный прогресс текущей главы.
        SaveManager.Instance.SaveChapterTransition(
            currentChapter.ChapterNumber
        );

        yield return new WaitForSeconds(0.25f);

        // Затемнение.
        if (blackOverlay != null)
        {
            yield return blackOverlay
                .DOFade(1f, 1.5f)
                .WaitForCompletion();
        }

        if (goingToNextChapter)
        {
            goingToNextChapter = false;

            SceneManager.LoadScene("Chapter_02");
        }
        else
        {
            SceneManager.LoadScene("woollybunny_PC");
        }
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
