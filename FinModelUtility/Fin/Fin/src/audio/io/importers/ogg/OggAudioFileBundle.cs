using fin.io;

namespace fin.audio.io.importers.ogg;

public class OggAudioFileBundle(IReadOnlyTreeFile oggFile)
    : IAudioFileBundle {
  public string? GameName { get; init; }
  public IReadOnlyTreeFile MainFile => this.OggFile;

  public IReadOnlyTreeFile OggFile { get; } = oggFile;
}