using fin.audio;
using fin.audio.io;
using fin.audio.io.importers;
using fin.io;
using fin.util.array;
using fin.util.asserts;
using fin.util.sets;

using gx.adpcm;

using ssm.schema;

namespace ssm.api;

public class SsmAudioFileBundle : IAudioFileBundle {
  public required string GameName { get; init; }
  public IReadOnlyTreeFile MainFile => this.SsmFile;
  public required IReadOnlyTreeFile SsmFile { get; init; }
}

public class SsmAudioImporter : IAudioImporter<SsmAudioFileBundle> {
  public ILoadedAudioBuffer<short>[] ImportAudio(
      IAudioManager<short> audioManager,
      SsmAudioFileBundle audioFileBundle) {
    var ssm = audioFileBundle.SsmFile.ReadNew<Ssm>();
    var dsps = ssm.Dsps;

    var buffers = new ILoadedAudioBuffer<short>[dsps.Length];
    for (var d = 0; d < dsps.Length; ++d) {
      var dsp = dsps[d];

      var buffer = audioManager.CreateLoadedAudioBuffer(
          audioFileBundle,
          audioFileBundle.SsmFile.AsFileSet());
      buffer.Frequency = (int) dsp.Frequency;

      var channels = new short[dsp.ChannelCount][];
      for (var i = 0; i < channels.Length; ++i) {
        var dspChannel = dsp.Channels[i];
        var hist1 = dspChannel.InitialSampleHistory1;
        var hist2 = dspChannel.InitialSampleHistory2;

        channels[i] = GcAdpcmDecoder.Decode(
            dspChannel.Bytes,
            dspChannel.Coefficients,
            ref hist1,
            ref hist2);
      }

      buffer.SetPcm(channels);

      buffers[d] = buffer;
    }

    return buffers;
  }
}