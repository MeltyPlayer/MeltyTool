using fin.audio;
using fin.audio.io;
using fin.audio.io.importers;
using fin.io;

namespace ssm.api;

public class SsmAudioFileBundle : IAudioFileBundle {
  public required string GameName { get; init; }
  public IReadOnlyTreeFile MainFile => this.SsmFile;
  public required IReadOnlyTreeFile SsmFile { get; init; }
}

public class SsmAudioImporter : IAudioImporter<SsmAudioFileBundle> {
  public ILoadedAudioBuffer<short> ImportAudio(
      IAudioManager<short> audioManager,
      SsmAudioFileBundle audioFileBundle) {
    throw new NotImplementedException();
  }
}