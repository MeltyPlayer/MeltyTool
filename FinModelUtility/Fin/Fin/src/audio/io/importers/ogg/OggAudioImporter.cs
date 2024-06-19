using System;

using fin.util.asserts;
using fin.util.sets;

using NVorbis;

namespace fin.audio.io.importers.ogg {
  public class OggAudioImporter : IAudioImporter<OggAudioFileBundle> {
    public ILoadedAudioBuffer<short> ImportAudio(
        IAudioManager<short> audioManager,
        OggAudioFileBundle audioFileBundle) {
      var oggFile = audioFileBundle.OggFile;
      Asserts.SequenceEqual(".ogg", oggFile.FileType.ToLower());

      using var ogg = new VorbisReader(oggFile.OpenRead());

      var mutableBuffer = audioManager.CreateLoadedAudioBuffer(
          audioFileBundle,
          oggFile.AsFileSet());
      mutableBuffer.Frequency = ogg.SampleRate;

      {
        var sampleCount = (int) ogg.TotalSamples;

        var channelCount = ogg.Channels;
        var floatCount = channelCount * sampleCount;
        var floatPcm = new float[floatCount];
        ogg.ReadSamples(floatPcm);

        var channels = new short[channelCount][];
        for (var c = 0; c < channelCount; ++c) {
          channels[c] = new short[sampleCount];
        }

        for (var i = 0; i < sampleCount; ++i) {
          for (var c = 0; c < channelCount; ++c) {
            var floatSample = floatPcm[2 * i + c];

            var floatMin = -1f;
            var floatMax = 1f;

            var normalizedFloatSample =
                (MathF.Max(floatMin, Math.Min(floatSample, floatMax)) -
                 floatMin) / (floatMax - floatMin);

            float shortMin = short.MinValue;
            float shortMax = short.MaxValue;

            var shortSample = (short) (shortMin +
                                       normalizedFloatSample *
                                       (shortMax - shortMin));

            channels[c][i] = shortSample;
          }
        }

        mutableBuffer.SetPcm(channels);
      }

      return mutableBuffer;
    }
  }
}