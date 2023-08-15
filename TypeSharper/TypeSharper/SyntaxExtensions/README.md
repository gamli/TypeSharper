
# SyntaxExtensions Directory

This directory contains extension methods that supplement the functionalities provided by Roslyn's syntax models. These extensions assist in extracting and converting syntax-related information into models that better fit TypeSharper's requirements.

## TypeSyntaxExtensions.cs
Though currently commented out, this file is intended to offer extension methods to convert `TypeSyntax` into TypeSharper models.

## PropertyDeclarationSyntaxExtensions.cs
Houses extension methods for `PropertyDeclarationSyntax`. The methods allow for checking if a property has specific accessors like get, init, or set.

## NamespaceDeclarationSyntaxExtensions.cs
Currently commented out, this file was planned to provide extension methods to derive the namespace name from `NamespaceDeclarationSyntax`.

## MemberDeclarationSyntaxExtensions.cs
This file contains extension methods for various member syntax types, particularly `AccessorDeclarationSyntax`. It provides methods to determine the visibility and kind of a member.

## TypeDeclarationSyntaxExtensions.cs
Features extension methods for `TypeDeclarationSyntax`. These methods are beneficial for fetching attributes, identifying the containing namespace, and checking various attributes of a type.

