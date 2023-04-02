using System;
using System.IO;
using System.Linq;

using fin.data.queue;
using fin.exporter;
using fin.exporter.assimp.indirect;
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
                           .GetSubdir("scenario/skirmish/design")
                           .GetExistingSubdirs();

      var outputDirectory = new FinDirectory(outputDirectoryPath);

      var baseDstMapDirectory =
          outputDirectory.GetSubdir("scenario/skirmish/design", true);
      foreach (var srcMapDirectory in mapDirectories) {
        var mapName = srcMapDirectory.Name;

        var dstMapDirectory = baseDstMapDirectory.GetSubdir(mapName, true);

        var gltfFile = new FinFile(
            Path.Combine(dstMapDirectory.FullName, $"{mapName}.gltf"));
        if (gltfFile.Exists) {
          continue;
        }

        var xttFile = srcMapDirectory.GetExistingFiles()
                                     .Single(file => file.Extension == ".xtt");
        var xtdFile = srcMapDirectory.GetExistingFiles()
                                     .Single(file => file.Extension == ".xtd");

        var xtt = HWXttResource.FromFile(context, xttFile.FullName);
        var xtd = HWXtdResource.FromFile(context, xtdFile.FullName);

        var finModel = xtd.Mesh;
        var xttMaterial = finModel.MaterialManager.AddStandardMaterial();

        xttMaterial.DiffuseTexture = finModel.MaterialManager.CreateTexture(
            xtt.AlbedoTexture);
        xttMaterial.DiffuseTexture.Name = $"{mapName}_albedo";

        xttMaterial.AmbientOcclusionTexture =
            finModel.MaterialManager.CreateTexture(
                xtd.AmbientOcclusionTexture);
        xttMaterial.AmbientOcclusionTexture.Name = $"{mapName}_ao";

        foreach (var primitive in finModel.Skin.Meshes[0].Primitives) {
          primitive.SetMaterial(xttMaterial);
        }

        GcUtil.ForceCollectEverything();

        var exporter = new AssimpIndirectExporter {
          LowLevel = true,
          ForceGarbageCollection = true,
        };
        exporter.Export(new ExporterParams {
            OutputFile = gltfFile.CloneWithExtension(".fbx"),
            Model = finModel,
        });

        // Cleans up any remaining .bin files.
        var binFiles = dstMapDirectory.GetExistingFiles()
                                      .Where(file => file.Extension == ".bin");
        foreach (var binFile in binFiles) {
          binFile.Delete();
        }

        // Forces an immediate garbage-collection cleanup. This is required to
        // prevent OOM errors, since Halo Wars maps are just so huge.
        GcUtil.ForceCollectEverything();
      }

      var artDirectory = scratchDirectory.GetSubdir("art");

      var artSubdirQueue = new FinQueue<FinDirectory>(artDirectory);
      // TODO: Switch to DFS instead, it's more intuitive as a user
      while (artSubdirQueue.TryDequeue(out var artSubdir)) {
        // TODO: Skip a file if it's already been extracted
        // TODO: Parse UGX files instead, as long as they specify their own animations
        var visFiles =
            artSubdir.GetExistingFiles()
                     .Where(f => f.Extension == ".vis")
                     .ToList();
        foreach (var visFile in visFiles) {
          var vis = HWVisResource.FromFile(context, visFile.FullName);

          var finModel = vis.Model;

          var outFilePath =
              visFile.FullName.Replace(scratchDirectoryPath,
                                       outputDirectoryPath);
          var outFile = new FinFile(outFilePath).CloneWithExtension(".fbx");
          if (outFile.TryGetParent(out var parent)) {
            parent.Create();
          }

          var exporter = new AssimpIndirectExporter();
          exporter.Export(new ExporterParams {
              OutputFile = outFile,
              Model = finModel
          });
          Console.WriteLine($"Processed {visFile.FullName}");
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