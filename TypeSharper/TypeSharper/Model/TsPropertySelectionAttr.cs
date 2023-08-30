using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using TypeSharper.Diagnostics;

namespace TypeSharper.Model;

public record TsPropertySelectionAttr(TsTypeRef FromType, TsUniqueList<TsName> SelectedProperties) : TsAttr
{
    protected override Maybe<DiagnosticsError> DoRunDiagnostics(ITypeSymbol targetTypeSymbol, TsModel model)
    {
        var fromTypeProps =
            TsHashSet.Create(
                (IEnumerable<TsName>)TsTypeFactory.FromTypeProperties(FromType, model).Select(prop => prop.Name));
        return SelectedProperties.Any(prop => !fromTypeProps.Contains(prop))
            ? new DiagnosticsError(
                EDiagnosticsCode.PropertyDoesNotExist,
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
