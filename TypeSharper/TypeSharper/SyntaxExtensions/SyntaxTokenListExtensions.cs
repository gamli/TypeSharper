using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using TypeSharper.Model.Mod;

namespace TypeSharper.SyntaxExtensions;

public static class SyntaxTokenListExtensions
{
    public static bool ContainsInternal(this SyntaxTokenList tokenList) => tokenList.Any(SyntaxKind.InternalKeyword);

    public static bool ContainsPrivate(this SyntaxTokenList tokenList) => tokenList.Any(SyntaxKind.PrivateKeyword);

    public static bool ContainsProtected(this SyntaxTokenList tokenList) => tokenList.Any(SyntaxKind.ProtectedKeyword);

    public static bool ContainsPublic(this SyntaxTokenList tokenList) => tokenList.Any(SyntaxKind.PublicKeyword);

    public static bool ContainsSealed(this SyntaxTokenList tokenList) => tokenList.Any(SyntaxKind.SealedKeyword);
    public static bool ContainsStatic(this SyntaxTokenList tokenList) => tokenList.Any(SyntaxKind.StaticKeyword);

    public static ETsVisibility Visibility(this SyntaxTokenList tokenList)
        => tokenList.ContainsPrivate()    ? ETsVisibility.Private :
            tokenList.ContainsProtected() ? ETsVisibility.Protected :
            tokenList.ContainsInternal()  ? ETsVisibility.Internal :
                                            ETsVisibility.Public;
}
