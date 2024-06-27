using fin.log;
using fin.util.asserts;

using SharpGLTF.Schema2;
using SharpGLTF.Validation;

namespace fin.model.io.exporters.gltf.lowlevel;

public class LowLevelGltfModelExporter : IGltfModelExporter {
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
    var skinNodeAndBones = new GltfSkeletonBuilder().BuildAndBindSkeleton(
        rootNode,
        skin,
        scale,
        model.Skeleton);

    // Builds animations.
    new GltfAnimationBuilder().BuildAnimations(
        modelRoot,
        skinNodeAndBones,
        scale,
        model.AnimationManager.Animations);

    // Builds materials.
    var finToTexCoordAndGltfMaterial =
        new GltfMaterialBuilder().GetMaterials(
            modelRoot,
            model.MaterialManager);

    // Builds meshes.
    var meshBuilder = new LowLevelGltfMeshBuilder
        { UvIndices = this.UvIndices };
    var gltfMeshes = meshBuilder.BuildAndBindMesh(
        modelRoot,
        model,
        scale,
        finToTexCoordAndGltfMaterial);

    /*var joints = skinNodeAndBones
                 .Select(
                     skinNodeAndBone => skinNodeAndBone.Item1)
                 .ToArray();*/
    foreach (var gltfMesh in gltfMeshes) {
      /*scene.CreateNode()
           .WithSkinnedMesh(gltfMesh,
                            rootNode.WorldMatrix,
                            joints);*/
      scene.CreateNode().WithMesh(gltfMesh);
    }

    return modelRoot;
  }

  public void ExportModel(IModelExporterParams modelExporterParams) {
    var outputFile = modelExporterParams.OutputFile;
    var model = modelExporterParams.Model;
    var scale = modelExporterParams.Scale;

    Asserts.True(
        outputFile.FileType.EndsWith(".gltf") ||
        outputFile.FileType.EndsWith(".glb"),
        "Target file is not a GLTF format!");

    this.logger_.BeginScope("Export");

    var modelRoot = this.CreateModelRoot(model, scale);

    var writeSettings = new WriteSettings {
        ImageWriting = this.Embedded
            ? ResourceWriteMode.EmbeddedAsBase64
            : ResourceWriteMode.SatelliteFile,
        MergeBuffers = false,
        Validation = ValidationMode.Skip,
    };

    var outputPath = outputFile.FullPath;
    this.logger_.LogInformation($"Writing to {outputPath}...");
    modelRoot.Save(outputPath, writeSettings);
  }
}