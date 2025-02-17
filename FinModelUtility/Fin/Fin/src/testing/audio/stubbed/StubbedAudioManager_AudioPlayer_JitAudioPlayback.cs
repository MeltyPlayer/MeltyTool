using fin.audio;


namespace fin.testing.audio.stubbed;

public partial class StubbedAudioManager {
  private partial class StubbedAudioPlayer {
    public IJitAudioPlayback<short> CreatePlayback(
        IJitAudioDataSource<short> buffer,
        uint bufferCount)
      => new StubbedJitAudioPlayback(buffer);

    private class StubbedJitAudioPlayback(IJitAudioDataSource<short> buffer)
        : IJitAudioPlayback<short> {
      private readonly StubbedPlaybackState state_ = new(buffer.Frequency);
      
      public void Dispose() { }
      public bool IsDisposed { get; }

      public IAudioDataSource<short> Source => buffer;
      public IJitAudioDataSource<short> TypedSource => buffer;

      public PlaybackState State => this.state_.State;

      public void Play() => this.state_.Play();
      public void Stop() => this.state_.Stop();

      public float Volume { get; set; }
    }
  }
}