using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TypeSharper.Model.Modifier;

namespace TypeSharper.SyntaxExtensions;

public static class MemberDeclarationSyntaxExtensions
{
    public static bool IsCtor(this MemberDeclarationSyntax member) => member is ConstructorDeclarationSyntax;
    public static bool IsInternal(this MemberDeclarationSyntax member) => member.Modifiers.ContainsInternal();
    public static bool IsMethod(this MemberDeclarationSyntax member) => member is MethodDeclarationSyntax;

    public static bool IsPrivate(this MemberDeclarationSyntax member) => member.Modifiers.ContainsPrivate();

    public static bool IsProperty(this MemberDeclarationSyntax member) => member is PropertyDeclarationSyntax;

    public static bool IsProtected(this MemberDeclarationSyntax member) => member.Modifiers.ContainsProtected();

    public static bool IsPublic(this MemberDeclarationSyntax member) => member.Modifiers.ContainsPublic();

    public static bool IsStatic(this MemberDeclarationSyntax member) => member.Modifiers.ContainsStatic();

    public static ETsVisibility Visibility(this MemberDeclarationSyntax member) => member.Modifiers.Visibility();

    public static IEnumerable<ConstructorDeclarationSyntax> WhereIsCtor<TMemberDeclarationSyntax>(
        this IEnumerable<TMemberDeclarationSyntax> members)
    where TMemberDeclarationSyntax : MemberDeclarationSyntax
        => members.Where(IsCtor).Cast<ConstructorDeclarationSyntax>();

    public static IEnumerable<MethodDeclarationSyntax> WhereIsMethod<TMemberDeclarationSyntax>(
        this IEnumerable<TMemberDeclarationSyntax> members)
    where TMemberDeclarationSyntax : MemberDeclarationSyntax
        => members.Where(IsMethod).Cast<MethodDeclarationSyntax>();

    public static IEnumerable<PropertyDeclarationSyntax> WhereIsProperty<TMemberDeclarationSyntax>(
        this IEnumerable<TMemberDeclarationSyntax> members)
    where TMemberDeclarationSyntax : MemberDeclarationSyntax
        => members.Where(IsProperty).Cast<PropertyDeclarationSyntax>();

    public static IEnumerable<TMemberDeclarationSyntax> WhereIsPublic<TMemberDeclarationSyntax>(
        this IEnumerable<TMemberDeclarationSyntax> members)
    where TMemberDeclarationSyntax : MemberDeclarationSyntax
        => members.Where(IsPublic);
}
