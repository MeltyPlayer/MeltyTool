using fin.audio;

using OpenTK.Audio.OpenAL;

namespace fin.ui.playback.al {
  public partial class AlAudioManager {
    private class AlAotAudioPlayback : BAlAudioPlayback,
                                       IAotAudioPlayback<short> {
      private int alBufferId_;

      public IAotAudioDataSource<short> TypedSource { get; }

      public AlAotAudioPlayback(IAudioPlayer<short> player,
                                IAotAudioDataSource<short> source)
          : base(player, source) {
        this.TypedSource = source;

        AL.GenBuffer(out var newBuffer);
        this.alBufferId_ = (int) newBuffer;

        ALFormat bufferFormat = default;
        short[] shortBufferData = default!;
        switch (source.AudioChannelsType) {
          case AudioChannelsType.MONO: {
            bufferFormat = ALFormat.Mono16;
            shortBufferData = new short[1 * source.LengthInSamples];

            for (var i = 0; i < source.LengthInSamples; ++i) {
              shortBufferData[i] = source.GetPcm(AudioChannelType.MONO, i);
            }

            break;
          }
          case AudioChannelsType.STEREO: {
            bufferFormat = ALFormat.Stereo16;
            shortBufferData = new short[2 * source.LengthInSamples];

            // TODO: Is this correct, are they interleaved?
            for (var i = 0; i < source.LengthInSamples; ++i) {
              shortBufferData[2 * i] =
                  source.GetPcm(AudioChannelType.STEREO_LEFT, i);
              shortBufferData[2 * i + 1] =
                  source.GetPcm(AudioChannelType.STEREO_RIGHT, i);
            }

            break;
          }
        }

        var byteCount = 2 * shortBufferData.Length;
        var byteBufferData = new byte[byteCount];
        Buffer.BlockCopy(shortBufferData,
                         0,
                         byteBufferData,
                         0,
                         byteCount);

        AL.BufferData(this.alBufferId_,
                      bufferFormat,
                      byteBufferData,
                      source.Frequency);

        AL.Source(this.AlSourceId, ALSourcei.Buffer, this.alBufferId_);
      }

      protected override void DisposeInternal() {
        AL.DeleteBuffer(this.alBufferId_);
      }

      public void Pause() {
        this.AssertNotDisposed();
        AL.SourcePause(this.AlSourceId);
      }

      public int SampleOffset {
        get {
          this.AssertNotDisposed();

          AL.GetSource(this.AlSourceId,
                       ALGetSourcei.SampleOffset,
                       out var sampleOffset);
          return sampleOffset;
        }
        set {
          this.AssertNotDisposed();

          AL.Source(this.AlSourceId, ALSourcei.SampleOffset, (int) value);
        }
      }

      public short GetPcm(AudioChannelType channelType)
        => this.TypedSource.GetPcm(channelType, this.SampleOffset);

      public bool Looping {
        get {
          this.AssertNotDisposed();

          AL.GetSource(this.AlSourceId, ALSourceb.Looping, out var looping);
          return looping;
        }
        set {
          this.AssertNotDisposed();
          AL.Source(this.AlSourceId, ALSourceb.Looping, value);
        }
      }
    }
  }
}