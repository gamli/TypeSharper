# Generator Directory

The `Generator` directory is central to the TypeSharper library, containing classes that generate TypeScript-like types
from C# code. These generators use attributes to identify the types of transformations required and produce the
corresponding output.

## IntersectionGenerator.cs

The `IntersectionGenerator` class extends `TypeGenerator` and is responsible for generating intersection types. It
recognizes the `TypeSharperIntersectionAttribute` to determine the types to intersect.

## OmitGenerator.cs

The `OmitGenerator` class, an extension of `MemberSelectionTypeGenerator`, handles the creation of types that omit
specific members. It uses the `TypeSharperOmitAttribute` to pinpoint the properties that should be omitted.

## TaggedUnionGenerator.cs

This class, named `TaggedUnionGenerator`, extends `TypeGenerator`. Its primary function is to produce tagged union
types, with the `TypeSharperTaggedUnionAttribute` being used to identify these types.

## MemberSelectionTypeGenerator.cs

An abstract class, `MemberSelectionTypeGenerator`, lays the foundation for other generators that need to select
particular members from types. It provides a common structure and functionalities for these specialized generators.

