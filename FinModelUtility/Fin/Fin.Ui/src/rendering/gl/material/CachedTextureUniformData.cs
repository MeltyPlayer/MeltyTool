using System.Numerics;

using Assimp.Unmanaged;

using fin.math.matrix.four;
using fin.math.matrix.three;
using fin.math.rotations;
using fin.model;
using fin.shaders.glsl;
using fin.util.time;


namespace fin.ui.rendering.gl.material {
  public class CachedTextureUniformData {
    public int TextureIndex { get; }
    public IReadOnlyTexture? FinTexture { get; }
    public GlTexture GlTexture { get; }

    public IReadOnlyFinMatrix3x2? Transform2d { get; private set; }
    public IReadOnlyFinMatrix4x4? Transform3d { get; private set; }

    public bool HasFancyData { get; }
    public IShaderUniform<int> SamplerUniform { get; }
    public IShaderUniform<Vector2> ClampMinUniform { get; }
    public IShaderUniform<Vector2> ClampMaxUniform { get; }
    public IShaderUniform<Matrix3x2> Transform2dUniform { get; }
    public IShaderUniform<Matrix4x4> Transform3dUniform { get; }

    public CachedTextureUniformData(
        string textureName,
        int textureIndex,
        IReadOnlyTexture? finTexture,
        GlTexture glTexture,
        GlShaderProgram shaderProgram) {
      this.TextureIndex = textureIndex;
      this.FinTexture = finTexture;
      this.GlTexture = glTexture;

      this.HasFancyData = GlslUtil.RequiresFancyTextureData(finTexture);
      if (!this.HasFancyData) {
        this.SamplerUniform = shaderProgram.GetUniformInt($"{textureName}");
      } else {
        this.SamplerUniform =
            shaderProgram.GetUniformInt($"{textureName}.sampler");
        this.ClampMinUniform =
            shaderProgram.GetUniformVec2($"{textureName}.clampMin");
        this.ClampMaxUniform =
            shaderProgram.GetUniformVec2($"{textureName}.clampMax");
        this.Transform2dUniform =
            shaderProgram.GetUniformMat3x2($"{textureName}.transform2d");
        this.Transform3dUniform =
            shaderProgram.GetUniformMat4($"{textureName}.transform3d");
      }
    }

    public unsafe void BindTextureAndPassInUniforms() {
      this.GlTexture.Bind(this.TextureIndex);
      this.SamplerUniform.SetAndMaybeMarkDirty(this.TextureIndex);

      if (this.HasFancyData) {
        Vector2 clampMin = new(-10000);
        Vector2 clampMax = new(10000);

        if (this.FinTexture?.WrapModeU == WrapMode.MIRROR_CLAMP) {
          clampMin.X = -1;
          clampMax.X = 2;
        }

        if (this.FinTexture?.WrapModeV == WrapMode.MIRROR_CLAMP) {
          clampMin.Y = -1;
          clampMax.Y = 2;
        }

        var clampS = this.FinTexture?.ClampS;
        var clampT = this.FinTexture?.ClampT;

        if (clampS != null) {
          clampMin.X = clampS.X;
          clampMax.X = clampS.Y;
        }

        if (clampT != null) {
          clampMin.Y = clampT.X;
          clampMax.Y = clampT.Y;
        }

        this.ClampMinUniform.SetAndMaybeMarkDirty(clampMin);
        this.ClampMaxUniform.SetAndMaybeMarkDirty(clampMax);

        if (this.FinTexture is IScrollingTexture) {
          ;
        }

        this.MaybeCalculateTextureTransform_();
        if (!(this.FinTexture?.IsTransform3d ?? false)) {
          this.Transform2dUniform.SetAndMaybeMarkDirty(this.Transform2d!.Impl);
        } else {
          var mat3d = this.Transform3d!.Impl;
          this.Transform3dUniform.SetAndMaybeMarkDirty(mat3d);
        }
      }
    }

    private void MaybeCalculateTextureTransform_() {
      var finTexture = this.FinTexture;
      var isTransform3d = finTexture?.IsTransform3d ?? false;
      var cannotCache = finTexture is IScrollingTexture;

      if (isTransform3d && (this.Transform3d == null || cannotCache)) {
        this.Transform3d = CalculateTextureTransform3d_(finTexture);
      } else if (!isTransform3d && (this.Transform2d == null || cannotCache)) {
        this.Transform2d = CalculateTextureTransform2d_(finTexture);
      }
    }

    private static IReadOnlyFinMatrix3x2 CalculateTextureTransform2d_(
        IReadOnlyTexture? texture) {
      if (texture == null) {
        return FinMatrix3x2.IDENTITY;
      }

      var scrollingTexture = texture as IScrollingTexture;
      var textureOffset = texture.Offset;
      var textureScale = texture.Scale;
      var textureRotationRadians = texture.RotationRadians;

      if ((textureOffset == null && scrollingTexture == null) &&
          textureScale == null &&
          textureRotationRadians == null) {
        return FinMatrix3x2.IDENTITY;
      }

      var secondsSinceStart = (float) FrameTime.ElapsedTime.TotalSeconds;

      Vector2? offset = null;
      if (textureOffset != null || scrollingTexture != null) {
        offset = new Vector2((textureOffset?.X ?? 0) +
                             secondsSinceStart *
                             (scrollingTexture?.ScrollSpeedX ?? 0),
                             (textureOffset?.Y ?? 0) +
                             secondsSinceStart *
                             (scrollingTexture?.ScrollSpeedY ?? 0));
      }

      Vector2? scale = null;
      if (textureScale != null) {
        scale = new Vector2(textureScale.X, textureScale.Y);
      }

      return FinMatrix3x2Util.FromTrss(offset,
                                       textureRotationRadians?.Z,
                                       scale,
                                       null);
    }

    private static IReadOnlyFinMatrix4x4 CalculateTextureTransform3d_(
        IReadOnlyTexture? texture) {
      if (texture == null) {
        return FinMatrix4x4.IDENTITY;
      }

      var scrollingTexture = texture as IScrollingTexture;
      var textureOffset = texture.Offset;
      var textureScale = texture.Scale;
      var textureRotationRadians = texture.RotationRadians;

      if ((textureOffset == null && scrollingTexture == null) &&
          textureScale == null &&
          textureRotationRadians == null) {
        return FinMatrix4x4.IDENTITY;
      }

      var secondsSinceStart = (float) FrameTime.ElapsedTime.TotalSeconds;

      Position? offset = null;
      if (textureOffset != null || scrollingTexture != null) {
        offset =
            new Position((textureOffset?.X ?? 0) +
                         secondsSinceStart *
                         (scrollingTexture?.ScrollSpeedX ?? 0),
                         (textureOffset?.Y ?? 0) +
                         secondsSinceStart *
                         (scrollingTexture?.ScrollSpeedY ?? 0),
                         textureOffset?.Z ?? 0);
      }

      Quaternion? rotation = null;
      if (textureRotationRadians != null) {
        rotation = QuaternionUtil.CreateZyx(textureRotationRadians.X,
                                            textureRotationRadians.Y,
                                            textureRotationRadians.Z);
      }

      Scale? scale = null;
      if (textureScale != null) {
        scale = new(textureScale.X, textureScale.Y, textureScale.Z);
      }

      return FinMatrix4x4Util.FromTrs(offset, rotation, scale);
    }
  }
}