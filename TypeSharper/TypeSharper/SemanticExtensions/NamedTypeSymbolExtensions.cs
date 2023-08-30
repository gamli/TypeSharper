using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using TypeSharper.Model;
using TypeSharper.Model.Mod;

namespace TypeSharper.SemanticExtensions;

public static class NamedTypeSymbolExtensions
{
    public static bool HasTypeSharperAttribute(this INamedTypeSymbol namedTypeSymbol)
        => namedTypeSymbol.TypeSharperAttribute().Any();

    public static IEnumerable<AttributeData> TypeSharperAttribute(this INamedTypeSymbol namedTypeSymbol)
        => namedTypeSymbol
           .GetAttributes()
           .Where(
               attributeData
                   => attributeData.AttributeClass?.ContainingNamespace?.ToQualifiedId()
                      == (TsQualifiedName)TsAttr.NS);

    public static IEnumerable<ITypeSymbol> ContainingTypeHierarchy(this INamedTypeSymbol namedTypeSymbol)
        => namedTypeSymbol.ContainingType == null
            ? new List<ITypeSymbol>()
            : new[] { namedTypeSymbol.ContainingType }.Concat(namedTypeSymbol.ContainingType.ContainingTypeHierarchy());

    public static TsType ToType(this INamedTypeSymbol namedTypeSymbol)
    {
        var typeInfo =
            new TsType.TypeInfo(
                namedTypeSymbol.ToName(),
                namedTypeSymbol.ContainingType?.ToTypeRef() ?? Maybe<TsTypeRef>.NONE,
                namedTypeSymbol.ContainingNamespace.ToNs(),
                namedTypeSymbol.TypeKind switch
                {
                    TypeKind.Interface => TsType.EKind.Interface,
                    TypeKind.Class when !namedTypeSymbol.IsRecord => TsType.EKind.Class,
                    TypeKind.Class when namedTypeSymbol.IsRecord => TsType.EKind.RecordClass,
                    TypeKind.Struct when !namedTypeSymbol.IsRecord => TsType.EKind.Struct,
                    TypeKind.Struct when namedTypeSymbol.IsRecord => TsType.EKind.RecordStruct,
                    _ => throw new ArgumentOutOfRangeException(nameof(namedTypeSymbol), namedTypeSymbol, null),
                },
                namedTypeSymbol.ToTypeMods());

        return TsTypeFactory.CreateNative(
            typeInfo,
            namedTypeSymbol.SpecialType == SpecialType.None
                ? namedTypeSymbol
                  .GetMembers()
                  .OfType<IPropertySymbol>()
                  .Where(propertySymbol => !propertySymbol.ShouldBeIgnored())
                  .Select(propertySymbol => propertySymbol.ToProp())
                : Enumerable.Empty<TsProp>());
    }

    public static TsTypeMods ToTypeMods(this INamedTypeSymbol namedTypeSymbol)
        => new(
            namedTypeSymbol.DeclaredAccessibility.ToVisibility(),
            new TsAbstractMod(namedTypeSymbol.IsAbstract && namedTypeSymbol.TypeKind != TypeKind.Interface),
            new TsStaticMod(namedTypeSymbol.IsStatic),
            new TsSealedMod(namedTypeSymbol.IsSealed),
            new TsPartialMod(namedTypeSymbol.IsPartial()));

    public static TsTypeRef ToTypeRef(this INamedTypeSymbol namedTypeSymbol)
    {
        var idParts =
            TsList.Create(
                namedTypeSymbol
                    .ContainingTypeHierarchy()
                    .Select(ts => ts.ToName())
                    .Reverse()
                    .Append(namedTypeSymbol.ToName()));

        return TsTypeRef.WithNs(namedTypeSymbol.ContainingNamespace.ToNsRef(), new TsQualifiedName(idParts));
    }
}
