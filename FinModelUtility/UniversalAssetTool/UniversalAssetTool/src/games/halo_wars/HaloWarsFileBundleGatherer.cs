using fin.data.queues;
using fin.io;
using fin.io.bundles;

using HaloWarsTools;

using hw.api;

using uni.platforms.desktop;

namespace uni.games.halo_wars;

using IAnnotatedHaloWarsBundle = IAnnotatedFileBundle<IHaloWarsFileBundle>;

public class HaloWarsFileBundleGatherer
    : IAnnotatedFileBundleGatherer<IHaloWarsFileBundle> {
  public IEnumerable<IAnnotatedHaloWarsBundle> GatherFileBundles() {
      if (!SteamUtils.TryGetGameDirectory("HaloWarsDE",
                                          out var haloWarsSteamDirectory)) {
        yield break;
      }

      if (!haloWarsSteamDirectory.GetFilesWithFileType(".era", true).Any()) {
        yield break;
      }

      var scratchDirectory =
          ExtractorUtil.GetOrCreateExtractedDirectory("halo_wars");

      var context = new HWContext(haloWarsSteamDirectory.FullPath,
                                  scratchDirectory.FullPath);
      // Expand all compressed/encrypted game files. This also handles the .xmb -> .xml conversion
      context.ExpandAllEraFiles();

      var fileHierarchy = FileHierarchy.From("halo_wars", scratchDirectory);

      var mapDirectories =
          fileHierarchy.Root
                       .AssertGetExistingSubdir("scenario/skirmish/design")
                       .GetExistingSubdirs();
      foreach (var srcMapDirectory in mapDirectories) {
        var xtdFile = srcMapDirectory.FilesWithExtension(".xtd").Single();
        var xttFile = srcMapDirectory.FilesWithExtension(".xtt").Single();
        yield return new XtdModelFileBundle(xtdFile, xttFile).Annotate(xtdFile);
      }

      var artDirectory = fileHierarchy.Root.AssertGetExistingSubdir("art");
      var artSubdirQueue = new FinQueue<IFileHierarchyDirectory>(artDirectory);
      // TODO: Switch to DFS instead, it's more intuitive as a user
      while (artSubdirQueue.TryDequeue(out var artSubdir)) {
        // TODO: Skip a file if it's already been extracted
        // TODO: Parse UGX files instead, as long as they specify their own animations
        var visFiles = artSubdir.FilesWithExtension(".vis");
        foreach (var visFile in visFiles) {
          yield return new VisSceneFileBundle(visFile, context).Annotate(
              visFile);
        }

        artSubdirQueue.Enqueue(artSubdir.GetExistingSubdirs());
      }
    }
}