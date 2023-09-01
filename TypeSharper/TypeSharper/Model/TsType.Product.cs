using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using TypeSharper.Diagnostics;

namespace TypeSharper.Model;

public abstract partial record TsType
{
    #region Nested types

    public record Product(
            TypeInfo Info,
            TsUniqueList<TsProp> Props,
            TsUniqueList<TsTypeRef> TypesToMultiply)
        : PropertyDuck(Info, Props)
    {
        public static IEnumerable<(TsType type, TsProp prop)> FromTypesProps(IEnumerable<TsType> fromTypes)
            => fromTypes
               .Select(
                   type => type.MapPropertyDuck(
                       propertyDuck => new { type, propertyDuck.Props },
                       _ => null!,
                       native => new { type, native.Props }))
               .Where(typeProps => typeProps is not null)
               .SelectMany(
                   typeProps
                       => typeProps.Props.Select(prop => (typeProps.type, prop)));

        public override string ToString() => $"Product:{base.ToString()}";

        #region Protected

        protected override Maybe<string> CsBody(TsModel model)
            => $$"""
                {
                {{CsConstituentTypesCtor(model).Indent()}}
                }
                """;

        #endregion

        #region Private

        private string CsConstituentTypesCtor(TsModel model)
        {
            var ctorParams =
                TypesToMultiply
                    .Select(type => type.Cs())
                    .Select(typeName => $"{typeName} from{typeName.Replace(".", "_")}");
            var csBaseCtorCallArgs =
                FromTypesProps(TypesToMultiply.Select(model.Resolve))
                    .Select(t => t.prop.CsGetFrom($"from{t.type.Ref().Cs().Replace(".", "_")}"));
            return Props.Any()
                ? $$"""
                public {{Info.Name.Cs()}}({{ctorParams.JoinList()}})
                : this({{csBaseCtorCallArgs.JoinList()}}) { }
                """
                : $$"""
                public {{Info.Name.Cs()}}({{ctorParams.JoinList()}}) { }
                """;
        }

        #endregion
    }

    public record ProductAttr(TsUniqueList<TsTypeRef> TypesToMultiply, TsList<TsPropMapping> PropMappings)
        : TsPropertyDuckAttr(PropMappings)
    {
        #region Protected

        protected override Maybe<DiagnosticsError> DoDoRunDiagnostics(ITypeSymbol targetTypeSymbol, TsModel model)
        {
            var unsupportedTypes =
                TypesToMultiply
                    .Where(
                        typeRef => model
                                   .Resolve(typeRef)
                                   .MapPropertyDuck(
                                       _ => false,
                                       _ => true,
                                       _ => false));

            if (unsupportedTypes.Any())
            {
                return new DiagnosticsError(
                    EDiagnosticsCode.ProductOfTaggedUnionsIsNotSupported,
                    targetTypeSymbol,
                    "A product of tagged unions is not supported. Currently only property containing types like"
                    + " native, omit, pick and intersection types are supported."
                    + " The following selected types are tagged unions:\n"
                    + unsupportedTypes.Select(type => $"- {type.Cs()}").JoinLines(),
                    targetTypeSymbol);
            }

            return Maybe<DiagnosticsError>.NONE;
        }

        protected override IEnumerable<TsName> PropNames(TsModel model)
            => TypesToMultiply
               .SelectMany(
                   type => model
                           .Resolve(type)
                           .MapPropertyDuck(
                               propertyDuck => propertyDuck.Props,
                               _ => TsUniqueList.Create<TsProp>(),
                               native => native.Props))
               .Select(prop => prop.Name);

        #endregion
    }

    #endregion
}
