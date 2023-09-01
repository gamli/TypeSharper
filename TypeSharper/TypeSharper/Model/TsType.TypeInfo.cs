using System;

namespace TypeSharper.Model;

public abstract partial record TsType
{
    #region Nested types

    public record TypeInfo(
        TsName Name,
        Maybe<TsTypeRef> ContainingType,
        TsNs Ns,
        EKind TypeKind,
        TsTypeMods Mods) : IComparable<TypeInfo>
    {
        public int CompareTo(TypeInfo other)
        {
            var comparedMods = Mods.CompareTo(other.Mods);
            return comparedMods != 0 ? comparedMods : Name.CompareTo(other.Name);
        }

        public string Cs(string csBody, TsModel model)
        {
            var inner = $"{Mods.Cs().MarginRight()}{CsKind()} {Name.Cs()}{csBody.Indent()}";
            return ContainingType.Map(
                typeRef => model.Resolve(typeRef).Info.Cs($"\n{{\n{inner.Indent()}\n}}", model),
                () => inner);
        }

        public TsTypeRef Ref()
            => ContainingType.Map(
                ct => ct.AddId(Name),
                () => TsTypeRef.WithNs(Ns.FullyQualifiedName, Name));

        #region Private

        private string CsKind()
            => TypeKind switch
            {
                EKind.Interface    => "interface",
                EKind.Class        => "class",
                EKind.RecordClass  => "record",
                EKind.Struct       => "struct",
                EKind.RecordStruct => "record struct",
                _                  => throw new ArgumentOutOfRangeException(nameof(TypeKind), TypeKind, null),
            };

        #endregion
    }

    #endregion
}
