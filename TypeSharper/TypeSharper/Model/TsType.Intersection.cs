using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using TypeSharper.Diagnostics;

namespace TypeSharper.Model;

public abstract partial record TsType
{
    #region Nested types

    public sealed record Intersection(
            TypeInfo Info,
            TsUniqueList<TsTypeRef> TypesToIntersect,
            TsUniqueList<TsProp> Props)
        : PropertyDuck(Info, Props)
    {
        public override string ToString() => $"Intersection:{base.ToString()}";

        #region Protected

        protected override Maybe<string> CsBody(TsModel model)
            => $$"""
                {
                {{TypesToIntersect.Select(CsConstituentTypeCastAndCtor).JoinLines().Indent()}}
                }
                """;

        #endregion

        #region Private

        private string CsConstituentTypeCastAndCtor(TsTypeRef type)
        {
            var csTypeName = Info.Name.Cs();
            var csFromTypeName = type.Cs();
            var csCast =
                $$"""
                public static implicit operator {{Info.Name.Cs()}}({{type.Cs()}} from)
                    => new(from);
                """;
            var csCtor =
                Props.Any()
                    ? $$"""
                    public {{csTypeName}}({{csFromTypeName}} from)
                    : this({{Props.Select(prop => prop.CsGetFrom("from")).JoinList()}}) { }
                    """
                    : $$"""
                    public {{csTypeName}}({{csFromTypeName}} _) { }
                    """;
            return $$"""
                {{csCast}}
                {{csCtor}}
                """;
        }

        #endregion

        public static TsUniqueList<TsProp> PropsPresentInAllTypes(TsUniqueList<TsTypeRef> types, TsModel model)
        {
            var props =
                types
                    .Select(type => FromTypeProperties(type, model))
                    .ToList();
            return props
                   .First()
                   .Where(
                       candidateProp =>
                           props.All(ps => ps.Any(p => p.Name == candidateProp.Name)));
        }
    }

    public record IntersectionAttr(
            TsUniqueList<TsTypeRef> TypesToIntersect,
            TsList<TsPropMapping> PropMappings)
        : TsPropertyDuckAttr(PropMappings)
    {
        #region Protected

        protected override IEnumerable<TsName> PropNames(TsModel model)
            => Intersection.PropsPresentInAllTypes(TypesToIntersect, model).Select(prop => prop.Name);

        protected override Maybe<DiagnosticsError> DoDoRunDiagnostics(ITypeSymbol targetTypeSymbol, TsModel model)
        {
            var unsupportedTypes =
                TsUniqueList.Create(
                    TypesToIntersect
                        .SelectWhereSome(
                            typeRef
                                => model
                                   .Resolve(typeRef)
                                   .MapPropertyDuck(
                                       _ => Maybe<TsType>.NONE,
                                       taggedUnion => taggedUnion,
                                       _ => Maybe<TsType>.NONE)));
            if (unsupportedTypes.Any())
            {
                return new DiagnosticsError(
                    EDiagnosticsCode.IntersectionOfTaggedUnionsIsNotSupported,
                    targetTypeSymbol,
                    $$"""
                    Intersection type {0} tries to intersect tagged union types which is currently not supported.
                    The following types are union types:
                    {{unsupportedTypes.Select(type => $"* {type.Ref().Cs()}").JoinLines()}}
                    """,
                    targetTypeSymbol);
            }

            return Maybe<DiagnosticsError>.NONE;
        }

        #endregion
    }

    #endregion
}
