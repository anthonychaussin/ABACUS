using System.Text.Json;
using ABACUS.Core;

namespace ABACUS.CRM;

/// <summary>
/// High-level SDK facade for the CRM module.
/// </summary>
public interface ICRMClient : IAbacusModuleClient
{
    /// <summary>
    /// Underlying generated client for advanced or not-yet-wrapped endpoints.
    /// </summary>
    ABACUS_CRMClient Raw { get; }

    /// <summary>
    /// Lists subjects (first page).
    /// </summary>
    Task<ODataPage<JsonElement>> ListSubjectsAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Enumerates subjects following <c>@odata.nextLink</c>.
    /// </summary>
    IAsyncEnumerable<JsonElement> EnumerateSubjectsAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a subject by id.
    /// </summary>
    Task<AbacusResponse<JsonElement>> GetSubjectAsync(
        string id,
        ODataQuery? query = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a subject from a JSON-compatible payload.
    /// </summary>
    Task<AbacusResponse<string>> CreateSubjectAsync(
        object payload,
        string? prefer = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Patches a subject.
    /// </summary>
    Task<AbacusResponse<string>> PatchSubjectAsync(
        string id,
        object payload,
        string? prefer = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a subject.
    /// </summary>
    Task<AbacusResponse<string>> DeleteSubjectAsync(
        string id,
        string? prefer = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists addresses (first page).
    /// </summary>
    Task<ODataPage<JsonElement>> ListAddressesAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default);
}
