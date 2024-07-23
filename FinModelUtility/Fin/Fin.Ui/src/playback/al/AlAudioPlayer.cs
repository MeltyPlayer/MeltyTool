using fin.audio;
using fin.data.disposables;

namespace fin.ui.playback.al;

public partial class AlAudioManager {
  private partial class AlAudioPlayer(IAudioPlayer<short>? parent = null)
      : IAudioPlayer<short> {
    private readonly IAudioPlayer<short>? parent_ = parent;
    private float volume_ = 1;

    private readonly TrackedDisposables<IAudioPlayer<short>>
        subAudioPlayers_ = [];

    private readonly TrackedDisposables<IAudioPlayback<short>> playbacks_ =
        [];

    public bool IsDisposed { get; private set; }

    public float Volume {
      get => this.volume_;
      set {
          this.volume_ = value;
          foreach (var subAudioPlayer in this.subAudioPlayers_) {
            subAudioPlayer.Volume = subAudioPlayer.Volume;
          }

          foreach (var playback in this.playbacks_) {
            playback.Volume = playback.Volume;
          }
        }
    }

    public IEnumerable<IAudioPlayback<short>> CurrentPlaybacks
      => this.playbacks_;

    ~AlAudioPlayer() => this.ReleaseUnmanagedResources_();

    public void Dispose() {
        this.ReleaseUnmanagedResources_();
        GC.SuppressFinalize(this);
      }

    private void ReleaseUnmanagedResources_() {
        this.IsDisposed = true;
        this.subAudioPlayers_.DisposeAll();
        this.playbacks_.DisposeAll();
      }

    public IAudioPlayer<short> CreateSubPlayer() {
        var subPlayer = new AlAudioPlayer(this);
        this.subAudioPlayers_.Add(subPlayer);
        return subPlayer;
      }

    public IAudioPlayback<short> CreatePlayback(
        IAudioDataSource<short> source)
      => source switch {
          IAotAudioDataSource<short> aotAudioDataSource =>
              this.CreatePlayback(aotAudioDataSource),
          IJitAudioDataSource<short> jitAudioDataSource =>
              this.CreatePlayback(jitAudioDataSource),
      };

    public IAotAudioPlayback<short> CreatePlayback(
        IAotAudioDataSource<short> source)
      => new AlAotAudioPlayback(this, source);

    public IJitAudioPlayback<short> CreatePlayback(
        IJitAudioDataSource<short> source,
        uint bufferCount)
      => new AlJitAudioPlayback(this, source, bufferCount);
  }
}