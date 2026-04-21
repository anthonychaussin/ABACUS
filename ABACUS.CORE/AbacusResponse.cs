namespace ABACUS.Core;

/// <summary>
/// Generic envelope for response payload plus HTTP metadata.
/// </summary>
public sealed record AbacusResponse<T>(
    T Data,
    int StatusCode,
    IReadOnlyDictionary<string, IEnumerable<string>> Headers);
