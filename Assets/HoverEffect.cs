using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class HoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Shadow shadowEffect;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (shadowEffect != null)
        {
            shadowEffect.enabled = true;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (shadowEffect != null)
        {
            shadowEffect.enabled = false;
        }
    }
}