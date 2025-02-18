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

using SharpGLTF.Schema2;
using SharpGLTF.Validation;

namespace fin.model.io.exporters.assimp.indirect;

public class AssimpIndirectModelExporter : IModelExporter {
  // You can bet your ass I'm gonna prefix everything with ass.

  public bool LowLevel { get; set; }
  public bool ForceGarbageCollection { get; set; }

  public void ExportModel(IModelExporterParams modelExporterParams)
    => this.ExportExtensions(modelExporterParams,
                             !this.LowLevel
                                 ? [".fbx", ".glb"]
                                 : new[] { ".gltf" },
                             false);

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
        => format.FileExtension is "gltf" or "glb";
    var gltfFormats = exportedFormats
                      .Where(isGltfFormat)
                      .ToArray();
    var nonGltfFormats = exportedFormats
                         .Where(exportedFormat =>
                                    !isGltfFormat(exportedFormat))
                         .ToArray();

    if (exportAllTextures) {
      var textures = model.MaterialManager.Textures.DistinctBy(t => t.Name)
                          .ToArray();
      ParallelHelper.For(0,
                         textures.Length,
                         new SaveTextureAction(outputDirectory, textures));
    }

    var modelRequirements = ModelRequirements.FromModel(model);

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

    if (gltfFormats.Length > 0) {
      gltfModelExporter.UvIndices = false;
      gltfModelExporter.Embedded = false;

      var gltfModelRoot = gltfModelExporter.CreateModelRoot(model, scale);
      if (this.ForceGarbageCollection) {
        GcUtil.ForceCollectEverything();
      }

      foreach (var gltfFormat in gltfFormats) {
        var gltfOutputFile =
            outputFile.CloneWithFileType($".{gltfFormat.FileExtension}");

        var gltfWriteSettings
            = WriteContext.CreateFromDirectory(
                new DirectoryInfo(gltfOutputFile.AssertGetParent().FullPath));
        gltfWriteSettings.ImageWriting = gltfModelExporter.Embedded
            ? ResourceWriteMode.EmbeddedAsBase64
            : ResourceWriteMode.SatelliteFile;

        if (this.LowLevel) {
          gltfWriteSettings.MergeBuffers = false;
          gltfWriteSettings.Validation = ValidationMode.Skip;
        }

        var name
            = Path.GetFileNameWithoutExtension(
                gltfOutputFile.FullNameWithoutExtension);
        if (gltfFormat.FileExtension == "glb") {
          gltfWriteSettings.WriteBinarySchema2(name, gltfModelRoot);
        } else {
          gltfWriteSettings.WriteTextSchema2(name, gltfModelRoot);
        }

        //gltfModelRoot.Save(gltfOutputFile.FullName, gltfWriteSettings);
        if (this.ForceGarbageCollection) {
          GcUtil.ForceCollectEverything();
        }
      }
    }

    if (!this.LowLevel && nonGltfFormats.Length > 0) {
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

      new AssimpIndirectAnimationFixer().Fix(model, assScene);
      new AssimpIndirectUvFixer().Fix(model, assScene);
      new AssimpIndirectTextureFixer().Fix(model, assScene);

      foreach (var nonGltfFormat in nonGltfFormats) {
        var nonGltfOutputFile =
            outputFile.CloneWithFileType($".{nonGltfFormat.FileExtension}");

        var outputPath = nonGltfOutputFile.FullPath;
        var outputExtension = nonGltfOutputFile.FileType;

        var supportedExportFormats = ctx.GetSupportedExportFormats();

        // TODO: Are these all safe to include?
        var preProcessing =
            PostProcessSteps.FindInvalidData |
            PostProcessSteps.JoinIdenticalVertices;

        var success =
            ctx.ExportFile(assScene,
                           outputPath,
                           nonGltfFormat.FormatId,
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