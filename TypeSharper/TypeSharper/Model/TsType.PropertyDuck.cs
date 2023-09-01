using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using TypeSharper.Diagnostics;

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
    public record TsPropMapping(TsName PropName, TsTypeRef MappedPropType);

    public abstract record TsPropertyDuckAttr(TsList<TsPropMapping> PropMappings) : TsAttr
    {
        public TsUniqueList<TsProp> MapProps(TsUniqueList<TsProp> props)
        {
            var propMappingsDict = PropMappings.ToDictionary(mapping => mapping.PropName);
            return props.Select(
                prop => propMappingsDict.TryGetValue(prop.Name, out var mapping)
                    ? prop with { Type = mapping.MappedPropType }
                    : prop);
        }
        
        protected sealed override Maybe<DiagnosticsError> DoRunDiagnostics(ITypeSymbol targetTypeSymbol, TsModel model)
            => RunAllMappedPropertiesMustExistDiagnostic(targetTypeSymbol, model)
                .IfNone(() => DoDoRunDiagnostics(targetTypeSymbol, model));


        private Maybe<DiagnosticsError> RunAllMappedPropertiesMustExistDiagnostic(
            ISymbol targetTypeSymbol,
            TsModel model)
        {
            var propNames = TsHashSet.Create(PropNames(model));
            return PropMappings.Any(propMapping => !propNames.Contains(propMapping.PropName))
                ? new DiagnosticsError(
                    EDiagnosticsCode.MappedPropertyDoesNotExist,
                    targetTypeSymbol,
                    $$"""
                    Generated type does not contain all properties mapped by {0}.
                    Mappings with missing properties are:
                    {{PropMappings
                      .Where(propMapping => !propNames.Contains(propMapping.PropName))
                      .Select(propMapping => $"* {propMapping.PropName.Cs()} => {propMapping.MappedPropType.Cs()}")
                      .JoinLines()}}
                    The list of all mapped properties is:
                    {{PropMappings
                      .Select(propMapping => $"* {propMapping.PropName.Cs()} => {propMapping.MappedPropType.Cs()}")
                      .JoinLines()}}
                    """,
                    targetTypeSymbol)
                : Maybe<DiagnosticsError>.NONE;
        }

        protected abstract IEnumerable<TsName> PropNames(TsModel model);
        protected abstract Maybe<DiagnosticsError> DoDoRunDiagnostics(ITypeSymbol targetTypeSymbol, TsModel model);
    }
}
