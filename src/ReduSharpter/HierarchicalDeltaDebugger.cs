using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RosylinHDD
{
  public class HierarchicalDeltaDebugger
  {
    public string MethodFilePath { get; set; }
    public string SolutionPath { get; set; }
    public string TestName { get; set; }
    public string[] IgnoreTests { get; set; }
    public string OutputPath { get; set; }
    private SyntaxTreeService SyntaxTreeService { get; set; }

    public HierarchicalDeltaDebugger(
      string methodFilePath,
      string solutionPath,
      string testName,
      string[] ignoreTests,
      string outputPath
    )
    {
      try
      {
        MethodFilePath = methodFilePath;
        SolutionPath = solutionPath;
        TestName = testName;
        IgnoreTests = ignoreTests;
        OutputPath = outputPath;
        SyntaxTreeService = new SyntaxTreeService(MethodFilePath);

        ValidatePaths();

        InitializeOutputDirectory();
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Initialization failed: {ex.Message}");
        throw ex; // Re-throwing the exception to ensure the caller is aware of the failure
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
          throw new FileNotFoundException(
            $"Directory not found at {OutputPath}"
          );
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Path validation failed: {ex.Message}");
        throw ex; // Re-throwing to handle in the calling constructor
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
        Console.WriteLine(
          $"Failed to create output directory at {OutputPath}: {ex.Message}"
        );
        throw ex; // Re-throwing to ensure initialization fails if the directory cannot be created
      }
    }

    /// <summary>
    /// Traverses the tree and simplifies the code
    /// </summary>
    public void TraverseAndSimplify()
    {
      try
      {
        Logger.Info("Starting traversal and simplification process...");

        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(
          System.IO.File.ReadAllText(MethodFilePath)
        );
        SyntaxNode rootNode = syntaxTree.GetRoot();

        // Start the traversal and simplification
        SyntaxNode simplifiedRoot = TraverseNode(rootNode);
        Logger.Info(simplifiedRoot.NormalizeWhitespace().ToFullString());

        File.WriteAllText(
          MethodFilePath,
          simplifiedRoot.NormalizeWhitespace().ToFullString()
        );
        // File.WriteAllText(
        //   OutputPath + "result.txt",
        //   simplifiedRoot.NormalizeWhitespace().ToFullString()
        // );

        Logger.Info("Traversal and simplification process completed.");
      }
      catch (Exception ex)
      {
        Console.WriteLine($"An error occurred during traversal: {ex.Message}");
        Logger.Error(ex.Message);
      }
    }

    private SyntaxNode TraverseNode(SyntaxNode node, int level = 0)
    {
      try
      {
        // Probably a better way to do this!
        if (node is IdentifierNameSyntax || node is LiteralExpressionSyntax || node is ObjectCreationExpressionSyntax) {
          return node;
        }
        SyntaxNode processedNode = ProcessNode(node, level);
        var newTree = SyntaxTreeService.ReplaceTreeNode(processedNode, node);
        var nodeFromTree = SyntaxTreeService.findDescendantNode(newTree, processedNode);

        if (nodeFromTree == null) {
          throw new Exception("Error finding node in tree");
        }

        List<SyntaxNode> processedChildren = [];
        foreach (var childNode in nodeFromTree.ChildNodes())
        {
          SyntaxNode processedChild = TraverseNode(childNode, level + 1);
          processedChildren.Add(processedChild);
        }

        SyntaxNode newNode = SyntaxTreeService.ReplaceTreeChildren(
          nodeFromTree,
          processedChildren
        );

        return newNode;

   
      }
      catch (Exception ex)
      {
        Logger.Error(ex.Message);
      }
      return node;
    }

    private SyntaxNode ProcessNode(SyntaxNode node, int level)
    {
      try
      {
        if (node is BlockSyntax block)
        {
          var statements = block.Statements.ToList();
          DeltaDebugger debugger = new DeltaDebugger();

          Func<List<StatementSyntax>, bool> testFunction = (subset) =>
          {
            var reducedBlock = SyntaxFactory.Block(subset);
            return TestSimplifiedMethod(reducedBlock, block);
          };

          var minimizedStatements = debugger.Minimize(statements, testFunction);
          var newBlock = SyntaxFactory.Block(minimizedStatements);

          return newBlock;
        }
      }
      catch (Exception ex)
      {
        Logger.Error(ex.Message);
      }
      return node;
    }

    private bool TestSimplifiedMethod(
      SyntaxNode simplifiedNode,
      SyntaxNode originalNode
    )
    {
      SyntaxNode newRoot = SyntaxTreeService.ReplaceTreeNode(
        simplifiedNode,
        originalNode
      );
      File.WriteAllText(
        MethodFilePath,
        newRoot.NormalizeWhitespace().ToFullString()
      );

      return TestPasses();
    }


    private bool TestPasses()
    {
      try
      {
        // Run the test using the dotnet CLI
        string[] filterStrings = IgnoreTests
          .Select(test => $"FullyQualifiedName!~{TestName}.{test}")
          .ToArray();
        string filterString = string.Join("|", filterStrings);
        ProcessStartInfo startInfo = new ProcessStartInfo(
          "dotnet",
          $"test \"{SolutionPath}\" --filter \"{filterString}\""
        )
        {
          RedirectStandardOutput = true,
          RedirectStandardError = true,
          UseShellExecute = false,
          CreateNoWindow = true,
        };

        using (Process process = Process.Start(startInfo))
        {
          bool exited = process.WaitForExit(30 * 1000);
          if (!exited)
          {
              // The process did not exit within the timeout
              Console.WriteLine("Process timed out. Killing the process...");
              process.Kill();
              return false; // Indicate that the process did not complete successfully
          }

          string output = process.StandardOutput.ReadToEnd();
          string error = process.StandardError.ReadToEnd();

          /*Console.WriteLine(output);*/
          /*if (error.Length > 0)*/
          /*  Console.WriteLine(error);*/

          return process.ExitCode == 0;
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine(
          $"An error occurred while running the test: {ex.Message}"
        );
        Logger.Error(ex.Message);
        return false;
      }
    }

    private void LogError(Exception ex)
    {
      Logger.Error(ex.Message);
    }
  }
}
