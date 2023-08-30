using System.Linq;

namespace TypeSharper.Model;

public abstract partial record TsType
{
    public abstract record PropertyDuck(TypeInfo Info, TsUniqueList<TsProp> Props) : Duck(Info)
    {
        public override string Cs(TsModel model)
            => Info.Cs(
                $"({Props.Select(prop => prop.CsPrimaryCtor()).JoinList()})"
                + CsBody(model).Map(csBody => $"\n{csBody}", () => ";"),
                model);

        protected abstract Maybe<string> CsBody(TsModel model);
    }
}
