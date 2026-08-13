using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CollectibleTooltip : MonoBehaviour
{
    [Header("Content")]
    [SerializeField] private Image collectibleImage;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    [Header("Show Hint")]
    [SerializeField] private GameObject showHint;

    public void Show(Sprite image, string title, string description)
    {
        if (collectibleImage != null)
        {
            collectibleImage.sprite = image;
            collectibleImage.enabled = image != null;
            collectibleImage.preserveAspect = true;
        }

        if (titleText != null)
            titleText.text = title;

        if (descriptionText != null)
            descriptionText.text = description;

        if (showHint != null)
            showHint.SetActive(true);

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        if (showHint != null)
            showHint.SetActive(false);

        gameObject.SetActive(false);
    }

    public void ShowHint()
    {
        if (showHint != null)
            showHint.SetActive(true);
    }

    public void HideHint()
    {
        if (showHint != null)
            showHint.SetActive(false);
    }
}