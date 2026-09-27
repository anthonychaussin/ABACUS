# ABACUS.Generators

Roslyn incremental source generator that emits `Apply{TypeName}` helpers for types using `[AbacusField]`.

## Usage

```xml
<ProjectReference Include="path\to\ABACUS.Generators.csproj"
                  OutputItemType="Analyzer"
                  ReferenceOutputAssembly="false" />
```

Then:

```csharp
using ABACUS.Core.Generated;

services.AddAbacusFieldMapping(b => b.ApplyMyModel());
// or
services.AddAbacusFieldMappingFromAttributes_MyModel();
```
