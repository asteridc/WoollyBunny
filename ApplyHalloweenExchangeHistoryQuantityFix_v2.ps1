$ErrorActionPreference = 'Stop'

$root = (Get-Location).Path
$paths = @{
    Account = Join-Path $root 'Assets\AccountManager.cs'
    Manager = Join-Path $root 'Assets\Events\Halloween 2026\HalloweenExchangeManager.cs'
    UI = Join-Path $root 'Assets\Events\Halloween 2026\HalloweenExchangeUI.cs'
    HistoryView = Join-Path $root 'Assets\Events\Halloween 2026\HalloweenPromoCodeHistoryItemView.cs'
}

foreach ($path in $paths.Values) {
    if (-not (Test-Path -LiteralPath $path)) {
        throw "Не найден файл: $path`nЗапусти скрипт из корня проекта Unity."
    }
}

function Read-Source([string]$Path) {
    return [System.IO.File]::ReadAllText($Path).Replace("`r`n", "`n")
}

function Replace-RegexBlock([string]$Text, [string]$Pattern, [string]$Replacement, [string]$Label) {
    $regex = [System.Text.RegularExpressions.Regex]::new(
        $Pattern,
        [System.Text.RegularExpressions.RegexOptions]::Singleline)
    $match = $regex.Match($Text)

    if (-not $match.Success) {
        throw "Не найден участок '$Label'. Ничего не записано. Проверь актуальность локальных скриптов."
    }

    if ($regex.Matches($Text).Count -ne 1) {
        throw "Участок '$Label' найден несколько раз. Ничего не записано."
    }

    return $Text.Substring(0, $match.Index) + $Replacement +
        $Text.Substring($match.Index + $match.Length)
}

function Replace-Once([string]$Text, [string]$Old, [string]$New, [string]$Label) {
    $first = $Text.IndexOf($Old, [System.StringComparison]::Ordinal)
    if ($first -lt 0) { throw "Не найден участок '$Label'. Ничего не записано." }
    $second = $Text.IndexOf($Old, $first + $Old.Length, [System.StringComparison]::Ordinal)
    if ($second -ge 0) { throw "Участок '$Label' найден несколько раз. Ничего не записано." }
    return $Text.Substring(0, $first) + $New + $Text.Substring($first + $Old.Length)
}

$account = Read-Source $paths.Account
$manager = Read-Source $paths.Manager
$ui = Read-Source $paths.UI
$historyView = Read-Source $paths.HistoryView

