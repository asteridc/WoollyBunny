using System;
using UnityEngine;

[Serializable]
public enum ConditionalActionType { ShowLine, JumpToLine }

[Serializable]
public class DialogueCondition
{
    [Tooltip("ID условной строки (например: Chapter2_Line76). Именно эту ID я буду писать в AllConditions->triggerID")]
    public string triggerID;

    [Tooltip("Список названий/ID выборов (если пустой — любой выбор подходит)")]
    public string[] requiredChoices;

    [Tooltip("Список преобладающих черт (если пустой — любая черта подходит)")]
    public string[] requiredTraits;

    public ConditionalActionType actionType;

    [TextArea]
    [Tooltip("Текст, который покажем если actionType == ShowLine")]
    public string conditionalLineText;

    [Tooltip("Куда перейти, если для этого triggerID НИ ОДНО условие не выполнено (например 'Chapter2_Line77'). " +
             "Если пусто — будет продолжен обычный поток (т.е. придёт default-next).")]
    public string jumpToIfNotMatched;

    [Tooltip("Куда перейти после того, как условная строка показана (опционально). " +
             "Если пусто — используется стандартный следующий индекс/ID.")]
    public string continueAfterMatched;

    [Tooltip("Чем больше — тем выше приоритет этой записи при совпадении нескольких условий")]
    public int priority = 0;
}