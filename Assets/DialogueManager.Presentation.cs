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
                Debug.Log("CALLBACK");
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
        if (line.collectibleItem != null)
        {
            CollectibleManager.Instance.RegisterCollectible(line.collectibleItem);
        }

        if (line.showCollectibleView &&
            line.collectibleItem != null)
        {
            ShowCollectibleView(line.collectibleItem);
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

    private void RegisterCollectible(
    CollectibleItemBase collectible)
    {
        if (collectible == null)
            return;

        if (CollectibleManager.Instance == null)
        {
            Debug.LogWarning(
                "CollectibleManager.Instance не найден.",
                this
            );

            return;
        }

        CollectibleManager.Instance.RegisterCollectible(
            collectible
        );
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
        IsDialogueTransitioning = true;

        dialogueTween = DOTween.Sequence()
    .Append(dialogueGroup.DOFade(0f, 0.2f))
    .Join(dialogueGroup.transform.DOScale(0.98f, 0.2f))
    .OnStart(() =>
    {
        Debug.Log("START");
    })
    .OnKill(() =>
    {
        Debug.Log("KILLED");
    })
    .OnComplete(() =>
    {
        Debug.Log("COMPLETE");
        IsDialogueTransitioning = false;
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



}
