using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;

public class ChapterSwipeDOTween : MonoBehaviour, IDragHandler, IEndDragHandler
{
    public RectTransform[] chapterWindows; // Окна глав
    public float swipeDuration = 0.3f; // Длительность свайпа
    public float leftBoundary = -500f; // Левая граница
    public float rightBoundary = 500f; // Правая граница

    private Vector2 dragStartPosition;
    private int currentChapterIndex = 0;

    public void OnDrag(PointerEventData eventData)
    {
        // Отслеживаем начальное движение (если нужно)
        dragStartPosition = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Определяем направление свайпа
        float dragDelta = eventData.position.x - dragStartPosition.x;

        if (Mathf.Abs(dragDelta) > 100f) // Порог движения для свайпа
        {
            if (dragDelta > 0 && currentChapterIndex > 0)
            {
                // Свайп влево
                currentChapterIndex--;
            }
            else if (dragDelta < 0 && currentChapterIndex < chapterWindows.Length - 1)
            {
                // Свайп вправо
                currentChapterIndex++;
            }
        }

        // Анимация движения через DOTween
        foreach (var chapter in chapterWindows)
        {
            chapter.DOLocalMoveX(-currentChapterIndex * chapter.rect.width, swipeDuration).SetEase(Ease.OutCubic);
        }
    }
}