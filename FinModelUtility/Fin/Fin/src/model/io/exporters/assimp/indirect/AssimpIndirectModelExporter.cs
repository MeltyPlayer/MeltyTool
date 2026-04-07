using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Assimp;

using CommunityToolkit.HighPerformance.Helpers;

using fin.io;
using fin.model.io.exporters.gltf;
using fin.model.io.exporters.gltf.lowlevel;
using fin.shaders.glsl;
using fin.util.asserts;
using fin.util.gc;
using fin.util.linq;


namespace fin.model.io.exporters.assimp.indirect;

public sealed class AssimpIndirectModelExporter : IModelExporter {
  // You can bet your ass I'm gonna prefix everything with ass.

  public bool AnimationOnly { get; set; }
  public bool LowLevel { get; set; }
  public bool ForceGarbageCollection { get; set; }

  public void ExportModel(IModelExporterParams modelExporterParams)
    => this.ExportExtensions(modelExporterParams,
                             !this.LowLevel
                                 ? GetDefaultExtensions_()
                                 : new[] { ".gltf" },
                             false);

  private static IReadOnlyList<string> GetDefaultExtensions_()
    => BlenderHeadlessFbxExporter.IsConfigured()
        ? [".fbx"]
        : [".fbx", ".glb"];

  public void ExportExtensions(IModelExporterParams modelExporterParams,
                               IReadOnlyList<string> exportedExtensions,
                               bool exportAllTextures) {
    var supportedExportFormats = AssimpUtil.SupportedExportFormats;
    var exportedFormats =
        exportedExtensions
            .Select(exportedExtension => exportedExtension.ToLower())
            .Select(exportedExtension =>
                        supportedExportFormats
                            .Where(exportFormat
                                       => exportedExtension ==
                                          $".{exportFormat.FileExtension}")
                            .First(
                                $"'{exportedExtension}' is not a supported export format!"))
            .ToArray();
    this.ExportFormats(modelExporterParams, exportedFormats, exportAllTextures);
  }

