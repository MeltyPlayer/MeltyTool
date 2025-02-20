using System;
using System.Collections.Generic;

using fin.audio;
using fin.io;
using fin.io.bundles;
using fin.util.asserts;

namespace fin.audio.stubbed;

public partial class StubbedAudioManager {
  public IAudioBuffer<short> CreateAudioBuffer()
    => new StubbedAudioBuffer(null, new HashSet<IReadOnlyGenericFile>());

  public ILoadedAudioBuffer<short> CreateLoadedAudioBuffer(
      IFileBundle fileBundle,
      IReadOnlySet<IReadOnlyGenericFile> files)
    => new StubbedAudioBuffer(fileBundle, files);

  private class StubbedAudioBuffer(
      IFileBundle fileBundle,
      IReadOnlySet<IReadOnlyGenericFile> files)
      : ILoadedAudioBuffer<short> {
    private short[][] channels_;

    public AudioChannelsType AudioChannelsType { get; private set; }

    public int Frequency { get; set; }

    public int LengthInSamples { get; private set; }

    public void SetPcm(short[][] channelSamples) {
      switch (channelSamples.Length) {
        case 1: {
          this.SetMonoPcm(channelSamples[0]);
          break;
        }
        case 2: {
          this.SetStereoPcm(channelSamples[0], channelSamples[1]);
          break;
        }
        default: throw new NotFiniteNumberException();
      }
    }


    public void SetMonoPcm(short[] samples) {
      this.AudioChannelsType = AudioChannelsType.MONO;
      this.LengthInSamples = samples.Length;
      this.channels_ = [samples];
    }

    public void SetStereoPcm(short[] leftChannelSamples,
                             short[] rightChannelSamples) {
      Asserts.Equal(leftChannelSamples.Length,
                    rightChannelSamples.Length,
                    "Expected the left/right channels to have the same number of samples!");

      this.AudioChannelsType = AudioChannelsType.STEREO;
      this.LengthInSamples = leftChannelSamples.Length;
      this.channels_ = [leftChannelSamples, rightChannelSamples];
    }

    public short GetPcm(AudioChannelType channelType, int sampleOffset)
      => this.channels_[channelType switch {
          AudioChannelType.MONO         => 0,
          AudioChannelType.STEREO_LEFT  => 0,
          AudioChannelType.STEREO_RIGHT => 1,
          _                             => throw new ArgumentOutOfRangeException(nameof(channelType), channelType, null)
      }][sampleOffset];

    public IFileBundle FileBundle => fileBundle;
    public IReadOnlySet<IReadOnlyGenericFile> Files => files;
  }
}