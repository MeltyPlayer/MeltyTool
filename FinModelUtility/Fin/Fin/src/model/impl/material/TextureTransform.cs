using System.Numerics;
using System.Runtime.CompilerServices;

using fin.math;
using fin.math.matrix.four;
using fin.math.matrix.three;
using fin.math.rotations;
using fin.util.hash;

namespace fin.model.impl;

public class TextureTransform : ITextureTransform {
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

  public Matrix4x4 AsMatrix() => this.IsTransform3d
      ? SystemMatrix4x4Util.FromCtrs(
          this.Center,
          this.Translation,
          this.RotationRadians?.CreateZyxRadians(),
          this.Scale)
      : new Matrix4x4(
          SystemMatrix3x2Util.FromCtrss(
              this.Center?.Xy(),
              this.Translation?.Xy(),
              this.RotationRadians?.Z,
              this.Scale?.Xy(),
              null));

  public override int GetHashCode()
    => new FluentHash()
       .With(this.IsTransform3d)
       .With(this.Center.OrDefaultNonScale())
       .With(this.Translation.OrDefaultNonScale())
       .With(this.RotationRadians.OrDefaultNonScale())
       .With(this.Scale.OrDefaultScale(this.IsTransform3d));

  public override bool Equals(object? other) {
    if (ReferenceEquals(null, other)) {
      var thisIs3d = this.IsTransform3d;

      return TextureTransformExtensions.AreNonScalesEquivalent(
                 this.Center,
                 null) &&
             TextureTransformExtensions.AreNonScalesEquivalent(
                 this.Translation,
                 null) &&
             TextureTransformExtensions.AreNonScalesEquivalent(
                 this.RotationRadians,
                 null) &&
             TextureTransformExtensions.AreScalesEquivalent(
                 this.Scale,
                 thisIs3d,
                 null,
                 thisIs3d);
    }

    if (ReferenceEquals(this, other)) {
      return true;
    }

    if (other is IReadOnlyTextureTransform otherTransform) {
      var thisIs3d = this.IsTransform3d;
      var otherIs3d = otherTransform.IsTransform3d;

      return this.IsTransform3d == otherTransform.IsTransform3d &&
             TextureTransformExtensions.AreNonScalesEquivalent(
                 this.Center,
                 otherTransform.Center) &&
             TextureTransformExtensions.AreNonScalesEquivalent(
                 this.Translation,
                 otherTransform.Translation) &&
             TextureTransformExtensions.AreNonScalesEquivalent(
                 this.RotationRadians,
                 otherTransform.RotationRadians) &&
             TextureTransformExtensions.AreScalesEquivalent(
                 this.Scale,
                 thisIs3d,
                 otherTransform.Scale,
                 otherIs3d);
    }

    return false;
  }
}

static file class TextureTransformExtensions {
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool AreNonScalesEquivalent(Vector3? lhs, Vector3? rhs)
    => lhs.OrDefaultNonScale() == rhs.OrDefaultNonScale();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool AreScalesEquivalent(Vector3? lhs,
                                         bool isLeft3d,
                                         Vector3? rhs,
                                         bool isRight3d)
    => lhs.OrDefaultScale(isLeft3d) == rhs.OrDefaultScale(isRight3d);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector3 OrDefaultNonScale(in this Vector3? value)
    => value ?? Vector3.Zero;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector3 OrDefaultScale(in this Vector3? value, bool is3d)
    => value ??
       (is3d ? Vector3.One : new Vector3(Vector2.One, 0));
}