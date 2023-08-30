using System.Linq;

namespace TypeSharper.Model;

public abstract partial record TsType
{
    public abstract record PropertySelection(TypeInfo Info, TsTypeRef FromType, TsUniqueList<TsProp> Props)
        : PropertyDuck(Info, Props)
    {
        protected override Maybe<string> CsBody(TsModel model)
        {
            var csTypeName = Info.Name.Cs();
            var csFromTypeName = FromType.Cs();
            var bodyContent =
                $$"""
                    public {{csTypeName}}({{csFromTypeName}} from)
                    : this({{Props.Select(prop => prop.CsGetFrom("from")).JoinList()}}) { }

                    public static implicit operator {{csTypeName}}({{csFromTypeName}} from)
                        => new(from);
                    """;
            return $$"""
                {
                {{bodyContent.Indent()}}
                }
                """;
        }
    }
}
