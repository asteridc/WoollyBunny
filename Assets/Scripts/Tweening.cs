using UnityEngine;

public class SmoothChapterScroller : MonoBehaviour
{
    public RectTransform content;  // Контейнер с главами (например, объект с Horizontal Layout Group)
    public float snapSpeed = 10f;  // Скорость плавного перемещения
    public float threshold = 0.2f; // Порог для переключения на следующую/предыдущую главу
    public float sensitivity = 1f; // Чувствительность скроллинга

    private Vector2 targetPosition; // Позиция, к которой мы будем перемещаться
    private int currentIndex = 1;   // Текущая глава
    private bool isDragging = false;

    void Start()
    {
        // Установить начальную позицию
        targetPosition = content.anchoredPosition;
    }

    void Update()
    {
        // Обновление позиции контента (плавное движение)
        if (!isDragging)
        {
            content.anchoredPosition = Vector2.Lerp(content.anchoredPosition, targetPosition, Time.deltaTime * snapSpeed);
        }
    }

    public void OnBeginDrag()
    {
        isDragging = true;
    }

    public void OnEndDrag()
    {
        isDragging = false;

        // Определить ближайшую главу
        float nearestPosition = float.MaxValue;

        for (int i = 0; i < content.childCount; i++)
        {
            // Рассчитываем расстояние до каждого элемента
            float distance = Mathf.Abs(content.GetChild(i).localPosition.x - content.localPosition.x);

            if (distance < nearestPosition)
            {
                nearestPosition = distance;
                currentIndex = i;
            }
        }

        // Рассчитываем новую целевую позицию
        targetPosition = new Vector2(-content.GetChild(currentIndex).localPosition.x, content.anchoredPosition.y);
    }

    public void SnapToIndex(int index)
    {
        // Програмно переключаемся на заданную главу
        if (index >= 0 && index < content.childCount)
        {
            currentIndex = index;
            targetPosition = new Vector2(-content.GetChild(currentIndex).localPosition.x, content.anchoredPosition.y);
        }
    }
}