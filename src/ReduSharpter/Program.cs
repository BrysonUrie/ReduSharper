using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;

namespace RosylinHDD
{
  class Program
  {
    static async void Main(string[] args)
    {
      Console.WriteLine("Starting RosylinHDD...");
      var solutionOption = new Option<string>(
        "--solution",
        "Path to the solution file of the project"
      )
      {
        IsRequired = true,
      };
      var classOption = new Option<string>(
        "--class",
        "Path to the target class"
      )
      {
        IsRequired = true,
      };
      var testsOption = new Option<string[]>(
        "--tests",
        "Test suites that will be included"
      )
      {
        IsRequired = true,
        AllowMultipleArgumentsPerToken = true,
      };

      var rootCommand = new RootCommand
      {
        solutionOption,
        classOption,
        testsOption,
      };

      rootCommand.SetHandler(
        (string solutionFile, string classFile, string[] testNames) =>
        {
          Console.WriteLine($"Solution File: {solutionFile}");
          Console.WriteLine($"Class File: {classFile}");
          Console.WriteLine("Test Names: ");
          if (testNames.Length > 0)
          {
            foreach (var testName in testNames)
            {
              Console.WriteLine($"  - {testName}");
            }
          }
          else
          {
            Console.WriteLine("  No tests provided.");
          }
        },
        solutionOption,
        classOption,
        testsOption
      );

      await rootCommand.InvokeAsync((args));

      return;
      /*try*/
      /*{*/
      /*  // Validate arguments*/
      /*  if (args.Length < 4)*/
      /*  {*/
      /*    Console.WriteLine(*/
      /*      "Insufficient arguments provided. Please provide: <MethodFilePath> <SolutionPath> <TestName> [OutputPath]"*/
      /*    );*/
      /*    return;*/
      /*  }*/
      /**/
      /*  methodFilePath = Path.GetFullPath(args[0]);*/
      /*  solutionPath = Path.GetFullPath(args[1]);*/
      /*  testName = args[2];*/
      /*  outputPath = args.Length >= 3 ? Path.GetFullPath(args[3]) : "./output";*/
      /**/
      /*  Console.WriteLine(*/
      /*    "Initialization complete. Ready to start hierarchical delta debugging."*/
      /*  );*/
      /**/
      /*  // Instantiate the HierarchicalDeltaDebugger and start the debugging process*/
      /*  HierarchicalDeltaDebugger debugger = new HierarchicalDeltaDebugger(*/
      /*    methodFilePath,*/
      /*    solutionPath,*/
      /*    testName,*/
      /*    outputPath*/
      /*  );*/
      /**/
      /*  // Start the debugging process*/
      /*  debugger.TraverseAndSimplify();*/
      /*}*/
      /*catch (Exception ex)*/
      /*{*/
      /*  Console.WriteLine($"An error occurred: {ex.Message}");*/
      /*  LogError(ex);*/
      /*}*/
    }

    /// <summary>
    /// Logs an error to the output path
    /// </summary>
    private static void LogError(Exception ex)
    {
      try
      {
        Logger.LogError(ex, outputPath);
      }
      catch (Exception logEx)
      {
        Console.WriteLine($"Failed to log the error: {logEx.Message}");
      }
    }
  }
}