# -----------------------------------------------------------------------------
# AccountManager: persist the actual reward amount and allow distinct offers
# to grant the same StackableResource.
# -----------------------------------------------------------------------------
if (-not $account.Contains('HALLOWEEN_HISTORY_QUANTITY_FIX_PURCHASE')) {
    $newPurchase = @'
    // HALLOWEEN_HISTORY_QUANTITY_FIX_PURCHASE
    public bool TryPurchaseHalloweenOffer(
        string currencyId,
        string offerId,
        string rewardInventoryId,
        int price,
        int capacityPercent,
        int rewardAmount,
        string code,
        long createdAtUnixSeconds,
        long expiresAtUnixSeconds,
        out string failureReason)
    {
        failureReason = string.Empty;

        string normalizedCurrencyId = NormalizeInventoryId(currencyId);
        string normalizedOfferId = NormalizeInventoryId(offerId);
        string normalizedRewardId = NormalizeInventoryId(rewardInventoryId);
        string normalizedCode = string.IsNullOrWhiteSpace(code)
            ? string.Empty
            : code.Trim().ToUpperInvariant();

        if (accountData == null || normalizedCurrencyId == null ||
            normalizedOfferId == null || normalizedRewardId == null ||
            price <= 0 || capacityPercent < 1 || capacityPercent > 100 ||
            rewardAmount < 1)
        {
            return FailHalloweenExchange("Некорректные параметры покупки.", out failureReason);
        }

        if (!HalloweenPromoCodeService.IsValidFormat(normalizedCode) ||
            HasHalloweenPromoCode(normalizedCode))
        {
            return FailHalloweenExchange(
                "Промокод имеет неверный формат или уже существует.", out failureReason);
        }

        // Only the same offer is a duplicate. Different offers may grant
        // the same StackableResource with different quantities.
        if (HasHalloweenPromoForOffer(normalizedOfferId))
        {
            return FailHalloweenExchange(
                "Это предложение уже было куплено.", out failureReason);
        }

        if (createdAtUnixSeconds <= 0 || expiresAtUnixSeconds <= createdAtUnixSeconds)
        {
            return FailHalloweenExchange(
                "Некорректный срок действия промокода.", out failureReason);
        }

        if (GetHalloweenBasketCapacityUsed() + capacityPercent > 100)
        {
            return FailHalloweenExchange("В корзине недостаточно места.", out failureReason);
        }

        if (GetInventoryAmount(normalizedCurrencyId) < price)
        {
            return FailHalloweenExchange("Недостаточно Конфетыкв.", out failureReason);
        }

        AccountInventoryAmount currencyEntry = GetOrCreateInventoryAmount(normalizedCurrencyId);
        int previousBalance = currencyEntry.amount;
        int previousCapacity = accountData.halloweenBasketCapacityUsed;

        HalloweenPromoCodeRecord record = new HalloweenPromoCodeRecord
        {
            code = normalizedCode,
            offerId = normalizedOfferId,
            rewardInventoryId = normalizedRewardId,
            rewardAmount = rewardAmount,
            pricePaid = price,
            basketCapacityPercent = capacityPercent,
            createdAtUnixSeconds = createdAtUnixSeconds,
            expiresAtUnixSeconds = expiresAtUnixSeconds,
            isRedeemed = false,
            redeemedAtUnixSeconds = 0
        };

        currencyEntry.amount = previousBalance - price;
        accountData.halloweenBasketCapacityUsed = previousCapacity + capacityPercent;
        accountData.halloweenPromoCodes.Add(record);

        try
        {
            SaveAccountData();
            return true;
        }
        catch (Exception exception)
        {
            currencyEntry.amount = previousBalance;
            accountData.halloweenBasketCapacityUsed = previousCapacity;
            accountData.halloweenPromoCodes.Remove(record);
            Debug.LogError(
                $"[AccountManager] Не удалось сохранить покупку Хеллоуинского обмена: {exception.Message}",
                this);
            failureReason = "Не удалось сохранить покупку. Конфеты и корзина не должны измениться.";
            return false;
        }
    }

'@
    $account = Replace-RegexBlock $account '(?s)    public bool TryPurchaseHalloweenOffer\(.*?(?=    public bool HasHalloweenPromoForOffer\()' $newPurchase 'AccountManager.TryPurchaseHalloweenOffer'
}

