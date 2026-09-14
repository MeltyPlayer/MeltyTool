using System;

using fin.scene;
using fin.util.time;

namespace marioartisttool.view.face;

public enum ExpressionKind {
  NORMAL,
  LAUGH,
  ANGRY,
  SAD,
  FREE,
  SLEEP,
}

public class ExpressionQueueManager : ISceneNodeTickComponent {
  private ExpressionKind nextExpression_;
  private float nextDelaySeconds_;
  private float nextInterpolationSeconds_;
  private float nextHoldSeconds_;

  public ExpressionKind FromExpression { get; private set; }
    = ExpressionKind.NORMAL;

  public ExpressionKind ToExpression { get; private set; }
    = ExpressionKind.NORMAL;

  public float Fraction { get; private set; } = 0;

  private bool inAnimation_;
  private DateTime startDelayingTime_;
  private DateTime startInterpolatingTime_;
  private DateTime startHoldingTime_;
  private DateTime targetCompletionTime_;

  public void Dispose() { }

  // Tries to queue up a new expression. If another expression is requested
  // before this triggers, that other one will be triggered instead.
  public void RequestNextExpression(
      ExpressionKind expression,
      float delaySeconds = 0,
      float interpolationSeconds = .2f,
      float holdSeconds = 1) {
    this.nextExpression_ = expression;
    this.nextInterpolationSeconds_ = interpolationSeconds;
    this.nextHoldSeconds_ = holdSeconds;
  }

  public void Tick(ISceneNodeInstance _) {
    if (this.inAnimation_) {
      var currentTime = FrameTime.StartOfFrame;
      if (currentTime < this.startInterpolatingTime_) {
        this.Fraction = 0;
      } else if (currentTime < this.startHoldingTime_) {
        var progress = (currentTime - this.startInterpolatingTime_) /
                       (this.startHoldingTime_ - this.startInterpolatingTime_);
        this.Fraction = (float) progress;
      } else if (currentTime < this.targetCompletionTime_) {
        this.Fraction = 1;
      } else {
        this.FromExpression = this.ToExpression;
        this.Fraction = 0;
        this.inAnimation_ = false;
      }
    }
    // Otherwise, ready to get the next animation.
    else if (this.nextExpression_ != this.FromExpression) {
      this.ToExpression = this.nextExpression_;
      this.Fraction = 0;
      this.inAnimation_ = true;

      this.startDelayingTime_ = FrameTime.StartOfFrame;
      this.startInterpolatingTime_
          = this.startDelayingTime_ +
            TimeSpan.FromSeconds(this.nextDelaySeconds_);
      this.startHoldingTime_
          = this.startInterpolatingTime_ +
            TimeSpan.FromSeconds(this.nextInterpolationSeconds_);
      this.targetCompletionTime_
          = this.startHoldingTime_ +
            TimeSpan.FromSeconds(this.nextHoldSeconds_);
    }
  }
}