using System.Numerics;

using fin.util.hash;

namespace fin.model.impl;

public partial class ModelImpl<TVertex> {
  private class TextureTransformImpl : ITextureTransform {
    public bool IsTransform3d { get; private set; }

    public Vector3? Center { get; private set; }

    public ITextureTransform SetCenter2d(float x, float y) {
      this.Center = new Vector3(x, y, 0);
      return this;
    }

    public ITextureTransform SetCenter3d(float x, float y, float z) {
      this.Center = new Vector3(x, y, z);
      this.IsTransform3d = true;
      return this;
    }

    public Vector3? Translation { get; private set; }

    public ITextureTransform SetTranslation2d(in Vector2 xy) {
      this.Translation = new Vector3(xy.X, xy.Y, 0);
      return this;
    }

    public ITextureTransform SetTranslation3d(in Vector3 xyz) {
      this.Translation = xyz;
      this.IsTransform3d = true;
      return this;
    }


    public Vector3? Scale { get; private set; }

    public ITextureTransform SetScale2d(in Vector2 xy) {
      this.Scale = new Vector3(xy.X, xy.Y, 0);
      return this;
    }

    public ITextureTransform SetScale3d(in Vector3 xyz) {
      this.Scale = xyz;
      this.IsTransform3d = true;
      return this;
    }


    public Vector3? RotationRadians { get; private set; }

    public ITextureTransform SetRotationRadians2d(float rotationRadians) {
      this.RotationRadians = new Vector3 { Z = rotationRadians };
      return this;
    }

    public ITextureTransform SetRotationRadians3d(in Vector3 xyz) {
      this.RotationRadians = xyz;
      this.IsTransform3d = true;
      return this;
    }

    public override int GetHashCode() {
      var fluentHash = new FluentHash().With(this.IsTransform3d);

      if (this.Center != null) {
        fluentHash.With(this.Center.Value);
      }

      if (this.Translation != null) {
        fluentHash.With(this.Translation.Value);
      }

      if (this.RotationRadians != null) {
        fluentHash.With(this.RotationRadians.Value);
      }

      if (this.Scale != null) {
        fluentHash.With(this.Scale.Value);
      }

      return fluentHash;
    }

    public override bool Equals(object? other) {
      if (ReferenceEquals(null, other)) {
        return false;
      }

      if (ReferenceEquals(this, other)) {
        return true;
      }

      if (other is IReadOnlyTextureTransform otherTexture) {
        return this.IsTransform3d == otherTexture.IsTransform3d &&
               this.Center == otherTexture.Center &&
               this.Translation == otherTexture.Translation &&
               this.RotationRadians == otherTexture.RotationRadians &&
               this.Scale == otherTexture.Scale;
      }

      return false;
    }
  }
}