# Diagnostics Directory

The `Diagnostics` directory is focused on providing diagnostic capabilities for the TypeSharper library. It aids in
error detection, reporting, and handling, ensuring that developers using TypeSharper receive clear feedback on any
issues that might arise during the source generation process.

## EDiagnosticsCode.cs

This file defines an enumeration `EDiagnosticsCode` that signifies different diagnostic codes relevant to TypeSharper.
These codes represent a variety of errors or issues that might occur, providing a clear categorization for potential
problems.

## Diag.cs

The `Diag` static class offers methods to report errors. With functionalities like `ReportError`, it provides a
structured way to handle diagnostic reporting using various parameters such as diagnostic codes, execution context, and
error messages. It ensures that errors are communicated clearly to the developers, facilitating a smoother development
experience.

