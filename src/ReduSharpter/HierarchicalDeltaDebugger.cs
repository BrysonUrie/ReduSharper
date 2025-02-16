using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics;

namespace RosylinHDD
{
  public class HierarchicalDeltaDebugger
  {
    public string MethodFilePath { get; set; }
    public string SolutionPath { get; set; }
    public string TestName { get; set; }
    public string OutputPath { get; set; }
    private SyntaxTreeService SyntaxTreeService { get; set; }

    public HierarchicalDeltaDebugger(
        string methodFilePath,
        string solutionPath,
        string testName,
        string outputPath)
    {
      try
      {
        MethodFilePath = methodFilePath;
        SolutionPath = solutionPath;
        TestName = testName;
        OutputPath = outputPath;
        SyntaxTreeService = new SyntaxTreeService(MethodFilePath);

        ValidatePaths();

        InitializeOutputDirectory();
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Initialization failed: {ex.Message}");
        throw ex;  // Re-throwing the exception to ensure the caller is aware of the failure
      }
    }

    /// <summary>
    /// Ensure that the paths exist on the system
    /// </summary>
    private void ValidatePaths()
    {
      try
      {
        List<string> filePaths = [MethodFilePath, SolutionPath];
        foreach (var path in filePaths)
        {
          if (!File.Exists(path))
            throw new FileNotFoundException($"File not found at {path}");
        }
        if (!Directory.Exists(OutputPath))
        {
          throw new FileNotFoundException($"Directory not found at {OutputPath}");
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Path validation failed: {ex.Message}");
        throw ex;  // Re-throwing to handle in the calling constructor
      }
    }

    /// <summary>
    /// Creates the output directory if it doesn't currently exist
    /// </summary>
    private void InitializeOutputDirectory()
    {
      try
      {
        if (!Directory.Exists(OutputPath))
        {
          Directory.CreateDirectory(OutputPath);
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Failed to create output directory at {OutputPath}: {ex.Message}");
        throw ex;  // Re-throwing to ensure initialization fails if the directory cannot be created
      }
    }

    public void TraverseAndSimplify()
    {
      try
      {
        Console.WriteLine("Starting traversal and simplification process...");

        bool testPasses = TestPasses();

        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(System.IO.File.ReadAllText(MethodFilePath));
        SyntaxNode rootNode = syntaxTree.GetRoot();

        // Start the traversal and simplification
        SyntaxNode simplifiedRoot = TraverseNode(rootNode);
        Console.WriteLine(simplifiedRoot.NormalizeWhitespace().ToFullString());

        File.WriteAllText(MethodFilePath, rootNode.NormalizeWhitespace().ToFullString());
        File.WriteAllText(OutputPath + "result.txt", simplifiedRoot.NormalizeWhitespace().ToFullString());

        Console.WriteLine("Traversal and simplification process completed.");
      }
      catch (Exception ex)
      {
        Console.WriteLine($"An error occurred during traversal: {ex.Message}");
        LogError(ex);
      }
    }

    private SyntaxNode TraverseNode(SyntaxNode node, int level = 0)
    {
      try
      {
        Console.WriteLine($"Traversing node at level {level}: {node.Kind()}");
        SyntaxNode processedNode = ProcessNode(node, level);

        List<SyntaxNode> processedChildren = [];
        foreach (var childNode in processedNode.ChildNodes())
        {
          SyntaxNode processedChild = TraverseNode(childNode, level + 1);
          processedChildren.Add(processedChild);
        }

        SyntaxNode newNode = SyntaxTreeService.ReplaceTreeChildren(processedNode, processedChildren);

        return newNode;
      }
      catch (Exception ex)
      {
        Console.WriteLine($"An error occurred while traversing node: {ex.Message}");
        LogError(ex);
        // Rethrow if you want the traversal to stop on error
      }
      return node;
    }

    private SyntaxNode ProcessNode(SyntaxNode node, int level)
    {
      try
      {
        if (node is MethodDeclarationSyntax methodDeclaration)
        {
          var statements = methodDeclaration.Body.Statements.ToList();
          DeltaDebugger debugger = new DeltaDebugger();

          Func<List<StatementSyntax>, bool> testFunction = (subset) =>
          {
            var reducedMethod = methodDeclaration.WithBody(
                SyntaxFactory.Block(subset));
            return TestSimplifiedMethod(reducedMethod, methodDeclaration);
          };

          var minimizedStatements = debugger.Minimize(statements, testFunction);

          var newMethod = methodDeclaration.WithBody(SyntaxFactory.Block(minimizedStatements));
          SyntaxTreeService.ReplaceTreeNode(newMethod, methodDeclaration);

          return newMethod;
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine($"An error occurred while processing node at level {level}: {ex.Message}");
        LogError(ex);
      }
      return node;
    }

    private bool TestSimplifiedMethod(
        SyntaxNode simplifiedNode,
        SyntaxNode originalNode)
    {
      SyntaxNode newRoot = SyntaxTreeService.ReplaceTreeNode(simplifiedNode, originalNode);
      File.WriteAllText(MethodFilePath, newRoot.NormalizeWhitespace().ToFullString());

      Console.WriteLine("Running the test");
      if (TestPasses())
      {
        Console.WriteLine("Test passed, simplification worked.");
        return true;
      }
      else
      {
        Console.WriteLine("Test failed.");
        return false;
      }
    }

    private bool TestPasses()
    {
      try
      {
        // Run the test using the dotnet CLI
        ProcessStartInfo startInfo = new ProcessStartInfo("dotnet", $"test \"{SolutionPath}\" --filter \"FullyQualifiedName~{TestName}\"")
        {
          RedirectStandardOutput = true,
          RedirectStandardError = true,
          UseShellExecute = false,
          CreateNoWindow = true
        };

        using (Process process = Process.Start(startInfo))
        {
          process.WaitForExit();

          string output = process.StandardOutput.ReadToEnd();
          string error = process.StandardError.ReadToEnd();

          Console.WriteLine(output);
          if (error.Length > 0) Console.WriteLine(error);

          return process.ExitCode == 0;
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine($"An error occurred while running the test: {ex.Message}");
        LogError(ex);
        return false;
      }
    }

    private void LogError(Exception ex)
    {
      Logger.LogError(ex, OutputPath);
    }

  }
}

