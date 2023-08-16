# SemanticExtensions Directory

This directory contains extension methods that augment the functionalities provided by Roslyn's semantic models. The
extensions aid in converting Roslyn's semantic symbols into models more suitable for TypeSharper's requirements.

## AccessibilityExtensions.cs

Contains extension methods for the `Accessibility` enum, facilitating its conversion to the custom `ETsVisibility` enum.
These methods enable the translation of Roslyn's accessibility representation to TypeSharper's visibility model.

## TypeSymbolExtensions.cs

Offers extension methods for the `ITypeSymbol` interface. It provides methods to retrieve type declarations and check
various attributes of a type, such as if it's abstract, partial, record, sealed, or static.

## ParameterSymbolExtensions.cs

Introduces extension methods for the `IParameterSymbol` interface. These methods assist in converting Roslyn's parameter
symbol to TypeSharper's parameter model.

## MethodSymbolExtensions.cs

Provides extension methods for the `IMethodSymbol` interface. These methods are essential to fetch the C# body of a
method, convert the method symbol to TypeSharper's constructor model, and derive various member modifiers from the
method symbol.

