using System.Collections.Generic;
using System.Linq;
using TypeSharper.Model.Identifier;
using TypeSharper.Model.Type;

namespace TypeSharper.Model.Attr;

public record TsAttr(
    TsTypeRef Type,
    bool IsTsAttr,
    TsList<TsAttrValue> CtorArgs,
    Dictionary<TsId, TsAttrValue> NamedArgs,
    TsList<TsTypeRef> TypeArgs)
{
    public static readonly TsQualifiedId ATTRIBUTE_NAMESPACE_ID = new("TypeSharper", "Attributes");
    public string Cs() => $"[{Type.Cs()}{CsTypeArgs()}({CsCtorArgs()}){CsNamedArgs()}]";

    public TsList<string> FlattenedArgs()
        => TsList.Create(
            CtorArgs
                .SelectMany(
                    attrValue =>
                        attrValue
                            .Match(
                                primitive => new[] { primitive },
                                array => (IEnumerable<string>)array)));

    public override string ToString() => Cs();

    #region Private

    private string CsCtorArgs() => CtorArgs.Select(arg => arg.Cs()).JoinList();

    private string CsNamedArgs()
        => NamedArgs
           .Select(kv => $"{kv.Key.Cs()} = {kv.Value.Cs()}")
           .JoinList()
           .AddLeftIfNotEmpty(", ");

    private string CsTypeArgs() => TypeArgs.Count == 0 ? "" : $"<{TypeArgs.Select(arg => arg.Cs()).JoinList()}>";

    #endregion

    #region Equality Members

    public virtual bool Equals(TsAttr? other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Type.Equals(other.Type)
               && IsTsAttr == other.IsTsAttr
               && CtorArgs.Equals(other.CtorArgs)
               && NamedArgs.Equals(other.NamedArgs)
               && TypeArgs.Equals(other.TypeArgs);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = Type.GetHashCode();
            hashCode = (hashCode * 397) ^ IsTsAttr.GetHashCode();
            hashCode = (hashCode * 397) ^ CtorArgs.GetHashCode();
            hashCode = (hashCode * 397) ^ NamedArgs.GetHashCode();
            hashCode = (hashCode * 397) ^ TypeArgs.GetHashCode();
            return hashCode;
        }
    }

    #endregion
}
