using System.Collections.Generic;
using System.Linq;

namespace TypeSharper.Model.Identifier;

public record TsQualifiedId(TsList<TsId> Parts)
{
    public TsQualifiedId(params string[] ids) : this((IEnumerable<string>)ids) { }
    public TsQualifiedId(IEnumerable<string> ids) : this(ids.Select(id => new TsId(id))) { }

    public TsQualifiedId(params TsId[] ids) : this(TsList.Create(ids)) { }
    public TsQualifiedId(IEnumerable<TsId> ids) : this(TsList.Create(ids)) { }
    public TsQualifiedId Add(TsId id) => new(Parts.Add(id));

    public string Cs() => string.Join(".", Parts.Select(name => name.Cs()));
    public override string ToString() => Cs();

    #region Equality Members

    public virtual bool Equals(TsQualifiedId? other)
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
