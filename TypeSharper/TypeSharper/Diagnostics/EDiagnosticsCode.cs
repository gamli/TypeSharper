namespace TypeSharper.Diagnostics;

public enum EDiagnosticsCode
{
    UnknownGeneratorError,
    TargetTypeHierarchyIsNotPartial,
    TaggedUnionTargetTypeIsNotAbstract,
    IntersectionOfTaggedUnionsIsNotSupported,
    ProductOfTaggedUnionsIsNotSupported,
    TargetTypeSymbolHasError,
    SelectedPropertyDoesNotExist,
    MappedPropertyDoesNotExist,
    TargetTypeIsNotARecord,
    MultipleTsAttributesAreNotAllowed,
}
