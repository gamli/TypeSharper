
# TypeSharper


## Overview

### Code Generation Mechanism:

TypeSharper employs Roslyn, the .NET Compiler Platform, to facilitate the generation of C# source code. The primary orchestrator of this operation is `TypeSharperGenerator`, which scans the provided codebase for specific attribute applications that dictate the code generation process. It then delegates the task of generating the code for specific types to individual type generators, such as `TaggedUnionGenerator`.

These type generators are designed to inherit from `TypeGenerator`, and they define their unique attributes. These attributes, when applied to partial types (like interfaces or classes), act as markers signaling the `TypeSharperGenerator` about the types that need code generation.

The generation process doesn't directly interact with the Roslyn API. Instead, it operates on the `TsModel` API, a simplified Abstract Syntax Tree (AST) that carries semantic information. This makes the code generation process less intricate and more focused on the specific needs of TypeSharper.

### Dependency Management:

When a type generator depends on other types, these dependencies are specified as generic parameters of the attribute. This ensures that the `TypeSharperGenerator` can ascertain the dependencies between types and generate them in the correct order, maintaining the integrity and correctness of the generated code. The `TypeDependencyGraph` is instrumental in tracking and resolving these dependencies.

## Creating a Type Generator

1. **Inherit from `TypeGenerator`**: This is the base class that provides essential functionalities and sets the contract for type generators.
2. **Define a Custom Attribute using `AttributeDefinition`**: Utilize the `TypeGenerator.AttributeDefinition()` method to define your custom attribute.
3. **Attribute Application**: The custom attributes are applied to partial types. The generated code will be another partial of the same type, ensuring the completeness of the type with both manual and generated code.
4. **Specify Dependencies**: If your type generator depends on other types, specify them as generic parameters of the attribute. For instance, for a `Pick` type, the type from which properties are picked is a dependency. This allows the `TypeSharperGenerator` to determine the order of code generation based on these dependencies.
5. **Leverage TsModel API**: The type generator is passed the target partial type, the attribute application, and a `TsModel` instance. The `TsModel` instance can be used to look up references of dependent types and add new types. It's important to note that `TsModel` is immutable, ensuring thread safety and deterministic behavior.
6. **Return the Updated Model**: After generating the required code, the type generator returns the updated `TsModel`. In the end, a diff between the updated and the original model is generated as C# source, which is then integrated into the target project.



- **Generator**: Contains the core classes responsible for generating TypeScript-like types from C# code based on specific attributes.
- **Model**: Central to TypeSharper, the `Model` contains the data structures and models used throughout the library. These models represent TypeScript-like constructs and offer methods to generate their C# counterparts.
- **Diagnostics**: Provides diagnostic capabilities, allowing for error detection, reporting, and handling. It ensures clear feedback for developers during the source generation process.
- **SemanticExtensions**: Contains extension methods that augment Roslyn's semantic models, facilitating the conversion of Roslyn symbols into TypeSharper models.
- **SyntaxExtensions**: Offers extension methods that supplement Roslyn's syntax models, aiding in information extraction and type conversion tailored for TypeSharper.



## TsList

Similar in spirit to `TsDict`, `TsList` wraps .NET's immutable list to make it behave like a value. Its equality members are tailored to compare the actual elements in the list rather than mere references, bringing value semantics to list operations. This ensures a more intuitive and predictable behavior when working with lists, especially in scenarios where content-based comparisons are essential. Though `TsList` includes a handful of convenience methods, its main distinction is offering value-based behavior to .NET's immutable lists.

## TsDict

`TsDict` serves as a thin wrapper around .NET's immutable dictionary, engineered primarily to make the underlying data structure behave like a value. This means that its equality members are designed to compare the actual contents of the dictionaries rather than just references. By doing so, it introduces value semantics to dictionary operations in C#, enabling developers to reason about dictionaries in terms of their actual contents. While it adds a few convenience methods to ease dictionary operations, the core value of `TsDict` lies in its provision of true value-based comparisons for immutable dictionaries.

## TypeSharperGenerator

The core component of the TypeSharper library. This class leverages the power of Roslyn to generate C# source code, introducing TypeScript-like types and constructs. It acts as a bridge, translating TypeScript idioms into their C# counterparts.



- [Generator](./Generator/README.md) - Main type generation logic.
- [Model](./Model/README.md) - Core data structures and models.
- [Diagnostics](./Diagnostics/README.md) - Diagnostic utilities and error handling.
- [SemanticExtensions](./SemanticExtensions/README.md) - Semantic model extension methods.
- [SyntaxExtensions](./SyntaxExtensions/README.md) - Syntax model extension methods.
