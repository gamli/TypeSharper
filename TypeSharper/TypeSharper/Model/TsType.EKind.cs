namespace TypeSharper.Model;

public abstract partial record TsType
{
    public enum EKind
    {
        Interface,
        Class,
        RecordClass,
        Struct,
        RecordStruct,
        Special,
    }
}
