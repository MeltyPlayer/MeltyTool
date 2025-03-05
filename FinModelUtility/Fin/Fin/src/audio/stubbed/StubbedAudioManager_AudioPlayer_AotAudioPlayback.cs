namespace fin.audio.stubbed;

public partial class StubbedAudioManager {
  private partial class StubbedAudioPlayer {
    public IAotAudioPlayback<short> CreatePlayback(
        IAotAudioDataSource<short> buffer)
      => new StubbedAotAudioPlayback(buffer);

    private class StubbedAotAudioPlayback(IAotAudioDataSource<short> buffer)
        : IAotAudioPlayback<short> {
      private readonly StubbedPlaybackState state_ =
          new(buffer.Frequency, buffer.LengthInSamples);

      public void Dispose() { }
      public bool IsDisposed { get; }

      public IAudioDataSource<short> Source => buffer;
      public IAotAudioDataSource<short> TypedSource => buffer;

      public PlaybackState State => this.state_.State;

      public void Play() => this.state_.Play();
      public void Stop() => this.state_.Stop();
      public void Pause() => this.state_.Pause();

      public float Volume { get; set; }

      public int SampleOffset {
        get => this.state_.SampleOffset;
        set => this.state_.SampleOffset = value;
      }

      public bool Looping {
        get => this.state_.Looping;
        set => this.state_.Looping = value;
      }

      public short GetPcm(AudioChannelType channelType)
        => buffer.GetPcm(channelType, this.SampleOffset);
    }
  }
}