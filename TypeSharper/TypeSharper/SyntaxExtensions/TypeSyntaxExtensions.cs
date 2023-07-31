// using System;
// using System.Linq;
// using Microsoft.CodeAnalysis;
// using Microsoft.CodeAnalysis.CSharp;
// using Microsoft.CodeAnalysis.CSharp.Syntax;
// using TypeSharper.Model.Identifier;
// using TypeSharper.Model.Type;
//
// namespace TypeSharper.SyntaxExtensions;
//
// public static class TypeSyntaxExtensions
// {
//     public static TsTypeRef ToTypeRef(this TypeSyntax typeSyntax) => new (typeSyntax.ToNs(), typeSyntax.ToDottedId());
//     public static TsNs ToNs(this TypeSyntax typeSyntax) => TsNs.Dotted(typeSyntax.);
//     public static TsDottedId ToDottedId(this TypeSyntax typeSyntax) => new TsTypeRef();
// }



