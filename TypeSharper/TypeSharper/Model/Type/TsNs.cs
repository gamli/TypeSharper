using System;
using System.Linq;
using TypeSharper.Model.Identifier;

namespace TypeSharper.Model.Type;

public record TsNs
{
    public TsNs(string csNs) : this(new TsQualifiedId(csNs)) { }

    public TsNs(TsQualifiedId id)
    {
        if (id.Parts.Any(part => part.Cs() == "::global"))
        {
            throw new ArgumentOutOfRangeException(nameof(Id), "::global should never be passed as ID");
        }

        if (id.Parts.Any(part => string.IsNullOrEmpty(part.Cs())))
        {
            throw new ArgumentOutOfRangeException(nameof(Id), "namespace part should never be empty");
        }

        Id = id;
    }

    public TsQualifiedId Id { get; init; }

    public bool IsGlobal => !Id.Parts.Any() || Id.Parts.First() == "::global";
    public static implicit operator TsNs(string csNs) => new(csNs);
    public TsNs Add(TsId id) => new(Id.Add(id));

    public string CsFileScoped() => IsGlobal ? "" : $"namespace {Id.Cs()};";
    public string CsRef() => IsGlobal ? "" : Id.Cs();

    public override string ToString() => CsFileScoped();
}
