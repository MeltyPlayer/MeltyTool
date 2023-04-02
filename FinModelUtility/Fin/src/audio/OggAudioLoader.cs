using fin.io;
using fin.util.asserts;

using NVorbis;

using System;


namespace fin.audio {
  public class OggAudioFileBundle : IAudioFileBundle {
    public OggAudioFileBundle(IFileHierarchyFile oggFile) {
      this.OggFile = oggFile;
    }

    public string? GameName { get; init; }
    public IFileHierarchyFile MainFile => this.OggFile;

    public IFileHierarchyFile OggFile { get; }
  }

  public class OggAudioLoader : IAudioLoader<OggAudioFileBundle> {
    public IAudioBuffer<short> LoadAudio(
        IAudioManager<short> audioManager,
        OggAudioFileBundle audioFileBundle) {
      var oggFile = audioFileBundle.OggFile;
      Asserts.Equal(".ogg", oggFile.Extension.ToLower());

      using var ogg = new VorbisReader(oggFile.OpenRead());

      var mutableBuffer = audioManager.CreateMutableBuffer();
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