using System.Text.Json;

namespace ABACUS.Core;

/// <summary>
/// Parses classic OData error payloads and builds <see cref="AbacusApiException"/> instances.
/// </summary>
public static class AbacusODataErrorParser
{
    /// <summary>
    /// Creates the appropriate exception from an HTTP failure body.
    /// </summary>
    public static AbacusApiException CreateException(
        int statusCode,
        string? responseBody,
        IReadOnlyDictionary<string, IEnumerable<string>>? headers = null,
        Exception? innerException = null,
        string? fallbackMessage = null)
    {
        TryParse(responseBody, out var code, out var message, out var details);
        var display = !string.IsNullOrWhiteSpace(message)
            ? message!
            : !string.IsNullOrWhiteSpace(fallbackMessage)
                ? fallbackMessage!
                : $"The HTTP status code of the response was not expected ({statusCode}).";

        if (statusCode == 400 && details.Count > 0)
        {
            return new AbacusValidationException(
                message: display,
                statusCode: statusCode,
                responseBody: responseBody,
                headers: headers,
                innerException: innerException,
                odataErrorCode: code,
                odataMessage: message,
                details: details);
        }

        return new AbacusApiException(
            message: display,
            statusCode: statusCode,
            responseBody: responseBody,
            headers: headers,
            innerException: innerException,
            odataErrorCode: code,
            odataMessage: message,
            details: details);
    }

    /// <summary>
    /// Tries to parse an OData error JSON body.
    /// </summary>
    public static bool TryParse(
        string? json,
        out string? code,
        out string? message,
        out IReadOnlyList<AbacusODataErrorDetail> details)
    {
        code = null;
        message = null;
        details = Array.Empty<AbacusODataErrorDetail>();
        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            using var doc = JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty("error", out var error) ||
                error.ValueKind != JsonValueKind.Object)
            {
                return false;
            }

            if (error.TryGetProperty("code", out var codeEl))
            {
                code = codeEl.GetString();
            }

            if (error.TryGetProperty("message", out var msgEl))
            {
                message = msgEl.ValueKind == JsonValueKind.Object && msgEl.TryGetProperty("value", out var valueEl)
                    ? valueEl.GetString()
                    : msgEl.ValueKind == JsonValueKind.String
                        ? msgEl.GetString()
                        : null;
            }

            if (error.TryGetProperty("details", out var detailsEl) && detailsEl.ValueKind == JsonValueKind.Array)
            {
                var list = new List<AbacusODataErrorDetail>();
                foreach (var item in detailsEl.EnumerateArray())
                {
                    if (item.ValueKind != JsonValueKind.Object)
                    {
                        continue;
                    }

                    list.Add(new AbacusODataErrorDetail(
                        item.TryGetProperty("code", out var c) ? c.GetString() : null,
                        item.TryGetProperty("message", out var m) ? m.GetString() : null,
                        item.TryGetProperty("target", out var t) ? t.GetString() : null));
                }

                details = list;
            }

            return code is not null || message is not null || details.Count > 0;
        }
        catch (JsonException)
        {
            return false;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }
}
