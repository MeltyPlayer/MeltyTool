using System;


namespace fin.util.time;

public interface IStopwatch {
  TimeSpan Elapsed { get; }
  void Start();
  void Stop();
  void Reset();
  void Restart();
}

public class FrameStopwatch : IStopwatch {
  public enum State {
    STOPPED,
    PLAYING,
  }

  private State state_ = State.STOPPED;
  private DateTime start_;
  private TimeSpan elapsed_ = TimeSpan.Zero;

  public TimeSpan Elapsed {
    get => this.state_ switch {
        State.STOPPED => this.elapsed_,
        State.PLAYING => FrameTime.StartOfFrame - this.start_,
        _             => throw new ArgumentOutOfRangeException()
    };
    set {
      this.elapsed_ = value;
      this.start_ = FrameTime.StartOfFrame - value;
    }
  }

  public void Start() {
    if (this.state_ == State.STOPPED) {
      this.start_ = FrameTime.StartOfFrame - this.elapsed_;
    }

    this.state_ = State.PLAYING;
  }

  public void Stop() {
    if (this.state_ == State.PLAYING) {
      this.elapsed_ = FrameTime.StartOfFrame - this.start_;
    }

    this.state_ = State.STOPPED;
  }

  public void Reset() {
    this.state_ = State.STOPPED;
    this.start_ = FrameTime.StartOfFrame;
    this.elapsed_ = TimeSpan.Zero;
  }

  public void Restart() {
    this.state_ = State.PLAYING;
    this.start_ = FrameTime.StartOfFrame;
    this.elapsed_ = TimeSpan.Zero;
  }
}