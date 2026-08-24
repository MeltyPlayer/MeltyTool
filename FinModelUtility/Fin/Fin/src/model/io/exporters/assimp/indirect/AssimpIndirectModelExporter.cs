using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;

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
        => format.FileExtension.EndsWith("gltf", StringComparison.OrdinalIgnoreCase) ||
           format.FileExtension.EndsWith("glb", StringComparison.OrdinalIgnoreCase);
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
      var materialName = finMaterial.Name?.ReplaceInvalidFilenameCharacters();

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

        gltfModelRoot.Export(gltfOutputFile.FullPath,
                             gltfModelExporter.Embedded,
                             this.LowLevel);

        if (this.ForceGarbageCollection) {
          GcUtil.ForceCollectEverything();
        }
      }
    }

    var hasMorphAnimation =
        model.AnimationManager.Animations.Any(
            animation => animation.MorphTargetFrames.Count > 0);
#if !ENABLE_PATCHED_ASSIMP_MORPH_FBX
    if (!this.LowLevel && nonGltfFormats.Length > 0 && hasMorphAnimation) {
      // Assimp 6 currently crashes in native code while writing FBX blend
      // shapes imported from glTF. This cannot be caught by managed code and
      // would terminate the entire batch exporter. The GLB written above is
      // the lossless animated result, so leave FBX disabled for these models.
      Console.WriteLine(
          "Skipping FBX export: Assimp cannot safely write morph-target " +
          "animation. The animated model was exported as GLB.");
      return;
    }
