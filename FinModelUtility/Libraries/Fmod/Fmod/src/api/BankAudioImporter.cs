using fin.audio;
using fin.audio.io;
using fin.audio.io.importers;
using fin.io;
using fin.util.sets;

using Fmod5Sharp;

namespace fmod.api;

public record BankAudioFileBundle(IReadOnlyTreeFile BankFile)
    : IAudioFileBundle {
  public IReadOnlyTreeFile? MainFile => this.BankFile;
}

public class BankAudioImporter : IAudioImporter<BankAudioFileBundle> {
  public ILoadedAudioBuffer<short>[] ImportAudio(
      IAudioManager<short> audioManager,
      BankAudioFileBundle audioFileBundle) {
    var bankFile = audioFileBundle.BankFile;
    var fmodBank = FsbLoader.LoadFsbFromByteArray(bankFile.ReadAllBytes());

    return fmodBank.Samples.Select(fmodSample => {
      var finBuffer = audioManager.CreateLoadedAudioBuffer(audioFileBundle,
        bankFile.AsFileSet());

      var fmodMetadata = fmodSample.Metadata;
      finBuffer.Frequency = fmodMetadata.Frequency;

      return finBuffer;
    }).ToArray();
  }
}