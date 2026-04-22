using ABACUS.Core;

namespace ABACUS.Tests;

public sealed class AbacusExceptionMapperTests
{
    [Fact]
    public void Map_ReturnsSameInstance_ForAbacusApiException()
    {
        var exception = new AbacusApiException("already mapped", statusCode: 400);

        var mapped = AbacusExceptionMapper.Map(exception);

        Assert.Same(exception, mapped);
    }

    [Fact]
    public void Map_ReturnsOriginalException_WhenExceptionHasNoGeneratedClientShape()
    {
        var exception = new InvalidOperationException("boom");

        var mapped = AbacusExceptionMapper.Map(exception);

        Assert.Same(exception, mapped);
    }

    [Fact]
    public void Map_ConvertsGeneratedLikeException_ToAbacusApiException()
    {
        IReadOnlyDictionary<string, IEnumerable<string>> headers = new Dictionary<string, IEnumerable<string>>
        {
            ["X-Trace-Id"] = ["trace-123"],
        };
        var exception = new GeneratedLikeException("bad request", 400, "{\"error\":true}", headers);

        var mapped = Assert.IsType<AbacusApiException>(AbacusExceptionMapper.Map(exception));

        Assert.Equal("bad request", mapped.Message);
        Assert.Equal(400, mapped.StatusCode);
        Assert.Equal("{\"error\":true}", mapped.ResponseBody);
        Assert.Same(headers, mapped.Headers);
        Assert.Same(exception, mapped.InnerException);
    }

    [Fact]
    public void Map_UsesAvailableResponseMetadata_EvenWithoutStatusCode()
    {
        var exception = new ResponseOnlyException("unexpected", "{\"error\":true}");

        var mapped = Assert.IsType<AbacusApiException>(AbacusExceptionMapper.Map(exception));

        Assert.Equal("unexpected", mapped.Message);
        Assert.Null(mapped.StatusCode);
        Assert.Equal("{\"error\":true}", mapped.ResponseBody);
        Assert.Same(exception, mapped.InnerException);
    }

    private sealed class GeneratedLikeException : Exception
    {
        public GeneratedLikeException(
            string message,
            int statusCode,
            string response,
            IReadOnlyDictionary<string, IEnumerable<string>> headers)
            : base(message)
        {
            StatusCode = statusCode;
            Response = response;
            Headers = headers;
        }

        public int StatusCode { get; }

        public string Response { get; }

        public IReadOnlyDictionary<string, IEnumerable<string>> Headers { get; }
    }

    private sealed class ResponseOnlyException : Exception
    {
        public ResponseOnlyException(string message, string response)
            : base(message)
        {
            Response = response;
        }

        public string Response { get; }
    }
}
