using fin.data.queues;
using fin.io;
using fin.io.bundles;
using fin.util.linq;
using fin.util.progress;

using uni.platforms.desktop;

namespace uni.games.unity;

public class UnityFileBundleGatherer : IAnnotatedFileBundleGatherer {
  public void GatherFileBundles(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress) {
    var knownUnityGames = new[] {
        "DoronkoWanko",
        "HyperBowl",
        "LittleKittyBigCity",
        "LogiartGrimoire",
        "Yellow Taxi Goes Vroom",
    };

    foreach (var knownUnityGame in knownUnityGames) {
      if (!SteamUtils.TryGetGameDirectory(knownUnityGame,
                                          out var steamDirectory)) {
        return;
      }

      if (!TryToFindDataDirectory_(steamDirectory, out var dataDirectory)) {
        return;
      }

      // TODO: Support Unity models
    }
  }

  private static bool TryToFindDataDirectory_(
      IReadOnlyTreeDirectory steamDirectory,
      out IReadOnlyTreeDirectory dataDirectory) {
    var directoryQueue = new FinQueue<IReadOnlyTreeDirectory>(steamDirectory);
    while (directoryQueue.TryDequeue(out var currentDirectory)) {
      var currentSubdirs = currentDirectory.GetExistingSubdirs();
      if (currentDirectory.TryToGetExistingFile("UnityPlayer.dll",
                                                out var unityPlayerDll)) {
        return currentSubdirs.TryGetFirst(d => d.Name.EndsWith("_Data"),
                                          out dataDirectory);
      }

      directoryQueue.Enqueue(currentSubdirs);
    }

    dataDirectory = null!;
    return false;
  }
}