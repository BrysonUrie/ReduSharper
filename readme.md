# ReduSharpter

ReduSharpter is a .NET-based tool designed to simplify and minimize C# code by removing unnecessary code while ensuring that specified tests still pass. It leverages hierarchical delta debugging techniques to traverse and simplify syntax trees of C# code files. This tool is particularly useful for reducing code complexity and isolating the minimal code required to satisfy specific test cases.

## Features

- **Hierarchical Delta Debugging**: Uses a hierarchical approach to minimize code by testing subsets of statements and their complements.
- **Syntax Tree Manipulation**: Parses and modifies C# syntax trees using the Roslyn API.
- **Automated Testing**: Runs tests using the `dotnet test` CLI to verify that the simplified code still satisfies the specified test cases.
- **Customizable Options**: Allows users to specify the solution file, target class, test class, tests to ignore, and output directory.

## How It Works

1. **Input**: The user provides:
   - A solution file path (`--solution`).
   - A target class file path (`--class`).
   - A test class name (`--testClass`).
   - A list of tests to ignore (`--ignoreTests`).
   - An optional output directory (`--output`).

2. **Initialization**: The tool validates the provided paths and initializes the output directory.

3. **Syntax Tree Traversal**: The tool parses the target class file into a syntax tree and recursively traverses its nodes.

4. **Code Simplification**: For each node, the tool attempts to minimize its child nodes using delta debugging. It tests subsets of statements to determine the minimal set required to pass the specified tests.

5. **Testing**: After each modification, the tool runs the specified tests using the `dotnet test` CLI to ensure the changes do not break the tests.

6. **Output**: The simplified code is written back to the target file or saved in the specified output directory.

## Usage

### Command-Line Interface

The tool can be run using the following command:

```bash
dotnet run --solution <path-to-solution> --class <path-to-class> --testClass <test-class-name> --ignoreTests <test1> <test2> --output <output-directory>
