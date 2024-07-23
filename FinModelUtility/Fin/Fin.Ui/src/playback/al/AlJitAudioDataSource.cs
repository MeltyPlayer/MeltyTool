using fin.audio;

namespace fin.ui.playback.al;

public partial class AlAudioManager {
  public IJitAudioDataSource<short> CreateJitAudioDataSource(
      AudioChannelsType audioChannelsType,
      int frequency)
    => new AlJitAudioDataSource(
        audioChannelsType,
        frequency);

  private class AlJitAudioDataSource(
      AudioChannelsType audioChannelsType,
      int frequency)
      : IJitAudioDataSource<short> {
    private DateTime lastQueueTime_;
    private int lastLengthOfQueuedSamples_;

    public AudioChannelsType AudioChannelsType { get; } = audioChannelsType;
    public int Frequency { get; } = frequency;

    public int LengthInQueuedSamples
      => this.CurrentTimeAndLengthInQueuedSamples_.Item2;

    private (DateTime, int) CurrentTimeAndLengthInQueuedSamples_ {
      get {
        var now = DateTime.Now;
        var spanSinceLastQueue = now - this.lastQueueTime_;
        var secondsSinceLastQueue = spanSinceLastQueue.TotalSeconds;
        var samplesSinceLastQueue = secondsSinceLastQueue * this.Frequency;

        var lengthInQueuedSamples =
            this.lastLengthOfQueuedSamples_ - (int) samplesSinceLastQueue;

        return (now, lengthInQueuedSamples);
      }
    }

    public void PopulateNextBufferPcm(short[] data) {
      var (now, currentQueuedSamples) =
          this.CurrentTimeAndLengthInQueuedSamples_;

      var numberOfChannels = this.AudioChannelsType switch {
          AudioChannelsType.MONO   => 1,
          AudioChannelsType.STEREO => 2,
      };

      this.lastQueueTime_ = now;
      this.lastLengthOfQueuedSamples_ = currentQueuedSamples +
                                        data.Length / numberOfChannels;

      this.OnNextBufferPopulated.Invoke(data);
    }

    public event Action<short[]> OnNextBufferPopulated = delegate { };
  }
}