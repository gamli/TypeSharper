namespace TypeSharper.Model.Type;

public partial interface TsType
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
