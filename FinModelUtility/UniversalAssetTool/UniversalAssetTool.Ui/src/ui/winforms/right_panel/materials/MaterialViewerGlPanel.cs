using System;

using fin.model;
using fin.model.impl;
using fin.ui.rendering.gl;
using fin.ui.rendering.gl.material;

using OpenTK.Graphics.OpenGL;

using uni.ui.winforms.common;

using PrimitiveType = fin.model.PrimitiveType;

namespace uni.ui.winforms.right_panel.materials {
  public class MaterialViewerGlPanel : BGlPanel, IMaterialViewerPanel {
    private IReadOnlyMaterial? material_;
    private IGlMaterialShader? materialShader_;

    public IReadOnlyMaterial? Material {
      get => this.material_;
      set {
        this.material_ = value;
        this.materialShader_?.Dispose();
        this.materialShader_ = null;
      }
    }

    private ModelImpl viewerModel_;
    private GlBufferManager bufferManager_;
    private GlBufferManager.GlBufferRenderer bufferRenderer_;

    protected override void InitGl() {
      this.ResetGl_();

      this.viewerModel_ = new ModelImpl(4);

      var ul = this.viewerModel_.Skin.AddVertex(0, 0, 0);
      ul.SetUv(0, 0);

      var ur = this.viewerModel_.Skin.AddVertex(1, 0, 0);
      ur.SetUv(1, 0);

      var ll = this.viewerModel_.Skin.AddVertex(0, 1, 0);
      ll.SetUv(0, 1);

      var lr = this.viewerModel_.Skin.AddVertex(1, 1, 0);
      lr.SetUv(1, 1);

      this.bufferManager_ = new GlBufferManager(this.viewerModel_);
      this.bufferRenderer_ = this.bufferManager_.CreateRenderer(PrimitiveType.QUADS, new[] {
          ur,
          ul,
          ll,
          lr,
      });
    }

    private void ResetGl_() => GlUtil.ResetGl();

    protected override void RenderGl() {
      var width = this.Width;
      var height = this.Height;
      GL.Viewport(0, 0, width, height);

      GlUtil.ClearColorAndDepth();

      if (this.Material == null) {
        this.materialShader_?.Dispose();
        this.materialShader_ = null;
      } else {
        if (this.materialShader_ == null) {
          this.materialShader_ =
              GlMaterialShader.FromMaterial(this.viewerModel_, this.Material);
          this.materialShader_.DisposeTextures = false;
        }

        this.RenderOrtho_();
      }
    }

    private void RenderOrtho_() {
      var width = this.Width;
      var height = this.Height;

      {
        GlTransform.MatrixMode(TransformMatrixMode.PROJECTION);
        GlTransform.LoadIdentity();
        GlTransform.Ortho2d(0, width, height, 0);

        GlTransform.MatrixMode(TransformMatrixMode.VIEW);
        GlTransform.LoadIdentity();

        GlTransform.MatrixMode(TransformMatrixMode.MODEL);
        GlTransform.LoadIdentity();
      }

      var size = MathF.Min(width, height);
      GlTransform.Translate(width / 2.0, height / 2.0, 0);
      GlTransform.Scale(size, size, 1);
      GlTransform.Translate(-.5, -.5, 0);
      GlTransform.PassMatricesIntoGl();
      
      this.materialShader_.Use();

      this.bufferRenderer_.Render();
    }
  }
}