using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.WebUtilities;

namespace Inventario.Web.Middleware;

public static class HttpLoggingSanitizer
{
    private static readonly HashSet<string> SensitiveFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "password",
        "currentPassword",
        "newPassword",
        "confirmPassword",
        "accessToken",
        "refreshToken",
        "token",
        "idToken",
        "authorization",
        "cookie",
        "set-cookie",
        "secret",
        "clientSecret",
        "apiKey",
        "key",
        "privateKey",
        "credential",
        "requestverificationtoken",
        "__RequestVerificationToken",
        "idempotency-key"
    };

    public static string MaskQueryString(string? queryString)
    {
        if (string.IsNullOrWhiteSpace(queryString))
        {
            return string.Empty;
        }

        var parsed = QueryHelpers.ParseQuery(queryString);
        var sanitized = parsed.ToDictionary(
            item => item.Key,
            item => IsSensitive(item.Key)
                ? new[] { MaskTokenValue(item.Value.ToString()) }
                : item.Value.Select(v => v).ToArray());

        return JsonSerializer.Serialize(sanitized);
    }

    public static string MaskHeaders(IHeaderDictionary headers)
    {
        var sanitized = headers.ToDictionary(
            item => item.Key,
            item => IsSensitive(item.Key)
                ? MaskTokenValue(item.Value.ToString())
                : item.Value.ToString());

        return JsonSerializer.Serialize(sanitized);
    }

    public static string MaskBody(string? body, string? contentType)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return string.Empty;
        }

        contentType ??= string.Empty;

        if (contentType.Contains("application/json", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                var node = JsonNode.Parse(body);
                SanitizeNode(node, null);
                return node?.ToJsonString(new JsonSerializerOptions { WriteIndented = false }) ?? string.Empty;
            }
            catch
            {
                return body;
            }
        }

        if (contentType.Contains("application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase))
        {
            var parsed = QueryHelpers.ParseQuery(body);
            var sanitized = parsed.ToDictionary(
                item => item.Key,
                item => IsSensitive(item.Key)
                    ? MaskTokenValue(item.Value.ToString())
                    : item.Value.ToString());
            return JsonSerializer.Serialize(sanitized);
        }

        return body;
    }

    public static string ComputeFingerprint(string? content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return string.Empty;
        }

        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(content)));
    }

    public static string MaskTokenValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "***enmascarado***";
        }

        return $"***enmascarado:{ComputeFingerprint(value)[..12]}***";
    }

    private static void SanitizeNode(JsonNode? node, string? propertyName)
    {
        if (node is null)
        {
            return;
        }

        if (node is JsonObject obj)
        {
            foreach (var item in obj.ToList())
            {
                if (item.Key is null)
                {
                    continue;
                }

                if (IsSensitive(item.Key))
                {
                    obj[item.Key] = MaskTokenValue(item.Value?.ToJsonString() ?? string.Empty);
                    continue;
                }

                SanitizeNode(item.Value, item.Key);
            }

            return;
        }

        if (node is JsonArray array)
        {
            foreach (var item in array)
            {
                SanitizeNode(item, propertyName);
            }

            return;
        }

        if (propertyName is not null && IsSensitive(propertyName) && node is JsonValue value)
        {
            var rawValue = value.ToJsonString().Trim('"');
            node.ReplaceWith(JsonValue.Create(MaskTokenValue(rawValue)));
        }
    }

    private static bool IsSensitive(string key)
    {
        return SensitiveFields.Contains(key) || key.Contains("token", StringComparison.OrdinalIgnoreCase);
    }
}
