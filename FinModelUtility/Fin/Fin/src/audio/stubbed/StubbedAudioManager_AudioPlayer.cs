using System;

using fin.audio;


namespace fin.audio.stubbed;

public partial class StubbedAudioManager {
  public IAudioPlayer<short> AudioPlayer { get; } = new StubbedAudioPlayer();

  private partial class StubbedAudioPlayer : IAudioPlayer<short> {
    public bool IsDisposed { get; }
    public void Dispose() { }

    public IAudioPlayer<short> CreateSubPlayer() => this;

    public IAudioPlayback<short> CreatePlayback(IAudioDataSource<short> buffer)
      => buffer switch {
          IAotAudioDataSource<short> aotSource => CreatePlayback(aotSource),
          IJitAudioDataSource<short> jitSource => CreatePlayback(jitSource),
          _ => throw new ArgumentOutOfRangeException(
              nameof(buffer),
              buffer,
              null)
      };

    public float Volume { get; set; }
  }
}