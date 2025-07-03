using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MenuSwitcher : MonoBehaviour
{
    [Header("Panels")]
    public List<GameObject> menuPanels; // Список панелей

    [Header("Buttons")]
    public List<Button> menuButtons; // Список кнопок

    private int currentSectionIndex = 0; // Индекс выбранного раздела

    void Start()
    {
        // Показываем первый раздел и выделяем первую кнопку
        ShowPanel(0);
        UpdateSections(); // Обновляем цвета кнопок на старте
    }

    public void SwitchPanel(int panelIndex)
    {
        ShowPanel(panelIndex); // Показываем панель
        currentSectionIndex = panelIndex; // Обновляем индекс текущего раздела
        UpdateSections(); // Обновляем выделение кнопки
    }

    private void ShowPanel(int index)
    {
        // Скрыть все панели
        foreach (var panel in menuPanels)
        {
            panel.SetActive(false);
        }

        // Показать выбранную панель
        menuPanels[index].SetActive(true);
    }

    // Обновляем выделение кнопок и их цвет
    void UpdateSections()
    {
        // Обновляем цвет кнопок
        for (int i = 0; i < menuButtons.Count; i++)
        {
            Image buttonImage = menuButtons[i].GetComponent<Image>();  // Получаем компонент Image на кнопке

            if (i == currentSectionIndex)
            {
                // Выделяем кнопку (светлый цвет)
                buttonImage.color = new Color(0.8490566f, 0.7088822f, 0.7627954f, 0.3960784f); // Смена цвета при RGBA
            }
            else
            {
                // Возвращаем обычный цвет — например, белый
                buttonImage.color = new Color(0.6745098f, 0.1960784f, 0.2901961f); // Смена цвета при RGBA на нормальный цвет
            }
        }
    }
}