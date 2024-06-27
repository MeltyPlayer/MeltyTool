using fin.audio;

using OpenTK.Audio.OpenAL;

namespace fin.ui.playback.al;

public partial class AlAudioManager {
  private abstract class BAlAudioPlayback : IAudioPlayback<short> {
    private readonly IAudioPlayer<short> player_;
    protected int AlSourceId { get; }

    public bool IsDisposed { get; private set; }
    public IAudioDataSource<short> Source { get; }

    protected BAlAudioPlayback(IAudioPlayer<short> player,
                               IAudioDataSource<short> source) {
        this.player_ = player;
        this.Source = source;

        AL.GenSource(out var alSourceId);
        this.AlSourceId = (int) alSourceId;
      }

    ~BAlAudioPlayback() => this.ReleaseUnmanagedResources_();

    public void Dispose() {
        this.AssertNotDisposed();

        this.ReleaseUnmanagedResources_();
        GC.SuppressFinalize(this);
      }

    private void ReleaseUnmanagedResources_() {
        this.IsDisposed = true;
        AL.DeleteSource(this.AlSourceId);

        this.DisposeInternal();
      }

    protected abstract void DisposeInternal();

    protected void AssertNotDisposed() {
        if (this.State == PlaybackState.DISPOSED) {
          throw new Exception("Expected active sound to not be disposed");
        }
      }

    public PlaybackState State {
      get {
          if (this.IsDisposed) {
            return PlaybackState.DISPOSED;
          }

          AL.GetSource(this.AlSourceId,
                       ALGetSourcei.SourceState,
                       out int state);
          var typedState = (ALSourceState) state;
          return typedState switch {
              ALSourceState.Initial => PlaybackState.STOPPED,
              ALSourceState.Playing => PlaybackState.PLAYING,
              ALSourceState.Paused  => PlaybackState.PAUSED,
              ALSourceState.Stopped => PlaybackState.STOPPED,
              _                     => throw new ArgumentOutOfRangeException()
          };
        }
    }

    public void Play() {
        this.AssertNotDisposed();
        AL.SourcePlay(this.AlSourceId);
      }

    public void Stop() {
        this.AssertNotDisposed();
        AL.SourceStop(this.AlSourceId);
      }

    public float Volume {
      get {
          this.AssertNotDisposed();

          AL.GetSource(this.AlSourceId, ALSourcef.Gain, out var gain);
          gain *= this.player_.Volume;
          return gain;
        }
      set {
          this.AssertNotDisposed();
          AL.Source(this.AlSourceId,
                    ALSourcef.Gain,
                    value * this.player_.Volume);
        }
    }
  }
}