# AccountManager: redeem stackable quantities transactionally, while preserving the old UniqueItem overload.
if (-not $account.Contains('HALLOWEEN_HISTORY_QUANTITY_FIX_REDEEM')) {
    $newRedeem = @'
    // HALLOWEEN_HISTORY_QUANTITY_FIX_REDEEM
    // Compatibility overload for existing UniqueItem callers.
    public bool TryRedeemHalloweenPromoCode(
        string code,
        string expectedRewardInventoryId,
        long nowUnixSeconds,
        out string failureReason)
    {
        return TryRedeemHalloweenPromoCode(
            code,
            expectedRewardInventoryId,
            InventoryEntryType.UniqueItem,
            nowUnixSeconds,
            out failureReason);
    }

    public bool TryRedeemHalloweenPromoCode(
        string code,
        string expectedRewardInventoryId,
        InventoryEntryType rewardType,
        long nowUnixSeconds,
        out string failureReason)
    {
        failureReason = string.Empty;

        if (accountData == null || accountData.halloweenPromoCodes == null ||
            string.IsNullOrWhiteSpace(code) ||
            string.IsNullOrWhiteSpace(expectedRewardInventoryId))
        {
            return FailHalloweenExchange("Промокод или ID награды не заданы.", out failureReason);
        }

        string normalizedCode = code.Trim().ToUpperInvariant();
        string rewardId = NormalizeInventoryId(expectedRewardInventoryId);
        HalloweenPromoCodeRecord record = accountData.halloweenPromoCodes.Find(item =>
            item != null && string.Equals(item.code, normalizedCode, StringComparison.OrdinalIgnoreCase));

        if (record == null)
            return FailHalloweenExchange("Промокод не найден в текущем аккаунте.", out failureReason);
        if (record.isRedeemed)
            return FailHalloweenExchange("Этот промокод уже использован.", out failureReason);
        if (nowUnixSeconds >= record.expiresAtUnixSeconds)
            return FailHalloweenExchange("Срок действия промокода истёк.", out failureReason);
        if (!InventoryIdsEqual(record.rewardInventoryId, rewardId))
            return FailHalloweenExchange("ID награды не совпадает с данными промокода.", out failureReason);

        int quantity = rewardType == InventoryEntryType.UniqueItem
            ? 1
            : record.rewardAmount;

        AccountInventoryAmount stackableEntry = null;
        int previousStackableAmount = 0;

        if (rewardType == InventoryEntryType.UniqueItem)
        {
            if (HasUniqueInventoryItem(rewardId))
                return FailHalloweenExchange("Этот уникальный предмет уже есть в инвентаре.", out failureReason);
        }
        else if (rewardType == InventoryEntryType.StackableResource)
        {
            if (quantity < 1)
                return FailHalloweenExchange("В промокоде не указано корректное количество награды.", out failureReason);

            stackableEntry = GetOrCreateInventoryAmount(rewardId);
            previousStackableAmount = Mathf.Max(0, stackableEntry.amount);
            if (previousStackableAmount > int.MaxValue - quantity)
                return FailHalloweenExchange("Количество награды превышает допустимый предел.", out failureReason);
        }
        else
        {
            return FailHalloweenExchange("Этот тип награды нельзя выдать через промокод.", out failureReason);
        }

        bool previousRedeemed = record.isRedeemed;
        long previousRedeemedAt = record.redeemedAtUnixSeconds;
        record.isRedeemed = true;
        record.redeemedAtUnixSeconds = nowUnixSeconds;

        if (rewardType == InventoryEntryType.UniqueItem)
            accountData.ownedUniqueInventoryItems.Add(rewardId);
        else
            stackableEntry.amount = previousStackableAmount + quantity;

        try
        {
            SaveAccountData();
            return true;
        }
        catch (Exception exception)
        {
            record.isRedeemed = previousRedeemed;
            record.redeemedAtUnixSeconds = previousRedeemedAt;

            if (rewardType == InventoryEntryType.UniqueItem)
                RemoveUniqueInventoryItem(rewardId);
            else if (stackableEntry != null)
                stackableEntry.amount = previousStackableAmount;

            Debug.LogError(
                $"[AccountManager] Не удалось сохранить погашение промокода: {exception.Message}", this);
            failureReason = "Не удалось сохранить награду. Попробуй ещё раз.";
            return false;
        }
    }

'@
    $account = Replace-RegexBlock $account '(?s)    public bool TryRedeemHalloweenPromoCode\(.*?(?=    /// <summary>\s*/// Test-only reset for the Halloween exchange pool|    public bool ResetHalloweenExchangeState\(|    private static bool FailHalloweenExchange\()' $newRedeem 'AccountManager.TryRedeemHalloweenPromoCode'
}

