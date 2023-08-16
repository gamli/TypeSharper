using System;
using System.Linq;
using TypeSharper.Model.Identifier;
using TypeSharper.Model.Type;

namespace TypeSharper.Model.Attr.Def;

public record TsAttrDef(
    TsId Id,
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
                        public class {{Id.Cs()}}{{overload.CsTypeParameters()}} : Attribute
                        {
                        {{overload.CsConstructor(Id).Indent()}}

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

    public override string ToString() => Cs();


    public TsTypeRef TypeRef() => new(TsNs.Qualified(TsAttr.ATTRIBUTE_NAMESPACE_ID), new TsQualifiedId(Id));

    #region Equality Members

    public virtual bool Equals(TsAttrDef? other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Id.Equals(other.Id)
               && AttributeTargets == other.AttributeTargets
               && Overloads.Equals(other.Overloads);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = Id.GetHashCode();
            hashCode = (hashCode * 397) ^ (int)AttributeTargets;
            hashCode = (hashCode * 397) ^ Overloads.GetHashCode();
            return hashCode;
        }
    }

    #endregion
}
