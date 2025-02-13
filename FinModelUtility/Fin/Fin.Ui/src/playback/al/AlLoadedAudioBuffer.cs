using fin.audio;
using fin.io;
using fin.io.bundles;
using fin.util.asserts;

namespace fin.ui.playback.al;

public partial class AlAudioManager {
  public ILoadedAudioBuffer<short> CreateLoadedAudioBuffer(
      IFileBundle fileBundle,
      IReadOnlySet<IReadOnlyGenericFile> files)
    => new AlLoadedAudioBuffer(fileBundle, files);

  private class AlLoadedAudioBuffer(
      IFileBundle fileBundle,
      IReadOnlySet<IReadOnlyGenericFile> files)
      : AlAudioBuffer, ILoadedAudioBuffer<short> {
    private short[][] channels_;

    public new AudioChannelsType AudioChannelsType { get; private set; }

    public new int Frequency { get; set; }

    public new int LengthInSamples { get; private set; }

    public new void SetPcm(short[][] channelSamples) {
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


    public new void SetMonoPcm(short[] samples) {
      this.AudioChannelsType = AudioChannelsType.MONO;
      this.LengthInSamples = samples.Length;
      this.channels_ = [samples];
    }

    public new void SetStereoPcm(short[] leftChannelSamples,
                                 short[] rightChannelSamples) {
      Asserts.Equal(leftChannelSamples.Length,
                    rightChannelSamples.Length,
                    "Expected the left/right channels to have the same number of samples!");

      this.AudioChannelsType = AudioChannelsType.STEREO;
      this.LengthInSamples = leftChannelSamples.Length;
      this.channels_ = [leftChannelSamples, rightChannelSamples];
    }

    public new short GetPcm(AudioChannelType channelType, int sampleOffset)
      => this.channels_[channelType switch {
          AudioChannelType.MONO         => 0,
          AudioChannelType.STEREO_LEFT  => 0,
          AudioChannelType.STEREO_RIGHT => 1
      }][sampleOffset];

    public IFileBundle FileBundle => fileBundle;
    public IReadOnlySet<IReadOnlyGenericFile> Files => files;
  }
}