using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TypeSharper.Model.Member;
using TypeSharper.Model.Modifier;
using TypeSharper.Support;

namespace TypeSharper.SemanticExtensions;

public static class MethodSymbolExtensions
{
    public static Maybe<string> CsBody(this IMethodSymbol methodSymbol)
    {
        var csBody =
            ((MethodDeclarationSyntax?)methodSymbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax())
            ?.Body
            ?.GetText()
            .ToString();
        return csBody == null ? Maybe<string>.NONE : Maybe<string>.Some(csBody);
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
            new TsStaticMod(methodSymbol.IsStatic));

    public static TsMethod ToMethod(this IMethodSymbol methodSymbol)
        => new(
            methodSymbol.ToId(),
            methodSymbol.ReturnType.ToTypeRef(),
            methodSymbol.Parameters.ToParams(),
            methodSymbol.ToMemberMods(),
            methodSymbol.CsBody());
}
