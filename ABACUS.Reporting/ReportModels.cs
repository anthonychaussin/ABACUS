using System.Text.Json.Serialization;

namespace ABACUS.Reporting;

/// <summary>Parameters used to start an AbaReport export.</summary>
public sealed record ReportRequest(
    [property: JsonPropertyName("outputType")] string OutputType,
    [property: JsonPropertyName("paging")] int Paging = 200,
    [property: JsonPropertyName("parameters")] IReadOnlyDictionary<string, string>? Parameters = null);

/// <summary>Asynchronous AbaReport job returned by the API.</summary>
public sealed record ReportJob(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("state")] string State,
    [property: JsonPropertyName("message")] string? Message,
    [property: JsonPropertyName("submittedAt")] DateTimeOffset? SubmittedAt,
    [property: JsonPropertyName("startedAt")] DateTimeOffset? StartedAt,
    [property: JsonPropertyName("finishedAt")] DateTimeOffset? FinishedAt);

/// <summary>One output page, preserved as bytes to support text and binary V2025/V2026 formats.</summary>
public sealed record ReportOutput(byte[] Content, string? ContentType);

/// <summary>Output type values supported by current AbaReport v1 releases.</summary>
public static class ReportOutputTypes
{
    public const string Text = "txt";
    public const string TextAll = "txt_all";
    public const string TextCString = "txt_cstr";
    public const string TextAllCString = "txt_allcstr";
    public const string XmlQuery = "xml_query";
    public const string Xml = "xml";
    public const string Json = "json";
    public const string JsonCompact = "json_compact";
    public const string JsonUserDefined = "json_userdef";
    public const string JsonUserDefinedCompact = "json_userdef_compact";
    public const string Pdf = "pdf";
    public const string ExcelData = "excel-xlsx_data";
    public const string ExcelMatrix = "excel-xlsx_matrix";
    public const string Excel = "excel-xlsx";
    public const string Word = "word-docx";
}
