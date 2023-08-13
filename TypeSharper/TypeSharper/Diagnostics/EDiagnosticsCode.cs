namespace TypeSharper.Diagnostics;

public enum EDiagnosticsCode
{
    UnknownGeneratorError,
    TypeHierarchyMustBePartial,
    TypeMustBeInterfaceOrClass,
    TypeMustBeAbstract,
    ModelCreationFailedBecauseOfSymbolError,
    PropertyDoesNotExist,
}
