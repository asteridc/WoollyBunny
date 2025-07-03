using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    public GameObject[] sections;  // Массив с разделами (панелями)
    public Button[] buttons;       // Массив с кнопками

    private int currentSectionIndex = 1;  // Индекс активного раздела. Изначально выделена "Игра" (index = 1)

    void Start()
    {

        // Привязываем кнопки к их функциям
        for (int i = 0; i < buttons.Length; i++)
        {
            int index = i;  // Локальная переменная для замыкания
            buttons[i].onClick.AddListener(() => SwitchSection(index)); // Привязываем метод SwitchSection
        }
    }

    // Метод для переключения между разделами
    void SwitchSection(int index)
    {
        currentSectionIndex = index;  // Устанавливаем новый индекс активного раздела
        UpdateSections();  // Обновляем видимость разделов и обновление выделения кнопок
    }

    // Обновление видимости разделов и цвета кнопок
    void UpdateSections()
    {

        // Обновляем цвет кнопок
        for (int i = 0; i < buttons.Length; i++)
        {
            Image buttonImage = buttons[i].GetComponent<Image>();  // Получаем компонент Image на кнопке

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