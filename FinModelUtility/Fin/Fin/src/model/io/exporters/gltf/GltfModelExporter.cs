using System;

using System.Linq;

using fin.log;
using fin.util.asserts;

using SharpGLTF.Schema2;

namespace fin.model.io.exporters.gltf;

public interface IGltfModelExporter : IModelExporter {
  bool UvIndices { get; set; }
  bool Embedded { get; set; }

  ModelRoot CreateModelRoot(IReadOnlyModel model, float scale);
}

public sealed class GltfModelExporter : IGltfModelExporter {
  private readonly ILogger logger_ = Logging.Create<GltfModelExporter>();

  public bool UvIndices { get; set; }
  public bool Embedded { get; set; }

  public ModelRoot CreateModelRoot(IReadOnlyModel model, float scale) {
    var modelRoot = ModelRoot.CreateModel();

    var scene = modelRoot.UseScene("default");

    var animations = model.AnimationManager.Animations;
    var firstAnimation = (animations?.Count ?? 0) > 0 ? animations[0] : null;

    if (firstAnimation != null) {
      // TODO: Put this somewhere else!
      // We should be able to use the raw bone positions, but this screws up
      // bones with multiple weights for some reason, perhaps because the
      // model is contorted in an unnatural way? Anyway, we NEED to use the
      // first animation instead.
      //this.ApplyFirstFrameToSkeleton_(model.Skeleton, firstAnimation);
    }

    var rootNode = scene.CreateNode();
    var hasSkinning = model.Skin.BoneWeights.Any(
        boneWeights => boneWeights.Weights.Count > 0);
    var hasBoneAnimation = animations?.Any(
        animation => animation.BoneTracks.Any(
            track => (track.Translations?.HasAnyData ?? false) ||
                     (track.Rotations?.HasAnyData ?? false) ||
                     (track.Scales?.HasAnyData ?? false))) ?? false;

    // Morph-only ASE meshes do not need a skin. An unused glTF skin makes
    // importers such as Blender create artificial Armature/Node_0 objects.
    Skin? gltfSkin = null;
    (Node, IReadOnlyBone)[] skinNodeAndBones = [];
    if (hasSkinning || hasBoneAnimation) {
      gltfSkin = modelRoot.CreateSkin();
      skinNodeAndBones = GltfSkeletonBuilder.BuildAndBindSkeleton(
          rootNode,
          gltfSkin,
          scale,
          model.Skeleton);
    }

    // Builds materials.
    var finToTexCoordAndGltfMaterial =
        GltfMaterialBuilder.GetMaterialBuilders(model.MaterialManager);

    // Builds meshes.
    var meshBuilder = new GltfSkinBuilder { UvIndices = this.UvIndices };
    meshBuilder.AddSkin(
        modelRoot,
        gltfSkin,
        model,
        rootNode,
        scale,
        finToTexCoordAndGltfMaterial,
        out var gltfNodeByFinMesh);

    // Builds animations.
    new GltfAnimationBuilder().BuildAnimations(
        modelRoot,
        skinNodeAndBones,
        gltfNodeByFinMesh,
        scale,
        model.AnimationManager.MorphTargets,
        model.AnimationManager.Animations);

    return modelRoot;
  }

  public void ExportModel(IModelExporterParams modelExporterParams) {
    var outputFile = modelExporterParams.OutputFile;
    var model = modelExporterParams.Model;
    var scale = modelExporterParams.Scale;

    Asserts.True(
        outputFile.FileType.EndsWith(".gltf", StringComparison.OrdinalIgnoreCase) ||
        outputFile.FileType.EndsWith(".glb", StringComparison.OrdinalIgnoreCase),
        "Target file is not a GLTF format!");

    this.logger_.BeginScope("Export");

    var outputPath = outputFile.FullPath;
    this.logger_.LogInformation($"Writing to {outputPath}...");

    var modelRoot = this.CreateModelRoot(model, scale);
    modelRoot.Export(outputPath, this.Embedded, false);
  }
}
