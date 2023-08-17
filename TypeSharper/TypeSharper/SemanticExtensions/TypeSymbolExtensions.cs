using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TypeSharper.Model.Modifier;
using TypeSharper.Model.Type;
using TypeSharper.SyntaxExtensions;

namespace TypeSharper.SemanticExtensions;

public static class TypeSymbolExtensions
{
    public static IEnumerable<TypeDeclarationSyntax> Declarations(this ITypeSymbol typeSymbol)
        => typeSymbol
           .DeclaringSyntaxReferences
           .Select(declRef => (TypeDeclarationSyntax)declRef.GetSyntax());

    public static bool IsAbstract(this ITypeSymbol typeSymbol) => typeSymbol.Declarations().Any(s => s.IsAbstract());

    public static bool IsPartial(this ITypeSymbol typeSymbol) => typeSymbol.Declarations().Any(s => s.IsPartial());

    public static bool IsRecord(this ITypeSymbol typeSymbol) => typeSymbol.Declarations().Any(s => s.IsRecord());

    public static bool IsSealed(this ITypeSymbol typeSymbol) => typeSymbol.Declarations().Any(s => s.IsSealed());

    public static bool IsStatic(this ITypeSymbol typeSymbol) => typeSymbol.Declarations().Any(s => s.IsStatic());

    public static TsTypeRef ToTypeRef(this ITypeSymbol typeSymbol)
        => typeSymbol switch
        {
            IArrayTypeSymbol arrayTypeSymbol
                => arrayTypeSymbol.ElementType.ToTypeRef().WithArrayMod(new TsArrayMod(true)),
            IDynamicTypeSymbol dynamicTypeSymbol => throw new NotImplementedException(),
            IErrorTypeSymbol errorTypeSymbol => throw new NotImplementedException(),
            IFunctionPointerTypeSymbol functionPointerTypeSymbol => throw new NotImplementedException(),
            INamedTypeSymbol namedTypeSymbol => namedTypeSymbol.ToTypeRef(),
            IPointerTypeSymbol pointerTypeSymbol => throw new NotImplementedException(),
            ITypeParameterSymbol typeParameterSymbol => throw new NotImplementedException(),
            _ => throw new ArgumentOutOfRangeException(nameof(typeSymbol)),
        };
}
