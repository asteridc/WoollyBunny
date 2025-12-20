using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Button))]
public class PauseButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI References")]
    public TextMeshProUGUI buttonText;
    public Image buttonBackground;

    [Header("Hover Settings")]
    public Color normalTextColor = Color.white;
    public Color hoverTextColor = Color.black;
    public Color normalBackgroundColor = new Color(1f, 1f, 1f, 0f);
    public Color hoverBackgroundColor = new Color(1f, 1f, 1f, 0.2f);

    private void Awake()
    {
        if (buttonText == null)
            buttonText = GetComponentInChildren<TextMeshProUGUI>();

        if (buttonBackground == null)
            buttonBackground = GetComponent<Image>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (buttonText != null)
            buttonText.color = hoverTextColor;

        if (buttonBackground != null)
            buttonBackground.color = hoverBackgroundColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ResetVisual();
    }

    private void OnDisable()
    {
        ResetVisual();
    }

    private void ResetVisual()
    {
        if (buttonText != null)
            buttonText.color = normalTextColor;

        if (buttonBackground != null)
            buttonBackground.color = normalBackgroundColor;
    }
}
