using ABACUS.Core;

namespace ABACUS.FieldInformation;

/// <summary>
/// Default implementation for the FieldInformation module wrapper.
/// </summary>
public sealed class FieldInformationClient : IFieldInformationClient
{
    private readonly HttpClient _httpClient;

    /// <inheritdoc />
    public string ModuleName => "FieldInformation";

    /// <inheritdoc />
    public ABACUS_FieldinformationClient Raw { get; }

    /// <summary>
    /// Creates the module client from a configured <see cref="HttpClient"/>.
    /// </summary>
    public FieldInformationClient(HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        _httpClient = httpClient;
        Raw = new ABACUS_FieldinformationClient(httpClient);
    }

    /// <inheritdoc />
    public Task<AbacusResponse<string>> GetFieldInformationsAsync(
        object payload,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(payload);
        return AbacusHttp.SendAsync(
            _httpClient,
            HttpMethod.Post,
            "/FieldInformations",
            payload,
            prefer: null,
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlySet<string>> DiscoverFieldPathsAsync(
        object payload,
        CancellationToken cancellationToken = default)
    {
        var response = await GetFieldInformationsAsync(payload, cancellationToken).ConfigureAwait(false);
        return AbacusFieldInformationMapper.ExtractFieldPaths(response.Data);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<string>> ValidateMappingAsync(
        IAbacusFieldMapper mapper,
        AbacusFieldMapping mapping,
        string entity,
        object fieldInformationPayload,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(mapper);
        ArgumentNullException.ThrowIfNull(mapping);
        _ = mapper; // reserved for future attribute-aware validation

        var known = await DiscoverFieldPathsAsync(fieldInformationPayload, cancellationToken).ConfigureAwait(false);
        return AbacusFieldInformationMapper.FindUnknownPaths(mapping, entity, known);
    }
}
