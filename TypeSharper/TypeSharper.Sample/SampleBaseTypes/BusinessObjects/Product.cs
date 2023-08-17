namespace TypeSharper.Sample.SampleBaseTypes.BusinessObjects;

public record Product(
    string Name,
    decimal Price,
    string Brand,
    string Model,
    string SKU,
    string UPC,
    string Manufacturer,
    string Category,
    string SubCategory,
    string Description,
    string Color,
    string Size,
    int Stock,
    bool IsAvailable,
    string Warranty);
