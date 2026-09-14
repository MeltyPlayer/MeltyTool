using System;

using fin.scene;

namespace marioartisttool.view.face;

public class RandomExpressionTickComponent(
    ExpressionQueueManager expressionQueueManager) : ISceneNodeTickComponent {
  private int timer_;

  public void Dispose() { }

  public void Tick(ISceneNodeInstance self) {
    if (this.timer_-- <= 0) {
      var randomExpression = (ExpressionKind) Random.Shared.Next(6);
      expressionQueueManager.RequestNextExpression(randomExpression);
      this.timer_ = 300;
    }
  }
}