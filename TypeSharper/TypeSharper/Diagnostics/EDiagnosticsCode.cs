namespace TypeSharper.Diagnostics;

public enum EDiagnosticsCode
{
    UnknownGeneratorError,
    TargetTypeHierarchyIsNotPartial,
    TaggedUnionTargetTypeIsNotAbstract,
    IntersectionOfTaggedUnionsIsNotSupported,
    ProductOfTaggedUnionsIsNotSupported,
    TargetTypeSymbolHasError,
    PropertyDoesNotExist,
    TargetTypeIsNotARecord,
    MultipleTsAttributesAreNotAllowed,
}
