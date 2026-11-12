using System;

using fin.log;
using fin.util.asserts;

using SharpGLTF.Schema2;


namespace fin.model.io.exporters.gltf.lowlevel;

public sealed class LowLevelGltfModelExporter : IGltfModelExporter {
  private readonly ILogger logger_ = Logging.Create<GltfModelExporter>();

  public bool UvIndices { get; set; }
  public bool Embedded { get; set; }

  public ModelRoot CreateModelRoot(IReadOnlyModel model, float scale) {
    var modelRoot = ModelRoot.CreateModel();

    var scene = modelRoot.UseScene("default");
    var skin = modelRoot.CreateSkin();

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

    // Builds skeleton.
    var rootNode = scene.CreateNode();
    var skinNodeAndBones = GltfSkeletonBuilder.BuildAndBindSkeleton(
        rootNode,
        skin,
        scale,
        model.Skeleton);

    // Builds materials.
    var finToTexCoordAndGltfMaterial =
        GltfMaterialBuilder.GetMaterials(
            modelRoot,
            model.MaterialManager);

    // Builds meshes.
    LowLevelGltfMeshBuilder.BuildAndBindMesh(
        modelRoot,
        model,
        rootNode,
        scale,
        finToTexCoordAndGltfMaterial,
        out var gltfMeshByFinMesh);

    // Builds animations.
    new GltfAnimationBuilder().BuildAnimations(
        modelRoot,
        skinNodeAndBones,
        gltfMeshByFinMesh,
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
    modelRoot.Export(outputPath, this.Embedded, true);
  }
}
