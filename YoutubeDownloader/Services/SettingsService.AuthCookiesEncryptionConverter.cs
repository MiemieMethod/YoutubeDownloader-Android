using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace YoutubeDownloader.Services;

public partial class SettingsService
{
    private class AuthCookiesEncryptionConverter : JsonConverter<IReadOnlyList<Cookie>?>
    {
        private const string KeyStorageName = "auth_cookies_key";

        // On Android, the encryption key is randomly generated and kept in the secure storage
        // (backed by the Android Keystore) instead of being derived from the machine ID.
        private static readonly Lazy<byte[]?> Key = new(() =>
        {
            try
            {
                return Task.Run(async () =>
                    {
                        var storedKey = await SecureStorage.Default.GetAsync(KeyStorageName);
                        if (!string.IsNullOrWhiteSpace(storedKey))
                            return Convert.FromBase64String(storedKey);

                        var key = RandomNumberGenerator.GetBytes(32);
                        await SecureStorage.Default.SetAsync(
                            KeyStorageName,
                            Convert.ToBase64String(key)
                        );

                        return key;
                    })
                    .GetAwaiter()
                    .GetResult();
            }
            catch
            {
                // Secure storage is not available, cookies won't be persisted
                return null;
            }
        });

        public override IReadOnlyList<Cookie>? Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options
        )
        {
            if (reader.TokenType != JsonTokenType.String)
                return null;

            var value = reader.GetString();
            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (Key.Value is not { } key)
                return null;

            try
            {
                var encryptedData = Convert.FromHexString(value);
                var cookieData = new byte[encryptedData.AsSpan(28).Length];

                // Layout: nonce (12 bytes) | tag (16 bytes) | cipher
                using var aes = new AesGcm(key, 16);
                aes.Decrypt(
                    encryptedData.AsSpan(0, 12),
                    encryptedData.AsSpan(28),
                    encryptedData.AsSpan(12, 16),
                    cookieData
                );

                return JsonSerializer
                    .Deserialize(cookieData, CookieSerializerContext.Default.CookieDataArray)
                    ?.Select(c => new Cookie(c.Name, c.Value, c.Path, c.Domain))
                    .ToArray();
            }
            catch (Exception ex)
                when (ex
                        is FormatException
                            or CryptographicException
                            or ArgumentException
                            or IndexOutOfRangeException
                            or JsonException
                            or CookieException
                )
            {
                return null;
            }
        }

        public override void Write(
            Utf8JsonWriter writer,
            IReadOnlyList<Cookie>? value,
            JsonSerializerOptions options
        )
        {
            if (value is null || value.Count == 0 || Key.Value is not { } key)
            {
                writer.WriteNullValue();
                return;
            }

            var cookieData = JsonSerializer.SerializeToUtf8Bytes(
                value.Select(c => new CookieData(c.Name, c.Value, c.Path, c.Domain)).ToArray(),
                CookieSerializerContext.Default.CookieDataArray
            );

            var encryptedData = new byte[28 + cookieData.Length];

            // Nonce
            RandomNumberGenerator.Fill(encryptedData.AsSpan(0, 12));

            // Layout: nonce (12 bytes) | tag (16 bytes) | cipher
            using var aes = new AesGcm(key, 16);
            aes.Encrypt(
                encryptedData.AsSpan(0, 12),
                cookieData,
                encryptedData.AsSpan(28),
                encryptedData.AsSpan(12, 16)
            );

            writer.WriteStringValue(Convert.ToHexStringLower(encryptedData));
        }
    }

    internal record CookieData(string Name, string Value, string Path, string Domain);

    [JsonSerializable(typeof(CookieData[]))]
    internal partial class CookieSerializerContext : JsonSerializerContext;
}
