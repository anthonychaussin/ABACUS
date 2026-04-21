using System.Net;
using System.Net.Http;

namespace ABACUS.Tests.Testing;

internal sealed class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, HttpResponseMessage> _responseFactory;

    public List<HttpRequestMessage> Requests { get; } = new();

    public FakeHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage>? responseFactory = null)
    {
        _responseFactory = responseFactory ?? (_ => new HttpResponseMessage(HttpStatusCode.OK));
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Requests.Add(request);
        return Task.FromResult(_responseFactory(request));
    }
}