# -----------------------------------------------------------------------------
# HalloweenExchangeManager: pass quantity into persistence, redeem both types.
# -----------------------------------------------------------------------------
if (-not $manager.Contains('HALLOWEEN_HISTORY_QUANTITY_FIX_PURCHASE')) {
    $newManagerPurchase = @'
    // HALLOWEEN_HISTORY_QUANTITY_FIX_PURCHASE
    /// <summary>
    /// Покупка создаёт промокод и фиксирует стоимость, вместимость и количество награды.
    /// Награда выдаётся только после успешного погашения промокода.
    /// </summary>
    public bool TryPurchaseOffer(
        HalloweenExchangeOffer offer,
        out HalloweenPromoCodeRecord createdRecord,
        out string failureReason)
    {
        createdRecord = null;
        if (!CanPurchase(offer, out failureReason))
            return false;

        AccountManager account = AccountManager.Instance;
        ResourceInventoryManager inventory = ResourceInventoryManager.Instance;
        InventoryEntryDefinition reward = inventory != null
            ? inventory.GetDefinition(offer.RewardInventoryId)
            : null;

        if (reward == null)
            return Fail(Localized(
                "Награда отсутствует в каталоге инвентаря.",
                "The reward is missing from the inventory catalog."), out failureReason);

        int rewardAmount = reward.type == InventoryEntryType.UniqueItem
            ? 1
            : offer.RewardAmount;

        if (rewardAmount < 1)
            return Fail(Localized(
                "Количество награды должно быть не меньше 1.",
                "Reward amount must be at least 1."), out failureReason);

        if (!HalloweenPromoCodeService.TryGenerateUniqueCode(account, out string code))
            return Fail(Localized(
                "Не удалось создать уникальный промокод. Попробуй ещё раз.",
                "Could not generate a unique promo code. Please try again."), out failureReason);

        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if (!HalloweenPromoCodeService.TryCalculateExpiry(now, PromoCodeLifetimeDays, out long expiresAt))
            return Fail(Localized(
                "Не удалось рассчитать срок действия промокода.",
                "Could not calculate the promo-code expiry."), out failureReason);

        bool purchased = account.TryPurchaseHalloweenOffer(
            currencyId,
            offer.OfferId,
            offer.RewardInventoryId,
            offer.CandyPrice,
            offer.BasketCapacityPercent,
            rewardAmount,
            code,
            now,
            expiresAt,
            out failureReason);

        if (!purchased)
            return false;

        inventory.NotifyAccountAmountChanged(currencyId);

        createdRecord = new HalloweenPromoCodeRecord
        {
            code = code,
            offerId = offer.OfferId,
            rewardInventoryId = offer.RewardInventoryId,
            rewardAmount = rewardAmount,
            pricePaid = offer.CandyPrice,
            basketCapacityPercent = offer.BasketCapacityPercent,
            createdAtUnixSeconds = now,
            expiresAtUnixSeconds = expiresAt,
            isRedeemed = false,
            redeemedAtUnixSeconds = 0
        };

        ExchangeChanged?.Invoke();
        return true;
    }

'@
    $manager = Replace-RegexBlock $manager '(?s)    public bool TryPurchaseOffer\(.*?(?=    /// <summary>\s*/// Погашение проверяется)' $newManagerPurchase 'HalloweenExchangeManager.TryPurchaseOffer'
}

if (-not $manager.Contains('HALLOWEEN_HISTORY_QUANTITY_FIX_REDEEM')) {
    $newManagerRedeem = @'
    // HALLOWEEN_HISTORY_QUANTITY_FIX_REDEEM
    /// <summary>Погашает промокод и выдаёт сохранённое в нём количество награды.</summary>
    public bool TryRedeemCode(
        string rawCode,
        out InventoryEntryDefinition reward,
        out string failureReason)
    {
        return TryRedeemCode(rawCode, out reward, out _, out failureReason);
    }

    public bool TryRedeemCode(
        string rawCode,
        out InventoryEntryDefinition reward,
        out int rewardAmount,
        out string failureReason)
    {
        reward = null;
        rewardAmount = 0;
        failureReason = string.Empty;

        string code = HalloweenPromoCodeService.Normalize(rawCode);
        if (!HalloweenPromoCodeService.IsValidFormat(code))
            return Fail(Localized(
                "Промокод должен состоять из 8 символов: A–Z и 0–9.",
                "The promo code must contain 8 characters: A–Z and 0–9."), out failureReason);

        AccountManager account = AccountManager.Instance;
        ResourceInventoryManager inventory = ResourceInventoryManager.Instance;
        if (account == null || inventory == null)
            return Fail(Localized(
                "Системы аккаунта или инвентаря ещё не готовы.",
                "Account or inventory systems are not ready yet."), out failureReason);

        HalloweenPromoCodeRecord record = account.FindHalloweenPromoCode(code);
        if (record == null)
            return Fail(Localized(
                "Такой промокод не найден в этом сохранении аккаунта.",
                "This promo code was not found in this account save."), out failureReason);

        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if (!HalloweenPromoCodeService.ValidateForRedemption(record, now, out failureReason))
            return false;

        reward = inventory.GetDefinition(record.rewardInventoryId);
        if (reward == null ||
            (reward.type != InventoryEntryType.UniqueItem &&
             reward.type != InventoryEntryType.StackableResource))
        {
            reward = null;
            return Fail(Localized(
                "Награда не найдена или имеет неподдерживаемый тип.",
                "The reward is missing or has an unsupported type."), out failureReason);
        }

        if (inventory.IsExpired(reward.id))
        {
            reward = null;
            return Fail(Localized(
                "Срок доступности этой награды закончился.",
                "This reward has expired."), out failureReason);
        }

        if (reward.type == InventoryEntryType.UniqueItem)
        {
            if (account.HasUniqueInventoryItem(reward.id))
            {
                reward = null;
                return Fail(Localized(
                    "Этот уникальный предмет уже есть в инвентаре.",
                    "You already own this unique item."), out failureReason);
            }
            rewardAmount = 1;
        }
        else
        {
            rewardAmount = record.rewardAmount;
            if (rewardAmount < 1)
            {
                reward = null;
                return Fail(Localized(
                    "В промокоде не указано корректное количество награды.",
                    "The promo code does not contain a valid reward quantity."), out failureReason);
            }
        }

        if (!account.TryRedeemHalloweenPromoCode(
                code, reward.id, reward.type, now, out failureReason))
        {
            reward = null;
            rewardAmount = 0;
            return false;
        }

        if (reward.type == InventoryEntryType.UniqueItem)
            inventory.RefreshUniqueItemViewAfterExternalGrant(reward.id);
        else
            inventory.NotifyAccountAmountChanged(reward.id);

        ExchangeChanged?.Invoke();
        return true;
    }

'@
    $manager = Replace-RegexBlock $manager '(?s)    public bool TryRedeemCode\(.*?(?=    /// <summary>Добавляет указанное количество обычного ресурса)' $newManagerRedeem 'HalloweenExchangeManager.TryRedeemCode'
}

