namespace TypeSharper.Diagnostics;

public enum EDiagnosticsCode
{
    UnknownGeneratorError,
    TypeHierarchyMustBePartial,
    TypeMustBeAbstract,
    ModelCreationFailedBecauseOfSymbolError,
    PropertyDoesNotExist,
    TargetTypeMustBeRecord,
}
