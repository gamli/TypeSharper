namespace TypeSharper.Model;

public abstract partial record TsType
{
    #region Nested types

    public abstract record Duck(TypeInfo Info) : TsType(Info);

    #endregion
}
