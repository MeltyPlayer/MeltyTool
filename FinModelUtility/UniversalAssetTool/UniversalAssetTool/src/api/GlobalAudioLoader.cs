using ast.api;

using fin.audio;
using fin.audio.io;
using fin.audio.io.importers;
using fin.audio.io.importers.ogg;

using ssm.api;

namespace uni.api;

public class GlobalAudioReader : IAudioImporter<IAudioFileBundle> {
  public ILoadedAudioBuffer<short> ImportAudio(
      IAudioManager<short> audioManager,
      IAudioFileBundle audioFileBundle)
    => audioFileBundle switch {
        AstAudioFileBundle astAudioFileBundle
            => new AstAudioReader().ImportAudio(
                audioManager,
                astAudioFileBundle),
        OggAudioFileBundle oggAudioFileBundle
            => new OggAudioImporter().ImportAudio(
                audioManager,
                oggAudioFileBundle),
        SsmAudioFileBundle ssmAudioFileBundle
            => new SsmAudioImporter().ImportAudio(
                audioManager,
                ssmAudioFileBundle),
        _ => throw new ArgumentOutOfRangeException(nameof(audioFileBundle))
    };
}