using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using TypeSharper.Model.Attr;
using TypeSharper.Model.Identifier;
using TypeSharper.Model.Member;
using TypeSharper.Model.Modifier;
using TypeSharper.Model.Type;
using TypeSharper.Support;

namespace TypeSharper.SemanticExtensions;

public static class NamedTypeSymbolExtensions
{
    public static Maybe<TsTypeRef> BaseType(this INamedTypeSymbol namedTypeSymbol)
        => namedTypeSymbol.BaseType == null
            ? Maybe<TsTypeRef>.NONE
            : Maybe<TsTypeRef>.Some(namedTypeSymbol.BaseType.ToTypeRef());

    public static Maybe<TsTypeRef> ContainingType(this INamedTypeSymbol namedTypeSymbol)
        => namedTypeSymbol.ContainingType == null
            ? Maybe<TsTypeRef>.NONE
            : Maybe<TsTypeRef>.Some(namedTypeSymbol.ContainingType.ToTypeRef());

    public static IEnumerable<ITypeSymbol> ContainingTypeHierarchy(this INamedTypeSymbol namedTypeSymbol)
        => namedTypeSymbol.ContainingType == null
            ? new List<ITypeSymbol>()
            : new[] { namedTypeSymbol.ContainingType }.Concat(namedTypeSymbol.ContainingType.ContainingTypeHierarchy());

    public static TsType ToType(this INamedTypeSymbol namedTypeSymbol)
    {
        var name = namedTypeSymbol.ToId();

        if (namedTypeSymbol.SpecialType != SpecialType.None)
        {
            return new TsType(
                name,
                Maybe<TsTypeRef>.NONE,
                Maybe<TsTypeRef>.NONE,
                namedTypeSymbol.ContainingNamespace.ToNs(),
                TsType.EKind.Special,
                new TsTypeMods(
                    ETsVisibility.Public,
                    new TsAbstractMod(false),
                    new TsStaticMod(false),
                    new TsSealedMod(true),
                    new TsPartialMod(false),
                    new TsTargetTypeMod(false)),
                new TsList<TsCtor>(),
                new TsList<TsProp>(),
                new TsList<TsMethod>(),
                new TsList<TsAttr>());
        }

        var typeKind =
            namedTypeSymbol.TypeKind switch
            {
                TypeKind.Interface => TsType.EKind.Interface,
                TypeKind.Class when !namedTypeSymbol.IsRecord => TsType.EKind.Class,
                TypeKind.Class when namedTypeSymbol.IsRecord => TsType.EKind.Record,
                _ => throw new ArgumentOutOfRangeException(nameof(namedTypeSymbol), namedTypeSymbol, null),
            };

        return new TsType(
            name,
            namedTypeSymbol.BaseType(),
            namedTypeSymbol.ContainingType(),
            namedTypeSymbol.ContainingNamespace.ToNs(),
            typeKind,
            namedTypeSymbol.ToTypeMods(),
            TsList.Create(
                namedTypeSymbol
                    .InstanceConstructors
                    .Select(ctorSymbol => ctorSymbol.ToCtor())),
            TsList.Create(
                namedTypeSymbol
                    .GetMembers()
                    .OfType<IPropertySymbol>()
                    .Select(propertySymbol => propertySymbol.ToProp())),
            TsList.Create(
                namedTypeSymbol
                    .GetMembers()
                    .OfType<IMethodSymbol>()
                    .Where(methodSymbol => methodSymbol.MethodKind == MethodKind.Ordinary)
                    .Select(methodSymbol => methodSymbol.ToMethod())),
            TsList.Create(
                namedTypeSymbol
                    .GetAttributes()
                    .Select(attributeData => attributeData.ToAttr())));
    }

    public static TsTypeMods ToTypeMods(this INamedTypeSymbol namedTypeSymbol)
        => new(
            namedTypeSymbol.DeclaredAccessibility.ToVisibility(),
            new TsAbstractMod(namedTypeSymbol.IsAbstract && namedTypeSymbol.TypeKind != TypeKind.Interface),
            new TsStaticMod(namedTypeSymbol.IsStatic),
            new TsSealedMod(namedTypeSymbol.IsSealed),
            new TsPartialMod(namedTypeSymbol.IsPartial()),
            new TsTargetTypeMod(namedTypeSymbol.TsAttributes().Any()));

    public static TsTypeRef ToTypeRef(this INamedTypeSymbol namedTypeSymbol)
        => new(
            namedTypeSymbol.ContainingNamespace.ToNs(),
            new TsQualifiedId(
                TsList.Create(
                    namedTypeSymbol
                        .ContainingTypeHierarchy()
                        .Select(ts => ts.ToId())
                        .Reverse()
                        .Append(namedTypeSymbol.ToId()))));
}
