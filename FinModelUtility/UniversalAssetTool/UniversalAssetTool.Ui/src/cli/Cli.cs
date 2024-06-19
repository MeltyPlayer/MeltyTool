using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

using CommandLine;
using CommandLine.Text;

using fin.io;
using fin.model.io;
using fin.model.io.exporters;
using fin.model.io.exporters.assimp.indirect;

using uni.ui;
using uni.ui.winforms;

namespace uni.cli {
  public class Cli {
    [STAThread]
    public static int Main(string[] args) {
      IEnumerable<Error>? errors = null;

      var massExporterOptionTypes =
          AppDomain.CurrentDomain.GetAssemblies()
                   .SelectMany(s => s.GetTypes())
                   .Where(typeof(IMassExporterOptions).IsAssignableFrom);

      var plugins = PluginUtil.Plugins;

      var verbTypes = massExporterOptionTypes
                      .Concat(new[] {
                          typeof(UiOptions),
                          typeof(ListPluginOptions),
                          typeof(ConvertOptions),
                          typeof(DebugOptions),
                      })
                      .ToArray();

      /*
                  var helpText = new HelpText {
                          Heading = HeadingInfo.Default,
                          Copyright = CopyrightInfo.Default,
                          AdditionalNewLineAfterOption = true,
                      }.AddVerbs(verbTypes)
                       .ToString();
       */

      ConsoleUtil.ShowConsole();
      var parserResult =
          Parser.Default.ParseArguments(args, verbTypes)
                .WithParsed(
                    (IMassExporterOptions extractorOptions)
                        => extractorOptions.CreateMassExporter().ExportAll())
                .WithParsed((UiOptions _) => {
                  DesignModeUtil.InDesignMode = false;
                  ApplicationConfiguration.Initialize();
                  Application.Run(new UniversalAssetToolForm());
                  //new UniversalAssetToolForm().ShowDialog();
                })
                .WithParsed((ListPluginOptions _) => {
                  foreach (var plugin in plugins) {
                    PrintPluginInfo_(plugin);
                  }
                })
                .WithParsed((ConvertOptions convertOptions) => {
                  var inputFiles =
                      convertOptions.Inputs
                                    .Select(
                                        input
                                            => (
                                                IReadOnlySystemFile)
                                            new FinFile(input))
                                    .ToArray();
                  var outputFile =
                      new FinFile(convertOptions.Output);
                  var frameRate = convertOptions.FrameRate;

                  var issues = new List<string>();

                  var nonexistentInputFiles =
                      inputFiles.Where(file => !file.Exists).ToArray();
                  if (nonexistentInputFiles.Length > 0) {
                    foreach (var file in nonexistentInputFiles) {
                      issues.Add(
                          $"Input file '{file.FullPath}' does not exist.");
                    }
                  }

                  var supportedOutputFileTypes =
                      new[] { ".gltf", ".glb", ".fbx" };
                  if (!supportedOutputFileTypes.Contains(outputFile.FileType)) {
                    issues.Add(
                        $"The output file type must one of the following: {string.Join(", ", supportedOutputFileTypes)}");
                  }

                  if (frameRate < 0) {
                    issues.Add("Frame rate cannot be negative.");
                  }

                  // TODO: Verify input files
                  // TODO: Verify output file
                  // TODO: Warn the user if the output file already exists

                  bool needsHelpGettingBestMatch = false;
                  IModelImporterPlugin? bestMatch = null;
                  if (issues.Count == 0) {
                    bestMatch =
                        plugins.FirstOrDefault(
                            plugin => plugin.SupportsFiles(inputFiles));

                    if (bestMatch == null) {
                      needsHelpGettingBestMatch = true;
                      issues.Add(
                          "None of the plugins supports the full set of input files.");
                    }
                  }

                  if (issues.Count > 0) {
                    Console.WriteLine(
                        "Ran into issue(s) while trying to convert the input files:");
                    foreach (var issue in issues) {
                      Console.WriteLine($" - {issue}");
                    }

                    if (needsHelpGettingBestMatch) {
                      Console.WriteLine();

                      Console.WriteLine(
                          "Make sure that all of the input files satisfy at least one of the following plugins:");
                      Console.WriteLine();
                      foreach (var plugin in plugins) {
                        PrintPluginInfo_(plugin);
                      }
                    }

                    return;
                  }

                  Console.WriteLine(
                      "Importing the model with the following plugin: ");
                  PrintPluginInfo_(bestMatch!);
                  var model = bestMatch!.Import(
                      inputFiles,
                      frameRate);

                  Console.WriteLine("Writing the output file...");
                  var exporter =
                      new AssimpIndirectModelExporter();
                  exporter.ExportModel(new ModelExporterParams {
                      Model = model, OutputFile = outputFile,
                  });
                })
                .WithParsed((DebugOptions _) => {
                  /*var window = new DebugWindow();
                  window.Run();*/
                  //new DebugProgram().Run();
                })
                .WithNotParsed(parseErrors => errors = parseErrors);

      if (errors != null) {
        var helpText = HelpText.AutoBuild(parserResult);
        helpText.AutoHelp = true;

        throw new Exception();
      }

      return 0;
    }

    private static void PrintPluginInfo_(IModelImporterPlugin plugin) {
      var width = 80;

      {
        var offset = 0;
        for (var i = 0; i < 3; ++i) {
          Console.Write('=');
          ++offset;
        }

        Console.Write(' ');
        ++offset;

        Console.Write(plugin.DisplayName);
        offset += plugin.DisplayName.Length;

        Console.Write(' ');
        ++offset;

        for (var i = offset; i < width; ++i) {
          Console.Write('=');
        }

        Console.WriteLine();
      }

      var indent = "  ";

      Console.WriteLine(
          $"{indent}{plugin.Description}");
      Console.WriteLine();

      Console.WriteLine(
          $"{indent}Known platforms:");
      foreach (var knownPlatform in plugin
                   .KnownPlatforms) {
        Console.WriteLine(
            $"{indent} - {knownPlatform}");
      }

      Console.WriteLine();

      Console.WriteLine($"{indent}Known games:");
      foreach (var knownGame in
               plugin.KnownGames) {
        Console.WriteLine(
            $"{indent} - {knownGame}");
      }

      Console.WriteLine();

      Console.WriteLine(
          $"{indent}Required extension (exactly 1 matching file must be included):");
      foreach (var mainFileExtension in
               plugin.MainFileExtensions) {
        Console.WriteLine(
            $"{indent} - {mainFileExtension}");
      }

      Console.WriteLine();


      Console.WriteLine(
          $"{indent}Supported extensions:");
      foreach (var fileExtension in plugin
                   .FileExtensions) {
        Console.WriteLine(
            $"{indent} - {fileExtension}");
      }

      Console.WriteLine();
    }
  }
}