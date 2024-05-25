using System;
using System.IO;
using System.Linq;

using fin.data.queues;
using fin.model.io.exporters;
using fin.model.io.exporters.assimp.indirect;
using fin.io;
using fin.util.gc;

using HaloWarsTools.Helpers;

namespace HaloWarsTools {
  public class Program {
    public void Run(string scratchDirectoryPath, string outputDirectoryPath) {
      string gameDirectory = null;

      if (OperatingSystem.IsWindows()) {
        gameDirectory = SteamInterop.GetGameInstallDirectory("HaloWarsDE");
        Console.WriteLine(
            $"Found Halo Wars Definitive Edition install at {gameDirectory}");
      }

      // Point the framework to the game install and working directories
      var context = new HWContext(gameDirectory, scratchDirectoryPath);

      // Expand all compressed/encrypted game files. This also handles the .xmb -> .xml conversion
      context.ExpandAllEraFiles();

      var scratchDirectory = new FinDirectory(scratchDirectoryPath);
      var mapDirectories = scratchDirectory
                           .AssertGetExistingSubdir("scenario/skirmish/design")
                           .GetExistingSubdirs();

      var outputDirectory = new FinDirectory(outputDirectoryPath);

      var baseDstMapDirectory =
          outputDirectory.GetOrCreateSubdir("scenario/skirmish/design");
      foreach (var srcMapDirectory in mapDirectories) {
        var mapName = srcMapDirectory.Name;

        var dstMapDirectory = baseDstMapDirectory.GetOrCreateSubdir(mapName);

        var gltfFile = new FinFile(
            Path.Combine(dstMapDirectory.FullPath, $"{mapName}.gltf"));
        if (gltfFile.Exists) {
          continue;
        }

        var xttFile = srcMapDirectory.GetExistingFiles()
                                     .Single(file => file.FileType == ".xtt");
        var xtdFile = srcMapDirectory.GetExistingFiles()
                                     .Single(file => file.FileType == ".xtd");

        var xtt = HWXttResource.FromFile(context, xttFile.FullPath);
        var xtd = HWXtdResource.FromFile(context, xtdFile.FullPath);

        var finModel = xtd.Mesh;
        var xttMaterial = finModel.MaterialManager.AddStandardMaterial();

        var diffuseTexture = finModel.MaterialManager.CreateTexture(
            xtt.AlbedoTexture);
        diffuseTexture.Name = $"{mapName}_albedo";
        xttMaterial.DiffuseTexture = diffuseTexture;

        var ambientOcclusionTexture = finModel.MaterialManager.CreateTexture(
            xtd.AmbientOcclusionTexture);
        ambientOcclusionTexture.Name = $"{mapName}_ao";
        xttMaterial.AmbientOcclusionTexture = ambientOcclusionTexture;

        foreach (var primitive in finModel.Skin.Meshes[0].Primitives) {
          primitive.SetMaterial(xttMaterial);
        }

        GcUtil.ForceCollectEverything();

        var exporter = new AssimpIndirectModelExporter {
            LowLevel = true, ForceGarbageCollection = true,
        };
        exporter.ExportModel(new ModelExporterParams {
            OutputFile = gltfFile.CloneWithFileType(".fbx"), Model = finModel,
        });

        // Cleans up any remaining .bin files.
        var binFiles = dstMapDirectory.GetExistingFiles()
                                      .Where(file => file.FileType == ".bin");
        foreach (var binFile in binFiles) {
          binFile.Delete();
        }

        // Forces an immediate garbage-collection cleanup. This is required to
        // prevent OOM errors, since Halo Wars maps are just so huge.
        GcUtil.ForceCollectEverything();
      }

      var artDirectory = scratchDirectory.AssertGetExistingSubdir("art");

      var artSubdirQueue = new FinQueue<IReadOnlySystemDirectory>(artDirectory);
      // TODO: Switch to DFS instead, it's more intuitive as a user
      while (artSubdirQueue.TryDequeue(out var artSubdir)) {
        // TODO: Skip a file if it's already been extracted
        // TODO: Parse UGX files instead, as long as they specify their own animations
        var visFiles =
            artSubdir.GetExistingFiles()
                     .Where(f => f.FileType == ".vis")
                     .ToList();
        foreach (var visFile in visFiles) {
          var vis = HWVisResource.FromFile(context, visFile.FullPath);

          var finModel = vis.Scene;

          var outFilePath =
              visFile.FullPath.Replace(scratchDirectoryPath,
                                       outputDirectoryPath);
          var outFile = new FinFile(outFilePath).CloneWithFileType(".fbx");
          if (outFile.TryGetParent(out var parent)) {
            parent.Create();
          }

          /*var exporter = new AssimpIndirectModelExporter();
          exporter.ExportModel(new ModelExporterParams {
              OutputFile = outFile, Model = finModel
          });*/
          Console.WriteLine($"Processed {visFile.FullPath}");
        }

        artSubdirQueue.Enqueue(artSubdir.GetExistingSubdirs());
      }


      /*var gls = HWGlsResource.FromFile(context,
                                       "scenario\\skirmish\\design\\blood_gulch\\blood_gulch.gls");
      Console.WriteLine($"Processed {gls}");

      var scn = HWScnResource.FromFile(context,
                                       "scenario\\skirmish\\design\\blood_gulch\\blood_gulch.scn");
      PrintScenarioObjects(scn);
      Console.WriteLine($"Processed {scn}");

      var sc2 = HWSc2Resource.FromFile(context,
                                       "scenario\\skirmish\\design\\blood_gulch\\blood_gulch.sc2");
      PrintScenarioObjects(sc2);
      Console.WriteLine($"Processed {sc2}");

      var sc3 = HWSc3Resource.FromFile(context,
                                       "scenario\\skirmish\\design\\blood_gulch\\blood_gulch.sc3");
      PrintScenarioObjects(sc3);
      Console.WriteLine($"Processed {sc3}");
      }*/
    }

    /*static void PrintScenarioObjects(HWScnResource scenario) {
      foreach (var obj in scenario.Objects) {
        Console.WriteLine($"\t{obj}");
      }
    }*/
  }
}