using System;
using System.Security.Cryptography;

public static class HalloweenPromoCodeService
{
    private const string Alphabet =
        "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    public const int CodeLength = 8;
    private const int MaximumGenerationAttempts = 100;

    public static string Normalize(string rawCode)
    {
        return string.IsNullOrWhiteSpace(rawCode)
            ? string.Empty
            : rawCode.Trim().ToUpperInvariant();
    }

    public static bool IsValidFormat(string code)
    {
        if (string.IsNullOrEmpty(code) || code.Length != CodeLength)
            return false;

        for (int i = 0; i < code.Length; i++)
        {
            char value = code[i];

            bool isAlphaNumeric =
                (value >= 'A' && value <= 'Z') ||
                (value >= '0' && value <= '9');

            if (!isAlphaNumeric)
                return false;
        }

        return true;
    }

    public static bool TryGenerateUniqueCode(AccountManager account, out string code)
    {
        code = string.Empty;

        if (account == null)
            return false;

        using (RandomNumberGenerator generator =
               RandomNumberGenerator.Create())
        {
            byte[] randomBytes = new byte[CodeLength];

            for (int attempt = 0;
                 attempt < MaximumGenerationAttempts;
                 attempt++)
            {
                generator.GetBytes(randomBytes);

                char[] characters = new char[CodeLength];

                for (int i = 0; i < characters.Length; i++)
                    characters[i] =
                        Alphabet[randomBytes[i] % Alphabet.Length];

                string candidate = new string(characters);

                if (!account.HasHalloweenPromoCode(candidate))
                {
                    code = candidate;
                    return true;
                }
            }
        }

        return false;
    }

    public static bool TryCalculateExpiry(long createdAtUnixSeconds,int lifetimeDays,out long expiresAtUnixSeconds)
    {
        expiresAtUnixSeconds = 0;

        if (createdAtUnixSeconds <= 0 || lifetimeDays < 1)
            return false;

        try
        {
            expiresAtUnixSeconds = DateTimeOffset
                .FromUnixTimeSeconds(createdAtUnixSeconds)
                .AddDays(lifetimeDays)
                .ToUnixTimeSeconds();

            return expiresAtUnixSeconds > createdAtUnixSeconds;
        }
        catch (ArgumentOutOfRangeException)
        {
            return false;
        }
    }

    public static bool ValidateForRedemption(HalloweenPromoCodeRecord record,long nowUnixSeconds,out string failureReason)
    {
        failureReason = string.Empty;

        if (record == null)
            return Fail("Промокод не найден в истории этого аккаунта.",out failureReason);

        if (record.isRedeemed)
            return Fail("Этот промокод уже использован.",out failureReason);

        if (record.expiresAtUnixSeconds <= 0 || nowUnixSeconds >= record.expiresAtUnixSeconds)
            return Fail("Срок действия промокода истёк.",out failureReason);
        return true;
    }

    private static bool Fail(string message,out string failureReason)
    {
        failureReason = message;
        return false;
    }
}