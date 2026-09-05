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

}
