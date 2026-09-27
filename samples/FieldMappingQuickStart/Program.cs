using ABACUS.Core;
using ABACUS.Core.Generated;

var mapping = new AbacusFieldMappingBuilder()
    .ApplyDemoSupplier()
    .Build();
var mapper = new AbacusFieldMapper(mapping);

var payload = mapper.ToPayload("DemoSupplier", new DemoSupplier { Name = "Acme SA", VatNumber = "CHE-123" });
Console.WriteLine("Mapped payload keys: " + string.Join(", ", payload.Keys));
Console.WriteLine("Call AccountsPayableClient.CreateSupplierAsync(model, mapper) with a live HttpClient.");

internal sealed class DemoSupplier
{
    [AbacusField("Name")]
    public string Name { get; set; } = "";

    [AbacusField("UserFields.UserField1")]
    public string VatNumber { get; set; } = "";
}
