using fin.io.bundles;

namespace fin.audio.io;

public interface IAudioFileBundle : IFileBundle {
  FileBundleType IFileBundle.Type => FileBundleType.AUDIO;
}