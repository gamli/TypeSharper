using System;

namespace TypeSharper.Model;

public abstract partial record TsType
{
    #region Nested types

    public sealed record Native(TypeInfo Info, TsUniqueList<TsProp> Props) : TsType(Info)
    {
        public override string ToString() => $"Native:{base.ToString()}";

        public override string Cs(TsModel model)
            => throw new NotSupportedException(
                "Native types are only intended to read from. Use Ducks for generation.");
    }

    #endregion
}
