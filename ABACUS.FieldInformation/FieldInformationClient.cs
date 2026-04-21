using ABACUS.Core;

namespace ABACUS.FieldInformation;

/// <summary>
/// Default implementation for the FieldInformation module wrapper.
/// </summary>
public sealed class FieldInformationClient : IFieldInformationClient
{
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
        Raw = new ABACUS_FieldinformationClient(httpClient);
    }
}
