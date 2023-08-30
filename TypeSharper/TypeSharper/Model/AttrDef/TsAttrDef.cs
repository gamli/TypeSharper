using System;
using System.Linq;

namespace TypeSharper.Model.AttrDef;

public record TsAttrDef(
    TsName Name,
    AttributeTargets AttributeTargets,
    TsList<TsAttrOverloadDef> Overloads)
{
    public string Cs()
    {
        var attributeTargetEnumValues =
            Enum
                .GetValues(typeof(AttributeTargets))
                .Cast<AttributeTargets>()
                .Where(value => (AttributeTargets & value) != 0)
                .Where(value => value != AttributeTargets.All)
                .Select(value => "AttributeTargets." + Enum.GetName(typeof(AttributeTargets), value))
                .ToList();

        var attributeDefinitions =
            Overloads
                .Select(
                    overload =>
                        $$"""
                        {{$"[AttributeUsage({string.Join(" | ", attributeTargetEnumValues)})]"}}
                        public class {{Name.Cs()}}{{overload.CsTypeParameters()}} : Attribute
                        {
                        {{overload.CsConstructor(Name).Indent()}}

                        {{overload.CsProperties().Indent()}}
                        }
                            
                        """)
                .JoinLines();

        return
            $$"""
            using System;

            namespace TypeSharper.Attributes;

            {{attributeDefinitions}}
            """;
    }

    public override string ToString() => Name.Cs();

    public TsTypeRef TypeRef() => TsTypeRef.WithNs(TsAttr.NS, Name);
}
