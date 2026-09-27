using System.Net;
using System.Net.Http;

namespace ABACUS.Tests.Testing;

internal sealed class CapturingHttpMessageHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> _responseFactory;

    public List<CapturedHttpRequest> Requests { get; } = new();

    public CapturingHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, HttpResponseMessage>? responseFactory = null)
    {
        _responseFactory = responseFactory ?? DefaultResponseFactory;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var body = request.Content is null
            ? null
            : await request.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        string? prefer = null;
        if (request.Headers.TryGetValues("Prefer", out var preferValues))
        {
            prefer = string.Join(", ", preferValues);
        }

        Requests.Add(new CapturedHttpRequest(
            request.Method,
            request.RequestUri,
            body,
            request.Content?.Headers.ContentType?.ToString(),
            request.Headers.Authorization?.ToString(),
            prefer,
            cancellationToken));

        return _responseFactory(request, cancellationToken);
    }

    private static HttpResponseMessage DefaultResponseFactory(HttpRequestMessage _, CancellationToken __) =>
        new(HttpStatusCode.OK);
}

internal sealed record CapturedHttpRequest(
    HttpMethod Method,
    Uri? RequestUri,
    string? Body,
    string? ContentType,
    string? Authorization,
    string? Prefer,
    CancellationToken CancellationToken);
