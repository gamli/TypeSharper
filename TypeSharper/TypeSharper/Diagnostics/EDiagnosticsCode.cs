namespace TypeSharper.Diagnostics;

public enum EDiagnosticsCode
{
    UnknownGeneratorError,
    TargetTypeHierarchyIsNotPartial,
    TaggedUnionTargetTypeIsNotAbstract,
    IntersectionOfTaggedUnionsIsNotSupported,
    TargetTypeSymbolHasError,
    PropertyDoesNotExist,
    TargetTypeIsNotARecord,
    MultipleTsAttributesAreNotAllowed,
}
