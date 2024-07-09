using fin.audio;
using fin.audio.io;
using fin.audio.io.importers;
using fin.io;
using fin.util.sets;

using ssm.schema;

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
    var ssm = audioFileBundle.SsmFile.ReadNew<Ssm>();

    // TODO: Support returning multiple buffers?
    var dsp = ssm.Dsps[0];
    var loadedAudioBuffer = audioManager.CreateLoadedAudioBuffer(
        audioFileBundle,
        audioFileBundle.SsmFile.AsFileSet());
    loadedAudioBuffer.Frequency = (int) dsp.Frequency;

    //return loadedAudioBuffer;

    return default!;
  }
}