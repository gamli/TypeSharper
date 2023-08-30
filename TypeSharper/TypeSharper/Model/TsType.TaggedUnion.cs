using System.Linq;

namespace TypeSharper.Model;

public abstract partial record TsType
{

    public record TaggedUnionAttr(TsUniqueList<TsName> CaseNames, TsList<TsTypeRef> CaseTypes) : TsAttr;
    #region Nested types

    public sealed record TaggedUnion(TypeInfo Info, TsUniqueList<TaggedUnion.Case> Cases) : Duck(Info)
    {
        public override string Cs(TsModel model)
            => Info.Cs(
                // language=C#
                $$"""
                {
                    private {{Info.Name.Cs()}}() { }
                {{CsMatchMethods().Indent()}}
                {{Cases.Select(c => c.CsIfMethods() + "\n").JoinLines().Indent()}}
                {{Cases.Select(c => c.CsType(Info) + "\n").JoinLines().Indent()}}
                }
                """,
                model);

        public override string ToString() => $"TaggedUnion:{base.ToString()}";

        private string CsMatchMethods()
            => $$"""
                {{CsMatchMethod(true)}}
                {{CsMatchMethod(false)}}
                """;

        private string CsMatchMethod(bool handlerHasReturnValue)
        {
            var returnType = handlerHasReturnValue ? "TReturn" : "void";
            var genericParameter = handlerHasReturnValue ? "<TReturn>" : "";
            var parameters = Cases.Select(c => c.CsMatchMethodParam(handlerHasReturnValue)).JoinList();
            var body = CsMatchMethodBody(handlerHasReturnValue);
            return $$"""
                public {{returnType}} Map{{genericParameter}}({{parameters}})
                {{body}}
                """;
        }

        private string CsMatchMethodBody(bool handlerHasReturnValue)
        {
            var switchCases =
                Cases
                    .Select(c => c.CsMatchMethodSwitchCase(handlerHasReturnValue))
                    .JoinLines()
                    .Indent()
                    .Indent();

            return handlerHasReturnValue
                ? // language=C#
                $$"""
                => this switch
                   {
                {{switchCases}}
                   };
                """
                : // language=C#
                $$"""
                {
                    switch(this)
                    {
                {{switchCases}}
                    }
                }
                """;
        }

        #region Nested types

        public record Case(TsName Name, Maybe<TsTypeRef> ValueType)
        {
            public string CsMatchMethodSwitchCase(bool handlerHasReturnValue)
                => ValueType.Map(
                    _ => handlerHasReturnValue
                        ? // language=C#
                        $"{Name} c => handle{Name}(c.Value),"
                        : // language=C#
                        $$"""
                        case {{Name}} c:
                            handle{{Name}}(c.Value);
                            break;
                        """,
                    // language=C#
                    () => handlerHasReturnValue
                        ? // language=C#
                        $"{Name} => handle{Name}(),"
                        : // language=C#
                        $$"""
                        case {{Name}}:
                            handle{{Name}}();
                            break;
                        """);

            public string CsType(TypeInfo unionTypeInfo)
                => ValueType.Map(
                    // language=C#
                    typeRef => $"public sealed record {Name}({typeRef.Cs()} Value) : {unionTypeInfo.Name.Cs()};",
                    // language=C#
                    () => $"public sealed record {Name} : {unionTypeInfo.Name.Cs()};");

            public string CsIfMethods()
                => $$"""
                    {{CsIfMethod(true)}}
                    {{CsIfMethod(false)}}
                    """;

            public TsTypeRef TypeRef() => TsTypeRef.WithoutNs(Name);

            #region Private

            public string CsMatchMethodParam(bool handlerHasReturnValue)
                => ValueType.Map(
                    typeRef => handlerHasReturnValue
                        ? // language=C#
                        $"System.Func<{typeRef.Cs()}, TReturn> handle{Name}"
                        : // language=C#
                        $"System.Action<{typeRef.Cs()}> handle{Name}",
                    // language=C#
                    () => handlerHasReturnValue
                        ? // language=C#
                        $"System.Func<TReturn> handle{Name}"
                        : // language=C#
                        $"System.Action handle{Name}");

            private string CsIfMethod(bool handlerHasReturnValue)
                => ValueType.Map(
                    _ => handlerHasReturnValue
                        ? // language=C#
                        $$"""
                        public Maybe<TReturn> If{{Name}}<TReturn>({{CsMatchMethodParam(handlerHasReturnValue)}})
                            => this is {{Name}} c ? handle{{Name}}(c.Value) : Maybe<TReturn>.NONE;
                        """
                        : // language=C#
                        $$"""
                        public Maybe<Void> If{{Name}}({{CsMatchMethodParam(handlerHasReturnValue)}})
                        {
                            if(this is {{Name}} c)
                            {
                                handle{{Name}}(c.Value);
                                return Void.Instance;
                            }
                            return Maybe<Void>.NONE;
                        }
                        """,
                    // language=C#
                    () => handlerHasReturnValue
                        ? // language=C#
                        $$"""
                        public Maybe<TReturn> If{{Name}}<TReturn>({{CsMatchMethodParam(handlerHasReturnValue)}})
                            => this is {{Name}} ? handle{{Name}}() : Maybe<TReturn>.NONE;
                        """
                        : // language=C#
                        $$"""
                        public Maybe<Void> If{{Name}}({{CsMatchMethodParam(handlerHasReturnValue)}})
                        {
                            if(this is {{Name}} c)
                            {
                                handle{{Name}}(c.Value);
                                return Void.Instance;
                            }
                            return Maybe<Void>.NONE;
                        }
                        """);

            #endregion
        }

        #endregion
    }

    #endregion
}
