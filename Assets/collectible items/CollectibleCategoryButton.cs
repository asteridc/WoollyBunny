using UnityEngine;
using UnityEngine.EventSystems;

public class CollectibleCategoryButton : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private CollectibleCategoriesController controller;
    [SerializeField] private int categoryIndex;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (controller == null)
        {
            Debug.LogWarning(
                "CollectibleCategoryButton: controller is not assigned.",
                this
            );

            return;
        }

        controller.SelectCategory(categoryIndex);
    }
}