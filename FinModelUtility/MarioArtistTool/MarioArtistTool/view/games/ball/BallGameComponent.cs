using System;
using System.Collections.Generic;
using System.Linq;

using fin.config.avalonia.services;
using fin.math;
using fin.math.rotations;
using fin.model;
using fin.model.skeleton;
using fin.model.util;
using fin.scene;
using fin.ui.rendering.gl;

using gawg.games.ball;

using marioartist.api;

namespace marioartisttool.view.games.ball;

public class BallGameComponent
    : ISceneNodeTickComponent, ISceneNodeRenderComponent {
  private readonly BallGameManager gameManager_;
  private readonly BallAudioManager audioManager_;

  private readonly BallRenderer ballRenderer_ = new(12);
  private readonly TransparentBallRenderer transparentBallRenderer_ = new(12);

  private readonly SimpleBoneTransformView boneTransformView_;

  private readonly IReadOnlyBone leftUpperArmBone_;
  private readonly IReadOnlyBone leftForearmBone_;
  private readonly IReadOnlyBone leftHandBone_;
  private readonly IReadOnlyBone rightUpperArmBone_;
  private readonly IReadOnlyBone rightForearmBone_;
  private readonly IReadOnlyBone rightHandBone_;

  private readonly float baseBallDistance_;
  private readonly float[] ballDistances_;

  private readonly float ballBaseY_;
  private readonly float minBallArcY_;
  private readonly float maxBallArcY_;

  private readonly (float upperArmRadians, float forearmRadians)[] armAngles_;

  private bool previousLeftDown_;
  private bool previousRightDown_;

  public BallGameComponent(SimpleBoneTransformView boneTransformView,
                           IReadOnlyModel model,
                           uint ballCount) {
    this.gameManager_ = new BallGameManager(ballCount, 1);
    this.audioManager_ = new(ballCount);

    this.boneTransformView_ = boneTransformView;

    var skeleton = model.Skeleton;

    var bonesByJointIndex = new Dictionary<JointIndex, IReadOnlyBone>();
    foreach (var bone in skeleton.Bones) {
      var boneName = bone.Name;
      var indexOfColon = boneName?.IndexOf(':') ?? -1;
      if (indexOfColon == -1) {
        continue;
      }

      if (Enum.TryParse(boneName![..indexOfColon], out JointIndex jointIndex)) {
        bonesByJointIndex[jointIndex] = bone;
      }
    }

    this.leftUpperArmBone_ = bonesByJointIndex[JointIndex.UPPER_ARM_1];
    this.leftForearmBone_ = bonesByJointIndex[JointIndex.FOREARM_1];
    this.leftHandBone_ = bonesByJointIndex[JointIndex.HAND_1];
    this.rightUpperArmBone_ = bonesByJointIndex[JointIndex.UPPER_ARM_0];
    this.rightForearmBone_ = bonesByJointIndex[JointIndex.FOREARM_0];
    this.rightHandBone_ = bonesByJointIndex[JointIndex.HAND_0];

    {
      var upperArmLength
          = this.leftForearmBone_.Transform.LocalTranslation.Length();
      var forearmLength
          = this.leftHandBone_.Transform.LocalTranslation.Length();

      var totalArmLength = upperArmLength + forearmLength;

      this.baseBallDistance_
          = this.leftUpperArmBone_.Transform.LocalTranslation.X;

      var minBallDistance = totalArmLength * .32f;
      var maxBallDistance = totalArmLength * .9f;

      this.ballDistances_
          = Enumerable.Range(0, (int) ballCount)
                      .Select(i => FinMath.MapRange(i,
                                                    0,
                                                    ballCount - 1,
                                                    minBallDistance,
                                                    maxBallDistance))
                      .ToArray();
      this.armAngles_
          = this.ballDistances_
                .Select(ballDistance => GetAnglesFromLegsOfTriangle_(
                            ballDistance,
                            upperArmLength,
                            forearmLength))
                .ToArray();
    }

    {
      this.ballBaseY_
          = this.leftUpperArmBone_.Transform.WorldMatrix.Translation.Y;

      var minMaxBoundsCalculator = new ModelMinMaxBoundsScaleCalculator();
      var bounds = minMaxBoundsCalculator.CalculateBounds(model);

      this.minBallArcY_ = bounds.MaxY + 64;
      this.maxBallArcY_ = this.minBallArcY_ +
                          this.ballDistances_.Last() -
                          this.ballDistances_.First();
    }

    var gameState = this.gameManager_.GameState;
    gameState.OnBallTicked
        += ballState => this.audioManager_.PlayBallTickAudio(ballState.Index);
    gameState.OnBallJuggled
        += _ => this.audioManager_.PlayBallJuggleAudio();
  }

  private static (float upperArmRadians, float forearmRadians)
      GetAnglesFromLegsOfTriangle_(float ballDistance,
                                   float upperArmLength,
                                   float forearmLength)
    => (CalculateLawOfSignsRadians_(forearmLength,
                                    ballDistance,
                                    upperArmLength),
        CalculateLawOfSignsRadians_(upperArmLength,
                                    ballDistance,
                                    forearmLength));

  private static float CalculateLawOfSignsRadians_(
      float oppositeSideLength,
      float otherSideLength0,
      float otherSideLength1) {
    var numerator = otherSideLength0 * otherSideLength0 +
                    otherSideLength1 * otherSideLength1 -
                    oppositeSideLength * oppositeSideLength;
    var denominator = 2 * otherSideLength0 * otherSideLength1;

    var angle = MathF.Acos(numerator / denominator);

    return angle;
  }

  ~BallGameComponent() => this.ReleaseUnmanagedResources_();

  public void Dispose() {
    this.ReleaseUnmanagedResources_();
    GC.SuppressFinalize(this);
  }

  private void ReleaseUnmanagedResources_() {
    this.audioManager_.Dispose();
    this.ballRenderer_.Dispose();
    this.transparentBallRenderer_.Dispose();
  }

  public void Tick(ISceneNodeInstance self) {
    this.gameManager_.Tick();
    var gameState = this.gameManager_.GameState;

    {
      var currentLeftDown = MainViewInputService.AKeyDown;
      var currentRightDown = MainViewInputService.DKeyDown;

      var leftPressed = !this.previousLeftDown_ && currentLeftDown;
      if (leftPressed) {
        gameState.MoveLeft();
      }

      var rightPressed = !this.previousRightDown_ && currentRightDown;
      if (rightPressed) {
        gameState.MoveRight();
      }

      this.previousLeftDown_ = currentLeftDown;
      this.previousRightDown_ = currentRightDown;
    }

    var leftArmAngles = this.armAngles_[gameState.LeftHandPosition];
    this.boneTransformView_.OverrideWorldRotation(
        this.leftUpperArmBone_,
        QuaternionUtil.CreateZyxRadians(
            MathF.PI / 2,
            -leftArmAngles.upperArmRadians,
            0)
    );
    this.boneTransformView_.OverrideWorldRotation(
        this.leftForearmBone_,
        QuaternionUtil.CreateZyxRadians(MathF.PI / 2, leftArmAngles.forearmRadians, 0));
    this.boneTransformView_.OverrideWorldRotation(
        this.leftHandBone_,
        QuaternionUtil.CreateZyxRadians(-MathF.PI / 2, 0, 0));

    var rightArmAngles = this.armAngles_[gameState.RightHandPosition];
    this.boneTransformView_.OverrideWorldRotation(
        this.rightUpperArmBone_,
        QuaternionUtil.CreateZyxRadians(MathF.PI / 2, rightArmAngles.upperArmRadians, 0));
    this.boneTransformView_.OverrideWorldRotation(
        this.rightForearmBone_,
        QuaternionUtil.CreateZyxRadians(MathF.PI / 2, -rightArmAngles.forearmRadians, 0));
    this.boneTransformView_.OverrideWorldRotation(
        this.rightHandBone_,
        QuaternionUtil.CreateZyxRadians(-MathF.PI / 2, 0, 0));
  }

  public void Render(ISceneNodeInstance self) {
    var ballStates = this.gameManager_.BallStates;
    var ballCount = ballStates.Count;

    for (uint ballI = 0; ballI < ballCount; ++ballI) {
      var tickDurationInAir = GetTickDurationInAir_(ballI);
      for (var tickI = 0; tickI < tickDurationInAir; ++tickI) {
        GlTransform.PushMatrix();

        var progress = tickI / (tickDurationInAir - 1f);
        var (x, y) = this.GetPosition_(ballI, ballCount, progress);

        GlTransform.Translate(x, y, 0);
        this.transparentBallRenderer_.Render();

        GlTransform.PopMatrix();
      }
    }

    foreach (var ballState in ballStates) {
      GlTransform.PushMatrix();

      var progress = ballState.AirSteppedProgress;
      if (ballState.Direction == BallDirection.RIGHT) {
        progress = 1 - progress;
      }

      var (x, y) = this.GetPosition_(ballState.Index, ballCount, progress);

      GlTransform.Translate(x, y, 0);
      this.ballRenderer_.Render();

      GlTransform.PopMatrix();
    }
  }

  private static uint GetTickDurationInAir_(uint index) => (4 + index) * 2;

  private (float x, float y) GetPosition_(uint index,
                                          int ballCount,
                                          float progress) {
    var ballDistanceX = this.baseBallDistance_ + this.ballDistances_[index];
    var ballDistanceY = float.Lerp(this.minBallArcY_,
                                   this.maxBallArcY_,
                                   1f * index / ballCount) -
                        this.ballBaseY_;

    var radians = MathF.PI * progress;
    var x = ballDistanceX * MathF.Cos(radians);
    var y = this.ballBaseY_ + ballDistanceY * MathF.Sin(radians);

    return (x, y);
  }
}