using fin.audio.io;

using uni.config;
using uni.ui.winforms.common.fileTreeView;

namespace uni;

public static class AudioPlaylistService {
  private static IFileTreeParentNode? gameDirectoryForPlaylist_;

  static AudioPlaylistService() {
    FileBundleService.OnFileBundleOpened
        += (_, fileBundle) => {
          if (fileBundle.FileBundle is IAudioFileBundle audioFileBundle) {
            UpdatePlaylist([audioFileBundle]);
          }
        };

    SceneInstanceService.OnSceneInstanceOpened
        += (fileTreeLeafNode, _) => {
          if (!Config.Instance.Viewer
                     .AutomaticallyPlayGameAudioForModel) {
            return;
          }

          var gameDirectory = fileTreeLeafNode?.Parent;
          while (gameDirectory?.Parent?.Parent != null) {
            gameDirectory = gameDirectory.Parent;
          }

          if (gameDirectoryForPlaylist_ == gameDirectory) {
            return;
          }

          gameDirectoryForPlaylist_ = gameDirectory;
          UpdatePlaylist(
              gameDirectory
                  .GetFilesOfType<IAudioFileBundle>(true)
                  .Select(bundle => bundle.TypedFileBundle)
                  .ToArray());
        };
  }

  public static event Action<IReadOnlyList<IAudioFileBundle>>
      OnPlaylistUpdated;

  public static void UpdatePlaylist(IReadOnlyList<IAudioFileBundle> playlist)
    => OnPlaylistUpdated?.Invoke(playlist);
}