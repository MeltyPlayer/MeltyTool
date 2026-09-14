using System;
using System.Drawing;
using System.Numerics;

using fin.color;
using fin.math;
using fin.model;
using fin.scene;
using fin.ui.rendering.gl;
using fin.ui.rendering.gl.texture;
using fin.ui.rendering.gl.ubo;
using fin.util.time;

using marioartist.api;
using marioartist.schema.talent_studio.face;

namespace marioartisttool.view;

public class FaceFboTickComponent : ISceneNodeTickComponent {
  private readonly IReadOnlyTexture faceTexture_;
  private readonly FaceRenderer faceRenderer_;
  private readonly ITextureSwapManager textureSwapManager_;
  private GlFbo? fbo_;
  private readonly Expression[] expressions_;

  private ViewMatricesUbo viewMatricesUbo_;

  private (Expression expression, float weight)[] weightedExpressions_
      = new (Expression expression, float weight)[2];

  public FaceFboTickComponent(
      IReadOnlyTexture faceTexture,
      ITextureSwapManager textureSwapManager,
      Expression[] expressions) {
    this.faceTexture_ = faceTexture;
    this.faceRenderer_ = new(this.faceTexture_.Image);
    this.textureSwapManager_ = textureSwapManager;
    this.expressions_ = expressions;
  }

  public void Dispose() {
    this.faceRenderer_.Dispose();
    this.fbo_?.Dispose();
    this.viewMatricesUbo_.Dispose();
  }

  public void Tick(ISceneNodeInstance self) {
    if (this.fbo_ == null) {
      this.fbo_ = new GlFbo(this.faceTexture_.Image.Width,
                            this.faceTexture_.Image.Height);
      this.textureSwapManager_.OverrideGlTexture(
          this.faceTexture_,
          this.fbo_.ColorTexture);
    }

    var seconds = FrameTime.ElapsedTimeSinceApplicationOpened.TotalSeconds;
    var fraction = .5f + .5f * MathF.Sin((float) (seconds * MathF.PI));

    this.weightedExpressions_[0] = (this.expressions_[0], fraction);
    this.weightedExpressions_[1] = (this.expressions_[1], 1 - fraction);

    GlTransform.MatrixMode(TransformMatrixMode.PROJECTION);
    GlTransform.PushMatrix();
    GlTransform.LoadIdentity();
    GlTransform.Set(
        Matrix4x4.CreateOrthographicOffCenter(0,
                                              this.fbo_.Width,
                                              0,
                                              this.fbo_.Height,
                                              -1,
                                              1));

    GlTransform.MatrixMode(TransformMatrixMode.VIEW);
    GlTransform.PushMatrix();
    GlTransform.LoadIdentity();

    GlTransform.MatrixMode(TransformMatrixMode.MODEL);
    GlTransform.PushMatrix();
    GlTransform.LoadIdentity();

    this.viewMatricesUbo_ ??= new ViewMatricesUbo();
    this.viewMatricesUbo_.UpdateData();
    this.viewMatricesUbo_.Bind();

    this.fbo_.InvokeAsDrawTarget(() => {
      GlUtil.ClearColorAndDepth();
      this.faceRenderer_.SetWeightedExpression(this.weightedExpressions_);
      this.faceRenderer_.Render();
    });

    GlTransform.MatrixMode(TransformMatrixMode.PROJECTION);
    GlTransform.PopMatrix();

    GlTransform.MatrixMode(TransformMatrixMode.VIEW);
    GlTransform.PopMatrix();

    GlTransform.MatrixMode(TransformMatrixMode.MODEL);
    GlTransform.PopMatrix();

    this.viewMatricesUbo_.UpdateData();
  }
}