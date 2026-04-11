using CommandLine;

using fin.model;

using System.Text.Json;

using System.IO;

using fin.io;
using fin.model.io;
using fin.model.io.exporters;
using fin.model.io.exporters.assimp.indirect;
using fin.model.processing;
using fin.util.types;
using System.Linq;


namespace uni.cli;

public static class Cli {
  public static void Run(string[] args,
                         Action launchUi,
                         Action? runDebug = null) {
    IEnumerable<Error>? errors = null;

    var massExporterOptionTypes
        = TypesUtil.GetAllImplementationTypes<IMassExporterOptions>();

    var plugins = PluginUtil.Plugins;

    var verbTypes = massExporterOptionTypes
                    .Concat([
                        typeof(UiOptions),
                        typeof(ListPluginOptions),
                        typeof(ConvertOptions),
                        typeof(DebugOptions),
                    ])
                    .ToArray();

    Parser.Default
          .ParseArguments(args, verbTypes)
          .WithParsed((IMassExporterOptions extractorOptions)
                          => extractorOptions.CreateMassExporter().ExportAll())
          .WithParsed((UiOptions _) => {
            ConsoleUtil.ShowConsole();
            launchUi();
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

            var supportedOutputFileTypes = new[] {".gltf", ".glb", ".fbx"};
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
            var model = bestMatch!.ImportAndProcess(
                inputFiles,
                frameRate);

            WriteMaterialDebugSidecar_(model, outputFile);

            Console.WriteLine("Writing the output file...");
            var exporter =
                new AssimpIndirectModelExporter {
                    AnimationOnly = convertOptions.AnimationOnly,
                };
            exporter.ExportExtensions(new ModelExporterParams {
                                        Model = model,
                                        OutputFile = outputFile,
                                    },
                                    [outputFile.FileType],
                                    true);
          })
          .WithParsed((DebugOptions _) => runDebug?.Invoke());
  }


  private static void WriteMaterialDebugSidecar_(IReadOnlyModel model,
                                                 IReadOnlySystemFile outputFile) {
    try {
      var outputDirectory = outputFile.AssertGetParent();
      var sidecarPath = Path.Combine(
          outputDirectory.FullPath,
          $"{Path.GetFileNameWithoutExtension(outputFile.FullPath)}.materials.debug.json");
      var sidecarFile = new FinFile(sidecarPath);

      object CreateTextureData(IReadOnlyTexture texture, int slotIndex) => new {
          SlotIndex = slotIndex,
          Name = texture.Name,
          ValidFileName = texture.ValidFileName,
          UvIndex = texture.UvIndex,
          UvType = texture.UvType.ToString(),
          WrapModeU = texture.WrapModeU.ToString(),
          WrapModeV = texture.WrapModeV.ToString(),
          MinFilter = texture.MinFilter.ToString(),
          MagFilter = texture.MagFilter.ToString(),
          LodBias = texture.LodBias,
          MinLod = texture.MinLod,
          TransparencyType = texture.TransparencyType.ToString(),
          BorderColor = new {
              R = texture.BorderColor.Rb,
              G = texture.BorderColor.Gb,
              B = texture.BorderColor.Bb,
              A = texture.BorderColor.Ab,
          },
          Image = new {
              Width = texture.Image?.Width,
              Height = texture.Image?.Height,
              PixelFormat = texture.Image?.PixelFormat.ToString(),
          },
          TextureTransform = texture.TextureTransform?.ToString(),
      };

      var materials = model.MaterialManager.All
                           .Select((material, materialIndex) => new {
                               MaterialIndex = materialIndex,
                               Name = material.Name,
                               MaterialType =
                                   material is IFixedFunctionMaterial
                                       ? "FixedFunction"
                                       : material is IStandardMaterial
                                           ? "Standard"
                                           : material.GetType().Name,
                               CullingMode = material.CullingMode.ToString(),
                               TextureCount = material.Textures.Count(),
                               Textures = material.Textures
                                                  .Select((texture, slotIndex) =>
                                                              CreateTextureData(texture, slotIndex))
                                                  .ToArray(),
                               NormalTexture = material switch {
                                   IStandardMaterial standardMaterial when standardMaterial.NormalTexture != null
                                       => CreateTextureData(standardMaterial.NormalTexture, -1),
                                   IFixedFunctionMaterial fixedFunctionMaterial when fixedFunctionMaterial.NormalTexture != null
                                       => CreateTextureData(fixedFunctionMaterial.NormalTexture, -1),
                                   _ => null,
                               },
                           })
                           .ToArray();

      var payload = new {
          OutputFile = outputFile.FullPath,
          MaterialCount = materials.Length,
          Materials = materials,
      };

      var json = JsonSerializer.Serialize(
          payload,
          new JsonSerializerOptions {
              WriteIndented = true,
          });
      sidecarFile.WriteAllText(json);
      Console.WriteLine($"Wrote material debug sidecar: {sidecarPath}");
    } catch (Exception e) {
      Console.WriteLine($"Failed to write material debug sidecar: {e}");
    }
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
