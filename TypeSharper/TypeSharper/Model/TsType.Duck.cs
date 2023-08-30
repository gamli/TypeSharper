namespace TypeSharper.Model;

public abstract partial record TsType
{
    public abstract record Duck(TypeInfo Info) : TsType(Info);
}
