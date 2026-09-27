namespace ABACUS.Testing;

/// <summary>
/// Minimal fake handler that records requests and returns a fixed or custom response.
/// </summary>
public sealed class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, HttpResponseMessage> _responseFactory;

    /// <summary>
    /// Requests observed by this handler.
    /// </summary>
    public List<HttpRequestMessage> Requests { get; } = new();

    /// <summary>
    /// Creates a fake handler that defaults to an empty OData collection.
    /// </summary>
    public FakeHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage>? responseFactory = null)
    {
        _responseFactory = responseFactory ?? (_ => ODataResponseFactory.EmptyPage());
    }

    /// <inheritdoc />
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Requests.Add(request);
        return Task.FromResult(_responseFactory(request));
    }
}
