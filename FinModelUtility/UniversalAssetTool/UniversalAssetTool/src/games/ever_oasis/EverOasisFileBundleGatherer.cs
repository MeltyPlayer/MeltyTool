using grezzo.api;

using fin.data.queues;
using fin.io;
using fin.io.bundles;

using uni.platforms.threeDs;

namespace uni.games.ever_oasis {
  using IAnnotatedCmbBundle = IAnnotatedFileBundle<CmbModelFileBundle>;

  public class EverOasisFileBundleGatherer
      : IAnnotatedFileBundleGatherer<CmbModelFileBundle> {
    public IEnumerable<IAnnotatedCmbBundle> GatherFileBundles() {
      if (true ||
          !new ThreeDsFileHierarchyExtractor().TryToExtractFromGame(
              "ever_oasis",
              out var fileHierarchy,
              archiveFileNameProcessor: this.ArchiveFileNameProcessor_)) {
        return Enumerable.Empty<IAnnotatedCmbBundle>();
      }

      return new AnnotatedFileBundleGathererAccumulatorWithInput<
                 CmbModelFileBundle,
                 IFileHierarchy>(fileHierarchy)
             .Add(this.GetAutomaticModels_)
             .GatherFileBundles();
    }

    private void ArchiveFileNameProcessor_(string archiveName,
                                           ref string relativeName,
                                           out bool relativeToRoot) {
      if (relativeName.StartsWith("C:")) {
        relativeName = relativeName[2..];
        relativeToRoot = true;
        return;
      }

      relativeToRoot = false;
    }

    private IEnumerable<IAnnotatedCmbBundle> GetAutomaticModels_(
        IFileHierarchy fileHierarchy) {
      var queue = new FinQueue<IFileHierarchyDirectory>(fileHierarchy.Root);
      while (queue.TryDequeue(out var dir)) {
        if (dir.TryToGetExistingSubdir("model", out var modelDir)) {
          dir.TryToGetExistingSubdir("anim", out var animDir);
          dir.TryToGetExistingSubdir("texture_set", out var textureSetDir);

          var cmbFiles = modelDir.GetFilesWithFileType(".cmb").ToArray();
          var csabFiles = animDir?.GetFilesWithFileType(".csab").ToArray();
          var ctxbFiles
              = textureSetDir?.GetFilesWithFileType(".ctxb").ToArray();

          if (cmbFiles.Length == 1 || (csabFiles?.Length ?? 0) == 0) {
            foreach (var cmbFile in cmbFiles) {
              yield return new CmbModelFileBundle(
                  "ever_oasis",
                  cmbFile,
                  csabFiles,
                  ctxbFiles,
                  null).Annotate(cmbFile);
            }
          }
        } else {
          queue.Enqueue(dir.GetExistingSubdirs());
        }
      }
    }
  }
}