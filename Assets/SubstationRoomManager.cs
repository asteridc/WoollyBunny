using System.Collections.Generic;
using UnityEngine;

public class SubstationRoomManager : MonoBehaviour
{
    public static SubstationRoomManager Instance;
    public List<InteractionPoint> allPoints;

    public GameObject miniGamePanel;      // Панель мини-игры
    public GameObject dialoguePanel;      // Панель диалога (если нужна)

    public bool toolsWereCollected = false;


    private void Awake()
    {
        Instance = this;
    }

    public void StartInteractionPhase()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        ShowAvailablePoints();
    }

    // Показываем только лупы, которые не были использованы или являются повторяемыми
    public void ShowAvailablePoints()
    {
        foreach (var point in allPoints)
        {
            Debug.Log($"[{point.name}] wasUsed: {point.wasUsed}, isRepeatable: {point.isRepeatable}");

            if (!point.wasUsed || point.isRepeatable)
                point.Show();
            else
                point.Hide();
        }

        // Включаем объект-родитель луп, если он скрыт
        if (allPoints.Count > 0 && allPoints[0].transform.parent != null)
            allPoints[0].transform.parent.gameObject.SetActive(true);
    }

    public void HideAllPoints()
    {
        foreach (var point in allPoints)
        {
            point.Hide();
        }
    }

    /// <summary>
    /// Вызывается после завершения мини-диалога или действия лупы.
    /// </summary>
    public void OnPointDialogueFinished()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        // Тут можно вернуть управление игроку, если нужно
    }

    /// <summary>
    /// Выход из мини-игры — скрываем мини-игру, показываем диалог и оставшиеся лупы
    /// </summary>
    public void ExitMinigame()
    {
        if (miniGamePanel != null)
            miniGamePanel.SetActive(false);

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        ShowAvailablePoints();
    }
}
