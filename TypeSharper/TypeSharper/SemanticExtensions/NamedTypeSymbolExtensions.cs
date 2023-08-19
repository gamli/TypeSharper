using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using TypeSharper.Model;
using TypeSharper.Model.Identifier;
using TypeSharper.Model.Member;
using TypeSharper.Model.Modifier;
using TypeSharper.Model.Type;

namespace TypeSharper.SemanticExtensions;

public static class NamedTypeSymbolExtensions
{
    public static Maybe<TsTypeRef> BaseType(this INamedTypeSymbol namedTypeSymbol)
        => namedTypeSymbol.BaseType == null
            ? Maybe<TsTypeRef>.NONE
            : namedTypeSymbol.BaseType.ToTypeRef();

    public static Maybe<TsTypeRef> ContainingType(this INamedTypeSymbol namedTypeSymbol)
        => namedTypeSymbol.ContainingType == null
            ? Maybe<TsTypeRef>.NONE
            : namedTypeSymbol.ContainingType.ToTypeRef();

    public static IEnumerable<ITypeSymbol> ContainingTypeHierarchy(this INamedTypeSymbol namedTypeSymbol)
        => namedTypeSymbol.ContainingType == null
            ? new List<ITypeSymbol>()
            : new[] { namedTypeSymbol.ContainingType }.Concat(namedTypeSymbol.ContainingType.ContainingTypeHierarchy());

    public static TsType ToType(this INamedTypeSymbol namedTypeSymbol)
    {
        if (namedTypeSymbol.Kind == SymbolKind.ErrorType)
        {
            throw new TsModelCreationSymbolErrorException(namedTypeSymbol);
        }

        var name = namedTypeSymbol.ToId();

        if (namedTypeSymbol.SpecialType != SpecialType.None)
        {
            return TsType.CreateNative(
                new TsType.TypeInfo(name, Maybe<TsTypeRef>.NONE, namedTypeSymbol.ContainingNamespace.ToNs()),
                Maybe<TsTypeRef>.NONE,
                TsType.EKind.Special,
                new TsTypeMods(
                    ETsVisibility.Public,
                    new TsAbstractMod(false),
                    new TsStaticMod(false),
                    new TsSealedMod(true),
                    new TsPartialMod(false),
                    new TsTargetTypeMod(false)));
        }

        var typeKind =
            namedTypeSymbol.TypeKind switch
            {
                TypeKind.Interface => TsType.EKind.Interface,
                TypeKind.Class when !namedTypeSymbol.IsRecord => TsType.EKind.Class,
                TypeKind.Class when namedTypeSymbol.IsRecord => TsType.EKind.RecordClass,
                TypeKind.Struct when !namedTypeSymbol.IsRecord => TsType.EKind.Struct,
                TypeKind.Struct when namedTypeSymbol.IsRecord => TsType.EKind.RecordStruct,
                _ => throw new ArgumentOutOfRangeException(nameof(namedTypeSymbol), namedTypeSymbol, null),
            };

        var (primaryCtor, primaryCtorProps) =
            namedTypeSymbol
                .InstanceConstructors
                .SingleOrDefault(ctor => ctor.IsPrimaryCtor())
                ?.ToPrimaryCtor()
            ?? (Maybe<TsPrimaryCtor>.NONE, TsList<TsProp>.Empty);

        return TsType
               .CreateNative(
                   new TsType.TypeInfo(
                       name,
                       namedTypeSymbol.ContainingType(),
                       namedTypeSymbol.ContainingNamespace.ToNs()),
                   namedTypeSymbol.BaseType(),
                   typeKind,
                   namedTypeSymbol.ToTypeMods())
               .SetPrimaryCtor(primaryCtor)
               .AddMembers(
                   TsList.Create(
                       namedTypeSymbol
                           .InstanceConstructors
                           .Where(ctorSymbol => !ctorSymbol.ShouldBeIgnored())
                           .Select(ctorSymbol => ctorSymbol.ToCtor())),
                   TsList.Create(
                       namedTypeSymbol
                           .GetMembers()
                           .OfType<IPropertySymbol>()
                           .Where(propertySymbol => !propertySymbol.ShouldBeIgnored())
                           .Select(propertySymbol => propertySymbol.ToProp())),
                   TsList.Create(
                       namedTypeSymbol
                           .GetMembers()
                           .OfType<IMethodSymbol>()
                           .Where(
                               methodSymbol => methodSymbol.MethodKind == MethodKind.Ordinary
                                               && !methodSymbol.ShouldBeIgnored())
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
    {
        var idParts =
            TsList.Create(
                namedTypeSymbol
                    .ContainingTypeHierarchy()
                    .Select(ts => ts.ToId())
                    .Reverse()
                    .Append(namedTypeSymbol.ToId()));

        return TsTypeRef.WithNs(namedTypeSymbol.ContainingNamespace.ToNsRef(), new TsQualifiedId(idParts));
    }
}
