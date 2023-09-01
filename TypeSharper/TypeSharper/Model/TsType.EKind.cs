namespace TypeSharper.Model;

public abstract partial record TsType
{
    #region Nested types

    public enum EKind
    {
        Interface,
        Class,
        RecordClass,
        Struct,
        RecordStruct,
        Special,
    }

    #endregion
}
