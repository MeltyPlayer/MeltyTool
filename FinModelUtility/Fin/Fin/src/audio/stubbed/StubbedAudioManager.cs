using System;

namespace fin.audio.stubbed;

public partial class StubbedAudioManager : IAudioManager<short> {
  public bool IsDisposed { get; private set; }

  ~StubbedAudioManager() => this.ReleaseUnmanagedResources_();

  public void Dispose() {
    this.ReleaseUnmanagedResources_();
    GC.SuppressFinalize(this);
  }

  private void ReleaseUnmanagedResources_() {
    this.IsDisposed = true;
    this.AudioPlayer?.Dispose();
  }

  public IJitAudioDataSource<short> CreateJitAudioDataSource(
      AudioChannelsType audioChannelsType,
      int frequency)
    => throw new NotImplementedException();
}