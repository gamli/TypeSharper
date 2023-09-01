using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using TypeSharper.Diagnostics;

namespace TypeSharper.Model;

public abstract partial record TsType
{
    #region Nested types

    public abstract record PropertySelection(TypeInfo Info, TsTypeRef FromType, TsUniqueList<TsProp> Props)
        : PropertyDuck(Info, Props)
    {
        #region Protected

        protected override Maybe<string> CsBody(TsModel model)
            => $$"""
                {
                {{CsCastsAndCtors(FromType, this, model).Indent()}}
                }
                """;

        #endregion
        
        #region Private

        private static string CsCastsAndCtors(TsTypeRef fromTypeRef, PropertyDuck selfType, TsModel model)
            => $$"""
                public static implicit operator {{selfType.Info.Name.Cs()}}({{fromTypeRef.Cs()}} from)
                    => new(from);
                public {{selfType.Info.Name.Cs()}}({{fromTypeRef.Cs()}} from)
                : this({{selfType.Props.Select(prop => prop.CsGetFrom("from")).JoinList()}}) { }
                {{model.Resolve(fromTypeRef)
                       .IfPropertySelection(propertySelection => CsCastsAndCtors(propertySelection.FromType, selfType, model))
                       .MapSomeOr("")}}
                """;

        #endregion
    }

    #endregion


    public record TsPropertySelectionAttr(
            TsTypeRef FromType,
            TsUniqueList<TsName> SelectedProperties,
            TsList<TsPropMapping> PropMappings)
        : TsPropertyDuckAttr(PropMappings)
    {
        protected override Maybe<DiagnosticsError> DoDoRunDiagnostics(ITypeSymbol targetTypeSymbol, TsModel model)
            => RunAllSelectedPropertiesMustExistDiagnostic(targetTypeSymbol, model);

        protected override IEnumerable<TsName> PropNames(TsModel model)
            => FromTypeProperties(FromType, model).Select(prop => prop.Name);

        private Maybe<DiagnosticsError> RunAllSelectedPropertiesMustExistDiagnostic(ISymbol targetTypeSymbol, TsModel model)
        {
            var fromTypeProps = TsHashSet.Create(PropNames(model));
            return SelectedProperties.Any(prop => !fromTypeProps.Contains(prop))
                ? new DiagnosticsError(
                    EDiagnosticsCode.SelectedPropertyDoesNotExist,
                    targetTypeSymbol,
                    $$"""
                    Type {0} does not contain all properties selected by {1}.
                    The missing properties are:
                    {{SelectedProperties
                      .Where(prop => !fromTypeProps.Contains(prop))
                      .Select(prop => $"* {prop.Cs()}")
                      .JoinLines()}}
                    """,
                    FromType.Cs(),
                    targetTypeSymbol)
                : Maybe<DiagnosticsError>.NONE;
        }
    }
}