#endif

    if (!this.LowLevel && nonGltfFormats.Length > 0) {
      // The managed Assimp path uses UV0 as a temporary vertex-index channel
      // and restores the real UV/color channels with AssimpIndirectUvFixer.
      // Morph FBX export bypasses that managed post-pass to preserve vertex
      // correspondence, so it must receive the real vertex attributes here.
      gltfModelExporter.UvIndices = !hasMorphAnimation;
      gltfModelExporter.Embedded = true;

      var inputFile = outputFile.CloneWithFileType(".tmp.glb");
      var inputPath = inputFile.FullPath;
      var assimpGltfModel = gltfModelExporter.CreateModelRoot(model,
                                                              scale * 100);
      DensifyMorphTargetAccessorsForAssimp_(assimpGltfModel);
      assimpGltfModel.Export(inputPath, true, false);
      if (this.ForceGarbageCollection) {
        GcUtil.ForceCollectEverything();
      }

#if ENABLE_PATCHED_ASSIMP_MORPH_FBX
      if (hasMorphAnimation) {
        try {
          foreach (var nonGltfFormat in nonGltfFormats) {
            if (!nonGltfFormat.FileExtension.Equals(
                    "fbx", StringComparison.OrdinalIgnoreCase)) {
              throw new NotSupportedException(
                  "The bundled morph-animation exporter supports FBX only.");
            }
            var outputPath = outputFile
                             .CloneWithFileType(".fbx")
                             .FullPath;
            ExportMorphAnimationFbx_(inputPath, outputPath);
          }
        } finally {
          if (!string.Equals(
                  Environment.GetEnvironmentVariable(
                      "MELTYTOOL_KEEP_ASSIMP_BRIDGE"),
                  "1",
                  StringComparison.Ordinal)) {
            File.Delete(inputPath);
          }
        }
        return;
      }
#endif

      using var ctx = new AssimpContext();
      var assScene = ctx.ImportFile(inputPath);
      File.Delete(inputPath);

      // Importing the pre-generated GLTF file does most of the hard work off
      // the bat: generating the mesh with properly weighted bones.

      // Bone orientation is already correct, you just need to enable
      // "Automatic Bone Orientation" if importing in Blender.

#if ENABLE_PATCHED_ASSIMP_MORPH_FBX
      if (!hasMorphAnimation) {
        AssimpIndirectAnimationFixer.Fix(model, assScene);
        AssimpIndirectUvFixer.Fix(model, assScene);
        AssimpIndirectTextureFixer.Fix(model, assScene);
      }
#else
      AssimpIndirectAnimationFixer.Fix(model, assScene);
      AssimpIndirectUvFixer.Fix(model, assScene);
      AssimpIndirectTextureFixer.Fix(model, assScene);
#endif

      foreach (var nonGltfFormat in nonGltfFormats) {
        var nonGltfOutputFile =
            outputFile.CloneWithFileType($".{nonGltfFormat.FileExtension}");

        var outputPath = nonGltfOutputFile.FullPath;
        var outputExtension = nonGltfOutputFile.FileType;

        var supportedExportFormats = ctx.GetSupportedExportFormats();

        // TODO: Are these all safe to include?
#if ENABLE_PATCHED_ASSIMP_MORPH_FBX
        // Reprocessing an imported scene with animation meshes corrupts the
        // morph vertex correspondence before the FBX writer sees it.
        var preProcessing = PostProcessSteps.None;
#else
        var preProcessing =
            PostProcessSteps.FindInvalidData |
            PostProcessSteps.JoinIdenticalVertices;
#endif

        // TODO: Write to abstracted filesystem instead
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

#if ENABLE_PATCHED_ASSIMP_MORPH_FBX
  private static void ExportMorphAnimationFbx_(string inputPath,
                                                string outputPath) {
    var converterPath = Path.Combine(
        AppContext.BaseDirectory,
        "assimp_morph_fbx_converter.exe");
    if (!File.Exists(converterPath)) {
      throw new FileNotFoundException(
          "The bundled animated FBX converter is missing.", converterPath);
    }

    var startInfo = new ProcessStartInfo(converterPath) {
        UseShellExecute = false,
        CreateNoWindow = true,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
    };
    startInfo.ArgumentList.Add(inputPath);
    startInfo.ArgumentList.Add(outputPath);

    using var process = Process.Start(startInfo) ??
                        throw new InvalidOperationException(
                            "Failed to start the animated FBX converter.");
    var stdout = process.StandardOutput.ReadToEnd();
    var stderr = process.StandardError.ReadToEnd();
    process.WaitForExit();
    if (process.ExitCode != 0 || !File.Exists(outputPath)) {
      throw new InvalidOperationException(
          $"Animated FBX export failed (exit {process.ExitCode}). " +
          $"{stderr}{stdout}");
    }
  }
#endif

  /// <summary>
  /// Assimp's glTF importer does not handle morph accessors that contain only
  /// sparse data (a legal glTF representation emitted by SharpGLTF). Convert
  /// just the temporary Assimp bridge model to dense buffers and retain only
  /// the standard glTF geometry morph semantics that Assimp's FBX exporter
  /// understands. Normal glTF/GLB exports retain their original representation.
  /// </summary>
  private static void DensifyMorphTargetAccessorsForAssimp_(
      SharpGLTF.Schema2.ModelRoot gltfModel) {
    foreach (var mesh in gltfModel.LogicalMeshes) {
      foreach (var primitive in mesh.Primitives) {
        for (var targetIndex = 0;
             targetIndex < primitive.MorphTargetsCount;
             ++targetIndex) {
          var targetAccessors = primitive.GetMorphTargetAccessors(targetIndex);
          var denseAccessors = new Dictionary<string, SharpGLTF.Schema2.Accessor>();
          foreach (var (attribute, accessor) in targetAccessors) {
            if (attribute is not ("POSITION" or "NORMAL" or "TANGENT")) {
              continue;
            }

            if (accessor.SourceBufferView != null && !accessor.IsSparse) {
              denseAccessors[attribute] = accessor;
              continue;
            }

            // Densify even non-sparse all-zero accessors: SharpGLTF legally
            // represents those without a bufferView, which Assimp also treats
            // as missing data.
            var denseAccessor = gltfModel.CreateAccessor(accessor.Name);
            switch (accessor.Dimensions) {
              case SharpGLTF.Schema2.DimensionType.VEC2: {
                var values = accessor.AsVector2Array().ToArray();
                var bufferView = SharpGLTF.Schema2.Toolkit.CreateBufferView(
                    gltfModel,
                    values);
                denseAccessor.SetVertexData(bufferView,
                                            0,
                                            values.Length,
                                            SharpGLTF.Schema2.DimensionType.VEC2);
                break;
              }
              case SharpGLTF.Schema2.DimensionType.VEC3: {
                var values = accessor.AsVector3Array().ToArray();
                var bufferView = SharpGLTF.Schema2.Toolkit.CreateBufferView(
                    gltfModel,
                    values);
                denseAccessor.SetVertexData(bufferView,
                                            0,
                                            values.Length,
                                            SharpGLTF.Schema2.DimensionType.VEC3);
                break;
              }
              case SharpGLTF.Schema2.DimensionType.VEC4: {
                var values = accessor.AsVector4Array().ToArray();
                var bufferView = SharpGLTF.Schema2.Toolkit.CreateBufferView(
                    gltfModel,
                    values);
                denseAccessor.SetVertexData(bufferView,
                                            0,
                                            values.Length,
                                            SharpGLTF.Schema2.DimensionType.VEC4);
                break;
              }
              default:
                throw new InvalidOperationException(
                    $"Unsupported morph attribute dimension " +
                    $"{accessor.Dimensions} for {attribute}.");
            }
            denseAccessors[attribute] = denseAccessor;
          }

          primitive.SetMorphTargetAccessors(targetIndex, denseAccessors);
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
