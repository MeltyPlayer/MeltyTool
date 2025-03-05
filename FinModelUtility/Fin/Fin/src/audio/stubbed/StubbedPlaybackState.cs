using System;

using fin.util.time;


namespace fin.audio.stubbed;

public class StubbedPlaybackState(int frequency, int? sampleCount = null) {
  private readonly FrameStopwatch stopwatch_ = new();
  private PlaybackState state_;

  public PlaybackState State {
    get {
      this.UpdateState_();
      return this.state_;
    }
    private set {
      this.state_ = value;
      this.UpdateState_();
    }
  }

  public void Play() {
    this.stopwatch_.Start();
    this.State = PlaybackState.PLAYING;
  }

  public void Stop() {
    this.stopwatch_.Reset();
    this.State = PlaybackState.STOPPED;
  }

  public void Pause() {
    this.stopwatch_.Stop();
    this.State = PlaybackState.PLAYING;
  }

  public int SampleOffset {
    get {
      this.UpdateState_();
      return this.GetCurrentSampleOffset_();
    }
    set {
      this.stopwatch_.Elapsed = this.ConvertSampleOffsetToTimeSpan_(value);
      this.UpdateState_();
    }
  }

  public bool Looping { get; set; }

  private int GetCurrentSampleOffset_()
    => (int) (this.stopwatch_.Elapsed.TotalSeconds * frequency);

  private TimeSpan ConvertSampleOffsetToTimeSpan_(int sampleOffset)
    => TimeSpan.FromSeconds(1f * sampleOffset / frequency);

  private void UpdateState_() {
    if (sampleCount == null) {
      return;
    }

    if (this.state_ is not (PlaybackState.PLAYING or PlaybackState.PAUSED)) {
      return;
    }

    var sampleOffset = this.GetCurrentSampleOffset_();
    if (!(sampleOffset >= sampleCount)) {
      return;
    }

    if (this.Looping) {
      this.Stop();
    } else {
      var remainder = sampleOffset % sampleCount.Value;
      this.stopwatch_.Elapsed = this.ConvertSampleOffsetToTimeSpan_(remainder);
    }
  }
}