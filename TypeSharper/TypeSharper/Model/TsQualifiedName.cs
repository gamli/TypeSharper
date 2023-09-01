using System;
using System.Collections.Generic;
using System.Linq;

namespace TypeSharper.Model;

public record TsQualifiedName(TsList<TsName> Parts) : IComparable<TsQualifiedName>
{
    public TsQualifiedName() : this(new TsList<TsName>()) { }
    public TsQualifiedName(params string[] ids) : this((IEnumerable<string>)ids) { }
    public TsQualifiedName(IEnumerable<string> ids) : this(ids.Select(id => new TsName(id))) { }

    public TsQualifiedName(params TsName[] ids) : this(TsList.Create(ids)) { }
    public TsQualifiedName(IEnumerable<TsName> ids) : this(TsList.Create(ids)) { }
    public static implicit operator TsQualifiedName(string csQualifiedId) => new(csQualifiedId.Split("."));
    public static implicit operator TsQualifiedName(TsName name) => new(name);
    public TsQualifiedName Add(TsName name) => new(Parts.Add(name));
    public TsQualifiedName Append(TsQualifiedName name) => new(Parts.Concat(name.Parts));

    public int CompareTo(TsQualifiedName other)
        => string.Compare(Cs(), other.Cs(), StringComparison.InvariantCultureIgnoreCase);

    public string Cs() => string.Join(".", Parts.Select(name => name.Cs()));
    public override string ToString() => Cs();

    #region Equality Members

    public virtual bool Equals(TsQualifiedName? other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Parts.Equals(other.Parts);
    }

    public override int GetHashCode() => Parts.GetHashCode();

    #endregion
}
