using UnityEngine;

public class ShopUIManager : MonoBehaviour
{
    [SerializeField] private GameObject shopPanel;

    public void ToggleShop()
    {
        shopPanel.SetActive(!shopPanel.activeSelf);
    }
}