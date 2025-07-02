using fin.io;
using fin.io.bundles;
using fin.util.progress;

using uni.platforms.desktop;

namespace uni.games;

public abstract class BDesktopFileBundleGatherer
    : INamedAnnotatedFileBundleGatherer {
  public abstract string Name { get; }

  public abstract string SteamName { get; }
  public abstract string? EpicName { get; }

  protected abstract void GatherFileBundlesImpl(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress,
      ISystemDirectory gameDir,
      ISystemDirectory cacheDir,
      ISystemDirectory extractedDir);

  public void GatherFileBundles(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress) {
    if (!SteamUtils.TryGetGameDirectory(this.SteamName,
                                        out var gameDir) &&
        !(this.EpicName != null &&
          EaUtils.TryGetGameDirectory(this.EpicName, out gameDir))) {
      return;
    }

    ExtractorUtil.GetOrCreateRomDirectoriesWithCache(
        this.Name,
        out var cacheDir,
        out var extractedDir);

    this.GatherFileBundlesImpl(organizer,
                               mutablePercentageProgress,
                               gameDir,
                               cacheDir,
                               extractedDir);
  }
}