using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TypeSharper.Model.Member;
using TypeSharper.Model.Modifier;

namespace TypeSharper.SemanticExtensions;

public static class MethodSymbolExtensions
{
    public static Maybe<string> CsBody(this IMethodSymbol methodSymbol)
    {
        var csBody =
            (methodSymbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() as MethodDeclarationSyntax)
            ?.Body
            ?.GetText()
            .ToString();
        return csBody == null ? Maybe<string>.NONE : csBody;
    }

    public static TsCtor ToCtor(this IMethodSymbol methodSymbol)
        => new(
            methodSymbol.Parameters.ToParams(),
            methodSymbol.ToMemberMods(),
            methodSymbol.CsBody());

    public static TsMemberMods ToMemberMods(this IMethodSymbol methodSymbol)
        => new(
            methodSymbol.DeclaredAccessibility.ToVisibility(),
            new TsAbstractMod(methodSymbol.IsAbstract),
            new TsStaticMod(methodSymbol.IsStatic),
            ETsOperator.None);

    public static TsMethod ToMethod(this IMethodSymbol methodSymbol)
        => new(
            methodSymbol.ToId(),
            methodSymbol.ReturnType.ToTypeRef(),
            TsList.Create(methodSymbol.TypeParameters.Select(typeParam => typeParam.ToTypeRef())),
            methodSymbol.Parameters.ToParams(),
            methodSymbol.ToMemberMods(),
            methodSymbol.CsBody());

    public static TsPrimaryCtor ToPrimaryCtor(this IMethodSymbol methodSymbol)
        => TsPrimaryCtor.Create(methodSymbol.Parameters.ToParams()).AssertSome();
}
