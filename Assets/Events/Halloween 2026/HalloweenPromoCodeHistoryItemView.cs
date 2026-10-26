using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class HalloweenPromoCodeHistoryItemView : MonoBehaviour
{
    [SerializeField] private TMP_Text codeText;
    [SerializeField] private TMP_Text rewardText;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_Text expiryText;
    [SerializeField] private Button copyButton;

    private string codeToCopy;

    public void Bind(HalloweenPromoCodeRecord record)
    {
        if (record == null)
        {
            gameObject.SetActive(false);
            return;
        }

        bool english =
            LanguageManager.CurrentLanguage == Language.English;

        codeToCopy = record.code ?? string.Empty;

        if (codeText != null)
            codeText.text = codeToCopy;

        if (rewardText != null)
        {
            InventoryEntryDefinition reward =
                ResourceInventoryManager.Instance != null
                    ? ResourceInventoryManager.Instance.GetDefinition(
                        record.rewardInventoryId)
                    : null;

            if (reward != null)
            {
                string rewardName = english
                    ? reward.englishName
                    : reward.russianName;

                string color =
                    HalloweenRarityColorUtility.ToHex(reward.rarity);

                rewardText.text =
                    $"<color=#{color}>{rewardName}</color>";
            }
            else
            {
                rewardText.text = record.rewardInventoryId;
            }
        }

        bool expired =
            DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            >= record.expiresAtUnixSeconds;

        string status;

        if (record.isRedeemed)
            status = english ? "Redeemed" : "Использован";
        else if (expired)
            status = english ? "Expired" : "Истёк";
        else
            status = english
                ? "Not redeemed"
                : "Не использован";

        if (statusText != null)
            statusText.text = status;

        if (expiryText != null)
        {
            DateTimeOffset expiration =
                DateTimeOffset
                    .FromUnixTimeSeconds(record.expiresAtUnixSeconds)
                    .ToLocalTime();

            expiryText.text = english
                ? $"Expires: {expiration:yyyy-MM-dd HH:mm}"
                : $"Действует до: {expiration:dd.MM.yyyy HH:mm}";
        }

        if (copyButton != null)
        {
            copyButton.onClick.RemoveListener(CopyCode);
            copyButton.onClick.AddListener(CopyCode);

            copyButton.interactable =
                !string.IsNullOrEmpty(codeToCopy);
        }
    }

    private void OnDestroy()
    {
        if (copyButton != null)
            copyButton.onClick.RemoveListener(CopyCode);
    }

    private void CopyCode()
    {
        if (!string.IsNullOrEmpty(codeToCopy))
            GUIUtility.systemCopyBuffer = codeToCopy;
    }
}