# -----------------------------------------------------------------------------
# History UI: keep rows and reuse them; make Content tall enough for all rows.
# -----------------------------------------------------------------------------
if (-not $ui.Contains('historyItemVerticalSpacing')) {
    $ui = Replace-RegexBlock $ui '    \[SerializeField\] private HalloweenPromoCodeHistoryItemView historyItemPrefab;' "    [SerializeField] private HalloweenPromoCodeHistoryItemView historyItemPrefab;`n    [SerializeField, Min(1f)] private float historyItemVerticalSpacing = 42f;" 'history row spacing field'
}

if (-not $ui.Contains('historyItemViews')) {
    $ui = Replace-RegexBlock $ui '    private readonly List<HalloweenExchangeOfferView> offerViews =\s*new List<HalloweenExchangeOfferView>\(\);' "    private readonly List<HalloweenExchangeOfferView> offerViews =`n        new List<HalloweenExchangeOfferView>();`n`n    private readonly List<HalloweenPromoCodeHistoryItemView> historyItemViews =`n        new List<HalloweenPromoCodeHistoryItemView>();" 'history row cache field'
}

if (-not $ui.Contains('HALLOWEEN_HISTORY_QUANTITY_FIX')) {
    $newRefreshAll = @'
    public void RefreshAll()
    {
        RefreshBalanceAndBasket();
        RefreshOfferList();
        RefreshHistoryIfOpen();
    }
'@
    $ui = Replace-RegexBlock $ui '(?s)    public void RefreshAll\(\)\s*\{.*?\n    \}' $newRefreshAll 'HalloweenExchangeUI.RefreshAll'

    $newOpenHistory = @'
    public void OpenHistory()
    {
        ShowPanel(historyGroup, historyFrame, closeHistoryButton);
        RefreshHistoryItems();
    }
'@
    $ui = Replace-RegexBlock $ui '(?s)    public void OpenHistory\(\)\s*\{.*?\n    \}' $newOpenHistory 'HalloweenExchangeUI.OpenHistory'

    $newRefreshHistory = @'
    // HALLOWEEN_HISTORY_QUANTITY_FIX
    private void RefreshHistoryItems()
    {
        if (exchangeManager == null || historyContent == null || historyItemPrefab == null)
            return;

        historyContent.gameObject.SetActive(true);

        // Adopt existing rows once, then reuse them instead of Destroy/Instantiate
        // on every refresh. This prevents delayed Destroy calls from interfering
        // with subsequent refreshes while the history panel is open.
        if (historyItemViews.Count == 0)
        {
            historyItemViews.AddRange(
                historyContent.GetComponentsInChildren<HalloweenPromoCodeHistoryItemView>(true));
        }

        List<HalloweenPromoCodeRecord> history = exchangeManager.GetPromoHistory();
        Debug.Log($"[HalloweenExchangeUI] Записей промокодов в сохранении: {history.Count}", this);

        if (historyEmptyText != null)
        {
            bool hasHistory = history.Count > 0;
            historyEmptyText.gameObject.SetActive(!hasHistory);
            if (!hasHistory)
            {
                historyEmptyText.text = LanguageManager.CurrentLanguage == Language.English
                    ? "No promo codes yet"
                    : "Промокодов пока нет";
            }
        }

        RectTransform contentRect = historyContent as RectTransform;
        RectTransform prefabRect = historyItemPrefab.GetComponent<RectTransform>();
        Vector2 prefabPosition = prefabRect != null ? prefabRect.anchoredPosition : Vector2.zero;
        float rowHeight = Mathf.Max(1f, historyItemVerticalSpacing);

        VerticalLayoutGroup verticalLayout = historyContent.GetComponent<VerticalLayoutGroup>();
        LayoutGroup anyLayout = historyContent.GetComponent<LayoutGroup>();

        if (verticalLayout != null)
        {
            verticalLayout.enabled = true;
            verticalLayout.childControlHeight = true;
            verticalLayout.childForceExpandHeight = false;
            verticalLayout.spacing = 0f;
        }
        else if (anyLayout != null)
        {
            // Horizontal/Grid layouts would override our vertical positions.
            anyLayout.enabled = false;
        }

        for (int i = 0; i < history.Count; i++)
        {
            HalloweenPromoCodeHistoryItemView item;
            if (i < historyItemViews.Count)
            {
                item = historyItemViews[i];
                if (item == null)
                {
                    item = Instantiate(historyItemPrefab, historyContent, false);
                    historyItemViews[i] = item;
                }
            }
            else
            {
                item = Instantiate(historyItemPrefab, historyContent, false);
                historyItemViews.Add(item);
            }

            item.gameObject.SetActive(true);
            item.transform.SetAsLastSibling();
            item.Bind(history[i]);

            RectTransform itemRect = item.GetComponent<RectTransform>();
            if (itemRect == null)
                continue;

            if (verticalLayout != null)
            {
                LayoutElement layoutElement = item.GetComponent<LayoutElement>();
                if (layoutElement == null)
                    layoutElement = item.gameObject.AddComponent<LayoutElement>();

                layoutElement.minHeight = rowHeight;
                layoutElement.preferredHeight = rowHeight;
                layoutElement.flexibleHeight = 0f;
            }
            else
            {
                itemRect.anchorMin = new Vector2(itemRect.anchorMin.x, 1f);
                itemRect.anchorMax = new Vector2(itemRect.anchorMax.x, 1f);
                itemRect.pivot = new Vector2(itemRect.pivot.x, 1f);
                itemRect.anchoredPosition = new Vector2(
                    prefabPosition.x,
                    prefabPosition.y - i * rowHeight);
            }
        }

        // Hide cached rows that are not needed now, but keep them for the next refresh.
        for (int i = history.Count; i < historyItemViews.Count; i++)
        {
            if (historyItemViews[i] != null)
                historyItemViews[i].gameObject.SetActive(false);
        }

        if (contentRect != null)
        {
            if (verticalLayout != null)
            {
                int count = history.Count;
                float requiredHeight = verticalLayout.padding.top + verticalLayout.padding.bottom +
                    count * rowHeight + Mathf.Max(0, count - 1) * verticalLayout.spacing;

                contentRect.SetSizeWithCurrentAnchors(
                    RectTransform.Axis.Vertical,
                    Mathf.Max(rowHeight, requiredHeight));
                LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
            }
            else
            {
                float requiredHeight = Mathf.Max(
                    rowHeight,
                    Mathf.Max(0f, -prefabPosition.y) + history.Count * rowHeight + 8f);
                contentRect.SetSizeWithCurrentAnchors(
                    RectTransform.Axis.Vertical,
                    requiredHeight);
            }
        }

        Canvas.ForceUpdateCanvases();
        if (contentRect != null && verticalLayout != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
    }
'@
    $ui = Replace-RegexBlock $ui '(?s)    private void RefreshHistoryItems\(\)\s*\{.*?(?=\n    private void ShowPanel\()' $newRefreshHistory 'HalloweenExchangeUI.RefreshHistoryItems'
}

# Show the exact quantity in the redemption feedback as well.
if (-not $ui.Contains('HALLOWEEN_REDEEM_QUANTITY_FEEDBACK_FIX')) {
    $newRedeemUI = @'
    // HALLOWEEN_REDEEM_QUANTITY_FEEDBACK_FIX
    private void RedeemEnteredCode()
    {
        if (exchangeManager == null || promoCodeInput == null)
            return;

        string enteredCode = promoCodeInput.text;
        if (string.IsNullOrWhiteSpace(enteredCode))
            return;

        if (!exchangeManager.TryRedeemCode(
                enteredCode,
                out InventoryEntryDefinition reward,
                out int rewardAmount,
                out string reason))
        {
            if (redeemFeedbackText != null)
                redeemFeedbackText.text = reason;
            return;
        }

        bool english = LanguageManager.CurrentLanguage == Language.English;
        string rewardName = english ? reward.englishName : reward.russianName;
        string color = HalloweenRarityColorUtility.ToHex(reward.rarity);
        string quantity = reward.type == InventoryEntryType.StackableResource
            ? $" × {rewardAmount}"
            : string.Empty;

        if (redeemFeedbackText != null)
        {
            redeemFeedbackText.text = english
                ? $"Code accepted! You received <color=#{color}>{rewardName}</color>{quantity}."
                : $"Промокод принят! Получен предмет <color=#{color}>{rewardName}</color>{quantity}.";
        }

        promoCodeInput.text = string.Empty;
        RefreshAll();
    }

'@
    # In the current local UI, OpenHistory is public (not private).
    $ui = Replace-RegexBlock $ui '(?s)    private void RedeemEnteredCode\(\)\s*\{.*?(?=    public void OpenHistory\()' $newRedeemUI 'HalloweenExchangeUI.RedeemEnteredCode'
}

# Old or missing rewardAmount values in old records should still display at least x1.
if ($historyView.Contains('Mathf.Max(0, record.rewardAmount)')) {
    $historyView = $historyView.Replace(
        'Mathf.Max(0, record.rewardAmount)',
        'Mathf.Max(1, record.rewardAmount)')
}

# Final safety checks before writing any file.
if (-not $account.Contains('HALLOWEEN_HISTORY_QUANTITY_FIX_PURCHASE') -or
    -not $account.Contains('HALLOWEEN_HISTORY_QUANTITY_FIX_REDEEM')) {
    throw 'AccountManager: не все исправления подготовлены. Файлы не записаны.'
}
if (-not $manager.Contains('HALLOWEEN_HISTORY_QUANTITY_FIX_PURCHASE') -or
    -not $manager.Contains('HALLOWEEN_HISTORY_QUANTITY_FIX_REDEEM')) {
    throw 'HalloweenExchangeManager: не все исправления подготовлены. Файлы не записаны.'
}
if (-not $ui.Contains('HALLOWEEN_HISTORY_QUANTITY_FIX') -or
    -not $ui.Contains('historyItemViews') -or
    -not $ui.Contains('HALLOWEEN_REDEEM_QUANTITY_FEEDBACK_FIX')) {
    throw 'HalloweenExchangeUI: не все исправления подготовлены. Файлы не записаны.'
}

$updated = @(
    @{ Path = $paths.Account; Content = $account },
    @{ Path = $paths.Manager; Content = $manager },
    @{ Path = $paths.UI; Content = $ui },
    @{ Path = $paths.HistoryView; Content = $historyView }
)

$timestamp = Get-Date -Format 'yyyyMMdd-HHmmss'
foreach ($entry in $updated) {
    Copy-Item -LiteralPath $entry.Path -Destination ($entry.Path + ".before-history-quantity-fix-$timestamp.bak")
}

$utf8NoBom = New-Object -TypeName System.Text.UTF8Encoding -ArgumentList $false
foreach ($entry in $updated) {
    [System.IO.File]::WriteAllText($entry.Path, $entry.Content, $utf8NoBom)
}

Write-Host ''
Write-Host 'Halloween Exchange history and quantity fixes applied.' -ForegroundColor Green
Write-Host 'Updated:'
Write-Host ' - AccountManager.cs'
Write-Host ' - HalloweenExchangeManager.cs'
Write-Host ' - HalloweenExchangeUI.cs'
Write-Host ' - HalloweenPromoCodeHistoryItemView.cs'
Write-Host 'Backup files were created next to the originals.'
Write-Host ''
Write-Host 'In Unity, wait for compilation. Check History Content has a ScrollRect viewport and preferably a VerticalLayoutGroup.' -ForegroundColor Yellow
Write-Host 'The row height/vertical step is controlled by History Item Vertical Spacing (default 42).' -ForegroundColor Yellow
