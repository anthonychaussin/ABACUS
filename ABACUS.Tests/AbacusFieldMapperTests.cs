using ABACUS.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ABACUS.Tests;

public sealed class AbacusFieldMapperTests
{
    [Fact]
    public void ToPayload_MapsRenamesAndNestsPaths_OmitsNullAndUnmapped()
    {
        var mapping = new AbacusFieldMappingBuilder()
            .Entity("Supplier")
            .Field("Name", "Name")
            .Field("VatNumber", "UserFields.UserField1")
            .Field("City", "Address.City")
            .Build();
        var mapper = new AbacusFieldMapper(mapping);

        var payload = mapper.ToPayload("Supplier", new SupplierModel
        {
            Name = "Acme",
            VatNumber = "CHE-123",
            City = "Zurich",
            Ignored = "should-not-appear",
            Notes = null,
        });

        Assert.Equal("Acme", payload["Name"]);
        Assert.False(payload.ContainsKey("VatNumber"));
        Assert.False(payload.ContainsKey("Ignored"));
        Assert.False(payload.ContainsKey("Notes"));

        var userFields = Assert.IsType<Dictionary<string, object?>>(payload["UserFields"]);
        Assert.Equal("CHE-123", userFields["UserField1"]);

        var address = Assert.IsType<Dictionary<string, object?>>(payload["Address"]);
        Assert.Equal("Zurich", address["City"]);
    }

    [Fact]
    public void ToPayload_UsesAttributes_AndConfigOverridesAttribute()
    {
        var mapping = new AbacusFieldMappingBuilder()
            .Entity("Supplier")
            .Field(nameof(AttributedSupplier.VatNumber), "UserFields.CustomVat")
            .Build();
        var mapper = new AbacusFieldMapper(mapping);

        var payload = mapper.ToPayload("Supplier", new AttributedSupplier
        {
            Name = "Acme",
            VatNumber = "CHE-999",
        });

        Assert.Equal("Acme", payload["Name"]);
        var userFields = Assert.IsType<Dictionary<string, object?>>(payload["UserFields"]);
        Assert.Equal("CHE-999", userFields["CustomVat"]);
        Assert.False(userFields.ContainsKey("UserField1"));
    }

    [Fact]
    public void ToPayload_MapsNestedObjectWhenEntityMapExists()
    {
        var mapping = new AbacusFieldMappingBuilder()
            .Entity("Supplier")
            .Field("Name", "Name")
            .Entity("AddressModel")
            .Field("City", "City")
            .Field("Zip", "PostalCode")
            .Build();
        var mapper = new AbacusFieldMapper(mapping);

        var payload = mapper.ToPayload("Supplier", new SupplierWithAddress
        {
            Name = "Acme",
            Address = new AddressModel
            {
                City = "Bern",
                Zip = "3000",
            },
        });

        Assert.Equal("Acme", payload["Name"]);
        var address = Assert.IsType<Dictionary<string, object?>>(payload["Address"]);
        Assert.Equal("Bern", address["City"]);
        Assert.Equal("3000", address["PostalCode"]);
    }

    [Fact]
    public void FromPayload_ReadsNestedPathsIntoModel()
    {
        var mapping = new AbacusFieldMappingBuilder()
            .Entity("Supplier")
            .Field("Name", "Name")
            .Field("VatNumber", "UserFields.UserField1")
            .Field("City", "Address.City")
            .Build();
        var mapper = new AbacusFieldMapper(mapping);

        var payload = new Dictionary<string, object?>
        {
            ["Name"] = "Acme",
            ["UserFields"] = new Dictionary<string, object?>
            {
                ["UserField1"] = "CHE-123",
            },
            ["Address"] = new Dictionary<string, object?>
            {
                ["City"] = "Zurich",
            },
        };

        var model = mapper.FromPayload<SupplierModel>("Supplier", payload);

        Assert.Equal("Acme", model.Name);
        Assert.Equal("CHE-123", model.VatNumber);
        Assert.Equal("Zurich", model.City);
    }

    [Fact]
    public void FromPayload_MapsNestedObjectEntity()
    {
        var mapping = new AbacusFieldMappingBuilder()
            .Entity("Supplier")
            .Field("Name", "Name")
            .Entity("AddressModel")
            .Field("City", "City")
            .Field("Zip", "PostalCode")
            .Build();
        var mapper = new AbacusFieldMapper(mapping);

        var payload = new Dictionary<string, object?>
        {
            ["Name"] = "Acme",
            ["Address"] = new Dictionary<string, object?>
            {
                ["City"] = "Bern",
                ["PostalCode"] = "3000",
            },
        };

        var model = mapper.FromPayload<SupplierWithAddress>("Supplier", payload);

        Assert.Equal("Acme", model.Name);
        Assert.NotNull(model.Address);
        Assert.Equal("Bern", model.Address!.City);
        Assert.Equal("3000", model.Address.Zip);
    }

    [Fact]
    public void AddAbacusFieldMapping_ConfigurationOverridesFluentFieldByField()
    {
        const string json = """
            {
              "Abacus": {
                "FieldMaps": {
                  "Supplier": {
                    "Name": "Name",
                    "VatNumber": "UserFields.FromConfig"
                  }
                }
              }
            }
            """;
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json));
        var configuration = new ConfigurationBuilder()
            .AddJsonStream(stream)
            .Build();

        var services = new ServiceCollection();
        services.AddAbacusFieldMapping(
            map => map
                .Entity("Supplier")
                .Field("Name", "Name")
                .Field("VatNumber", "UserFields.FromCode"),
            configuration);

        using var provider = services.BuildServiceProvider();
        var mapper = provider.GetRequiredService<IAbacusFieldMapper>();

        var payload = mapper.ToPayload("Supplier", new SupplierModel
        {
            Name = "Acme",
            VatNumber = "CHE-1",
        });

        var userFields = Assert.IsType<Dictionary<string, object?>>(payload["UserFields"]);
        Assert.Equal("CHE-1", userFields["FromConfig"]);
        Assert.False(userFields.ContainsKey("FromCode"));
    }

    private sealed class SupplierModel
    {
        public string? Name { get; set; }
        public string? VatNumber { get; set; }
        public string? City { get; set; }
        public string? Ignored { get; set; }
        public string? Notes { get; set; }
    }

    private sealed class AttributedSupplier
    {
        [AbacusField("Name")]
        public string? Name { get; set; }

        [AbacusField("UserFields.UserField1")]
        public string? VatNumber { get; set; }
    }

    private sealed class SupplierWithAddress
    {
        public string? Name { get; set; }
        public AddressModel? Address { get; set; }
    }

    private sealed class AddressModel
    {
        public string? City { get; set; }
        public string? Zip { get; set; }
    }
}
