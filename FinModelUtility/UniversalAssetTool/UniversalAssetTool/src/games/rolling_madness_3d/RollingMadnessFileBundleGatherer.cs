using fin.io;
using fin.io.bundles;
using fin.util.progress;

using rollingMadness.api;

namespace uni.games.rolling_madness_3d;

public sealed class RollingMadness3dFileBundleGatherer : BPrereqsFileBundleGatherer {
  public override string Name => "rolling_madness_3d";

  public override FileBundleGathererPlatform Platform
    => FileBundleGathererPlatform.DESKTOP;

  protected override void GatherFileBundlesFromHierarchy(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress,
      IFileHierarchy fileHierarchy) {
    var textureDirectory
        = fileHierarchy.Root.AssertGetExistingSubdir("texture");
    var actors = fileHierarchy.Root.AssertGetExistingSubdir("actor")
                              .FilesWithExtension(".txt")
                              .Select(file => (
                                  File: file,
                                  Metadata: RollingMadnessMetadataParser.ParseActor(
                                      file.NameWithoutExtension.ToString(),
                                      file.ReadAllText())))
                              .ToArray();

    foreach (var aseMeshFile in fileHierarchy.Root.FilesWithExtensionRecursive(
                 ".ase.mesh")) {
      var aseMeshName = aseMeshFile.Name.ToString();
      var matchingActors = actors.Where(actor =>
          actor.Metadata.Animations.Any(animation =>
              ModelNamesMatch_(animation.ModelFileName, aseMeshName))).ToArray();
      var animationMetadata = matchingActors.SelectMany(actor =>
          actor.Metadata.Animations.Where(animation =>
              ModelNamesMatch_(animation.ModelFileName, aseMeshName))).ToArray();
      var specularValues = matchingActors.Select(actor => actor.Metadata.Specular)
                                         .Where(value => value.HasValue)
                                         .Select(value => value!.Value)
                                         .Distinct()
                                         .ToArray();
      if (specularValues.Length > 1) {
        throw new InvalidDataException(
            $"Conflicting actor specular values reference {aseMeshName}.");
      }

      var metadataFiles = matchingActors.Select(actor =>
                                            (IReadOnlyTreeFile) actor.File.Impl)
                                        .ToList();
      var modelSidecarName = aseMeshName.EndsWith(".mesh",
                                                   StringComparison.OrdinalIgnoreCase)
                                 ? aseMeshName[..^".mesh".Length] + ".txt"
                                 : aseMeshName + ".txt";
      if (aseMeshFile.AssertGetParent().TryToGetExistingFile(
              modelSidecarName, out var modelSidecar)) {
        metadataFiles.Add(modelSidecar);
      }

      organizer.Add(
          new AseMeshModelFileBundle(
              aseMeshFile.Impl, textureDirectory.Impl, animationMetadata,
              specularValues.Length == 1 ? specularValues[0] : null,
              metadataFiles));
    }
  }

  private static bool ModelNamesMatch_(string actorModelName,
                                       string extractedModelName) {
    static string Normalize(string name) {
      name = Path.GetFileName(name);
      if (name.EndsWith(".ase.mesh", StringComparison.OrdinalIgnoreCase)) {
        name = name[..^".mesh".Length];
      }
      return name;
    }

    return string.Equals(Normalize(actorModelName),
                         Normalize(extractedModelName),
                         StringComparison.OrdinalIgnoreCase);
  }
}