  public void ExportFormats(
      IModelExporterParams modelExporterParams,
      IReadOnlyList<ExportFormatDescription> exportedFormats,
      bool exportAllTextures) {
    var outputFile = modelExporterParams.OutputFile;
    var outputDirectory = outputFile.AssertGetParent();
    var model = modelExporterParams.Model;
    var scale = modelExporterParams.Scale;

    if (exportedFormats.Count == 0) {
      return;
    }

    IGltfModelExporter gltfModelExporter = !this.LowLevel
        ? new GltfModelExporter()
        : new LowLevelGltfModelExporter();

    var isGltfFormat = (ExportFormatDescription format)
        => format.FileExtension.EndsWith("gltf", StringComparison.OrdinalIgnoreCase) ||
           format.FileExtension.EndsWith("glb", StringComparison.OrdinalIgnoreCase);
    var gltfFormats = exportedFormats
                      .Where(isGltfFormat)
                      .ToArray();
    var nonGltfFormats = exportedFormats
                         .Where(exportedFormat =>
                                    !isGltfFormat(exportedFormat))
                         .ToArray();

    if (!this.AnimationOnly && exportAllTextures) {
      var textures = model.MaterialManager.Textures.DistinctBy(t => t.Name)
                          .ToArray();
      ParallelHelper.For(0,
                         textures.Length,
                         new SaveTextureAction(outputDirectory, textures));
    }

    var modelRequirements = ModelRequirements.FromModel(model);

    if (!this.AnimationOnly) {
      var finMaterials = model.MaterialManager.All;
      for (var i = 0; i < finMaterials.Count; ++i) {
        var finMaterial = finMaterials[i];
        var materialName =
            finMaterial.Name?.ReplaceInvalidFilenameCharacters() ??
            $"material{i}";

        var shaderSource = finMaterial.ToShaderSource(model, modelRequirements);
        var vertexShaderFile = new FinFile(
            Path.Combine(outputDirectory.FullPath,
                         $"{materialName}.vertex.glsl"));
        var fragmentShaderFile = new FinFile(
            Path.Combine(outputDirectory.FullPath,
                         $"{materialName}.fragment.glsl"));
        vertexShaderFile.WriteAllText(shaderSource.VertexShaderSource);
        fragmentShaderFile.WriteAllText(shaderSource.FragmentShaderSource);
      }
    }

    if (!this.AnimationOnly && gltfFormats.Length > 0) {
      gltfModelExporter.UvIndices = false;
      gltfModelExporter.Embedded = false;

      var gltfModelRoot = gltfModelExporter.CreateModelRoot(model, scale);
      if (this.ForceGarbageCollection) {
        GcUtil.ForceCollectEverything();
      }

      foreach (var gltfFormat in gltfFormats) {
        var gltfOutputFile =
            outputFile.CloneWithFileType($".{gltfFormat.FileExtension}");

        gltfModelRoot.Export(gltfOutputFile.FullPath,
                             gltfModelExporter.Embedded,
                             this.LowLevel);

        if (this.ForceGarbageCollection) {
          GcUtil.ForceCollectEverything();
        }
      }
    }

    var blenderFbxFormats = !this.LowLevel
        ? nonGltfFormats
              .Where(format => format.FileExtension.Equals("fbx",
                                                           StringComparison.OrdinalIgnoreCase))
              .ToArray()
        : Array.Empty<ExportFormatDescription>();
    var assimpFormats = nonGltfFormats
                        .Where(format => !format.FileExtension.Equals("fbx",
                                                                      StringComparison.OrdinalIgnoreCase) ||
                                         !BlenderHeadlessFbxExporter.IsConfigured())
                        .ToArray();

    if (blenderFbxFormats.Length > 0 &&
        BlenderHeadlessFbxExporter.IsConfigured()) {
      foreach (var blenderFbxFormat in blenderFbxFormats) {
        BlenderHeadlessFbxExporter.ExportFbx(
            new ModelExporterParams {
                OutputFile = outputFile.CloneWithFileType(
                    $".{blenderFbxFormat.FileExtension}"),
                Model = model,
                Scale = scale,
            },
            this.AnimationOnly);

        if (this.ForceGarbageCollection) {
          GcUtil.ForceCollectEverything();
        }
      }
    }

    if (!this.LowLevel && assimpFormats.Length > 0) {
      gltfModelExporter.UvIndices = true;
      gltfModelExporter.Embedded = true;

      var inputFile = outputFile.CloneWithFileType(".tmp.glb");
      var inputPath = inputFile.FullPath;
      gltfModelExporter.ExportModel(new ModelExporterParams {
          OutputFile = inputFile, Model = model, Scale = scale * 100,
      });
      if (this.ForceGarbageCollection) {
        GcUtil.ForceCollectEverything();
      }

      using var ctx = new AssimpContext();
      var assScene = ctx.ImportFile(inputPath);
      File.Delete(inputPath);

      // Importing the pre-generated GLTF file does most of the hard work off
      // the bat: generating the mesh with properly weighted bones.

      // Bone orientation is already correct, you just need to enable
      // "Automatic Bone Orientation" if importing in Blender.

      AssimpIndirectAnimationFixer.Fix(model, assScene);
      if (!this.AnimationOnly) {
        AssimpIndirectUvFixer.Fix(model, assScene);
        AssimpIndirectTextureFixer.Fix(model, assScene);
      }

      foreach (var assimpFormat in assimpFormats) {
        var assimpOutputFile =
            outputFile.CloneWithFileType($".{assimpFormat.FileExtension}");

        var outputPath = assimpOutputFile.FullPath;

        // TODO: Are these all safe to include?
        var preProcessing =
            PostProcessSteps.FindInvalidData |
            PostProcessSteps.JoinIdenticalVertices;

        // TODO: Write to abstracted filesystem instead
        var success =
            ctx.ExportFile(assScene,
                           outputPath,
                           assimpFormat.FormatId,
                           preProcessing);
        Asserts.True(success, "Failed to export model.");

        if (this.ForceGarbageCollection) {
          GcUtil.ForceCollectEverything();
        }
      }
    }
  }

  private readonly struct SaveTextureAction(
      ISystemDirectory outputDirectory,
      IReadOnlyList<IReadOnlyTexture> textures) : IAction {
    public void Invoke(int i) => textures[i].SaveInDirectory(outputDirectory);
  }
}
