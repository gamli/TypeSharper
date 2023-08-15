
# Model

The `Model` directory centralizes various data structures and models essential to the `TypeSharper` project. These models represent types, attributes, modifiers, members, and more, facilitating the transformation and representation of source code elements.

## Root Files Overview

### TypeDependencyGraph.cs
The `TypeDependencyGraph` class manages dependencies between various types (`TsType`). It offers methods to:
- Add types and their dependencies.
- Order types based on their dependencies.

### TsModelCreationSymbolErrorException.cs
Introduces a custom exception named `TsModelCreationSymbolErrorException`, thrown when encountering an issue creating a model from a Roslyn symbol (`INamedTypeSymbol`). It carries the problematic symbol in its `Symbol` property.

### TsModel.cs
Represents the `TsModel` record, encapsulating a type lookup (`TsTypeRef` to `TsType`). It provides functionalities to:
- Create a new model.
- Add types to the model.
- Differentiate between models.


## Attr Subdirectory

The `Attr` subdirectory focuses on models related to attributes.

### TsAttrValue.cs
Introduces a `TsAttrValue` record representing a value for an attribute. It offers functionalities to create array or primitive attribute values, assert and retrieve values, match the type of attribute value, and generate its C# representation.

### TsAttr.cs
Represents the `TsAttr` record encapsulating information about an attribute. It includes type, constructor arguments, named arguments, and type arguments. Provides methods to generate its C# representation.

### Def Sub-subdirectory

#### TsAttrOverloadDef.cs
Represents the `TsAttrOverloadDef` record capturing details about an overload definition of an attribute. Contains constructor parameters, named parameters, and type parameters. Offers methods to generate its C# representation.

#### TsAttrDef.cs
Introduces the `TsAttrDef` record detailing the definition of an attribute. Consists of identifier, attribute targets, and overloads. Provides methods to generate its C# representation.


## Type Subdirectory

The `Type` subdirectory encompasses models related to types, namespaces, and type references.

### TsTypeMods.cs
Introduces the `TsTypeMods` record representing various type modifiers such as visibility, abstractness, static nature, sealed nature, partial nature, and target type. Provides methods to generate its C# representation.

### TsType.cs
Represents the comprehensive `TsType` record detailing a type. Encapsulates information like type identifier, base type, containing type, namespace, type kind, modifiers, constructors, properties, methods, and attributes. Offers functionalities to generate its C# representation.

### TsNs.cs
Introduces the `TsNs` record representing a namespace. It provides capabilities to represent the global namespace, create a qualified namespace, and generate its C# representation.

### TsTypeRef.cs
Represents the `TsTypeRef` record encapsulating a reference to a type. Contains details like namespace, type identifier, and if it's an array type. Provides methods to generate its C# representation.


## Identifier Subdirectory

The `Identifier` subdirectory contains models related to identifiers, both simple and qualified.

### TsQualifiedId.cs
Represents the `TsQualifiedId` record encapsulating a qualified identifier (e.g., `Namespace.ClassName`). Consists of a list of `TsId` parts. Provides methods to construct a qualified identifier, add additional parts, and generate its C# representation.

### TsId.cs
Introduces the `TsId` record representing a simple identifier. Contains a string value for the identifier. Offers functionalities to capitalize the identifier and generate its C# representation.


## Modifier Subdirectory

The `Modifier` subdirectory focuses on models related to various modifiers that can be applied to types and members.

### TsSealedMod.cs
Introduces the `TsSealedMod` record representing the `sealed` modifier for a type. Contains a boolean indicating its presence and offers methods to generate its C# representation.

### TsTargetTypeMod.cs
Represents the `TsTargetTypeMod` record, seemingly for a target type modifier. Contains a boolean indicating its presence.

### TsAbstractMod.cs
Introduces the `TsAbstractMod` record representing the `abstract` modifier for a type. Contains a boolean indicating its presence and provides methods to generate its C# representation.

### ETsVisibility.cs
Defines the `ETsVisibility` enumeration representing various visibility levels (Private, Protected, Internal, Public) and offers methods to generate its C# representation.

### TsPartialMod.cs
Represents the `TsPartialMod` record signifying the `partial` modifier for a type or method. Contains a boolean indicating its presence and offers methods to generate its C# representation.

### TsStaticMod.cs
Introduces the `TsStaticMod` record representing the `static` modifier for a type or member. Contains a boolean indicating its presence and provides methods to generate its C# representation.


## Member Subdirectory

The `Member` subdirectory focuses on models related to class or struct members, such as properties, methods, constructors, and parameters.

### TsMember.cs
Represents the abstract `TsMember` record, serving as a base model for various member types. It encapsulates the member's modifiers (`TsMemberMods`).

### TsMemberMods.cs
Introduces the `TsMemberMods` record representing member-specific modifiers like visibility, abstractness, and static nature. Provides methods to generate its C# representation.

### TsProp.cs
Represents the `TsProp` record modeling a property. Contains details like type reference, identifier, modifiers, and body implementation. Offers methods to generate its C# representation.

### TsCtor.cs
Models the `TsCtor` record representing a constructor. Encapsulates constructor parameters, modifiers, and body. Provides methods to generate its C# representation.

### TsMethod.cs
Represents the `TsMethod` record modeling a method. Contains details like method identifier, return type, parameters, modifiers, and body. Offers methods to generate its C# representation.

### TsParam.cs
Introduces the `TsParam` record representing a parameter. Contains details about the type reference, identifier, and if it's a params parameter. Provides methods to generate its C# representation.


### TsPropAccessor.cs
Represents the `TsPropAccessor` record modeling a property accessor (like `get`, `set`, `init`). Contains details about the kind of accessor and its visibility. Offers methods to generate its C# representation.

