using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;

namespace RosylinHDD
{
  class Program
  {
    static async Task Main(string[] args)
    {
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
      var outputOption = new Option<string>(
        "--output",
        "The output directory for the new solution"
      );
      outputOption.SetDefaultValue("../../../output/");

      var rootCommand = new RootCommand(
        "Redusharpter will remove code that is not required to run specified tests"
      )
      {
        solutionOption,
        classOption,
        testsOption,
        outputOption,
      };

      rootCommand.SetHandler(
        (
          string solutionFile,
          string classFile,
          string[] testNames,
          string outputPath
        ) =>
        {
          try
          {
            var debugger = new HierarchicalDeltaDebugger(
              classFile,
              solutionFile,
              testNames,
              outputPath
            );
            debugger.TraverseAndSimplify();
          }
          catch (Exception ex)
          {
            Console.WriteLine(ex.Message);
          }
        },
        solutionOption,
        classOption,
        testsOption,
        outputOption
      );

      await rootCommand.InvokeAsync((args));

      return;
    }

    /// <summary>
    /// Logs an error to the output path
    /// </summary>
    /*private static void LogError(Exception ex)*/
    /*{*/
    /*  try*/
    /*  {*/
    /*    Logger.LogError(ex, outputPath);*/
    /*  }*/
    /*  catch (Exception logEx)*/
    /*  {*/
    /*    Console.WriteLine($"Failed to log the error: {logEx.Message}");*/
    /*  }*/
    /*}*/
  }
}
