using System.Collections.Generic;
using System.Numerics;

using fin.data.indexable;
using fin.math.matrix.four;
using fin.math.matrix.three;
using fin.math.rotations;
using fin.model;
using fin.util.time;

namespace fin.math;

public interface IReadOnlyTextureTransformManager {
  (bool is2d, Matrix3x2 twoDMatrix, Matrix4x4 threeDMatrix)? GetMatrix(
      IReadOnlyTexture texture);
}

public interface ITextureTransformManager : IReadOnlyTextureTransformManager {
  void Clear();

  void CalculateMatrices(
      IReadOnlyList<IReadOnlyTexture> textures,
      (IReadOnlyModelAnimation, float)? animationAndFrame
  );
}

public class TextureTransformManager : ITextureTransformManager {
  private readonly IndexableDictionary<IReadOnlyTexture, (bool is2d, Matrix3x2
      twoDMatrix, Matrix4x4 threeDMatrix)> texturesToMatrices_ = new();

  public void Clear() => this.texturesToMatrices_.Clear();

  public void CalculateMatrices(
      IReadOnlyList<IReadOnlyTexture> textures,
      (IReadOnlyModelAnimation, float)? animationAndFrame) {
    var animation = animationAndFrame?.Item1;
    var frame = animationAndFrame?.Item2;

    // Intentionally looping by index to avoid allocating an enumerator.
    for (var i = 0; i < textures.Count; ++i) {
      var texture = textures[i];
      Vector3? animationTranslation = null;
      Vector3? animationRotation = null;
      Vector3? animationScale = null;

      // The pose of the animation, if available.
      IReadOnlyTextureTracks? textureTracks = null;
      animation?.TextureTracks.TryGetValue(texture, out textureTracks);
      if (textureTracks != null) {
        // Only gets the values from the animation if the frame is at least partially defined.
        if (textureTracks.Translations?.HasAnyData ?? false) {
          if (textureTracks.Translations.TryGetAtFrame(
                  frame.Value,
                  out var outAnimationTranslation)) {
            animationTranslation = outAnimationTranslation;
          }
        }

        if (textureTracks.Rotations?.HasAnyData ?? false) {
          if (textureTracks.Rotations.TryGetAtFrame(
                  frame.Value,
                  out var outAnimationRotation)) {
            animationRotation = outAnimationRotation.ToEulerRadians();
          }
        }

        if (textureTracks.Scales?.HasAnyData ?? false) {
          if (textureTracks.Scales.TryGetAtFrame(
                  frame.Value,
                  out var outAnimationScale)) {
            animationScale = outAnimationScale;
          }
        }
      }

      // Uses the animation pose instead of the root pose when available.
      var transform = texture.TextureTransform;
      var center = transform.Center;
      var translation = animationTranslation ?? transform.Translation;
      var rotation = animationRotation ?? transform.RotationRadians;
      var scale = animationScale ?? transform.Scale;

      var isTransform3d = transform.IsTransform3d;

      if (isTransform3d) {
        this.texturesToMatrices_[texture] = (
            false,
            default,
            CalculateTextureTransform3d_(texture,
                                         center,
                                         translation,
                                         rotation,
                                         scale));
      } else {
        this.texturesToMatrices_[texture] = (
            true,
            CalculateTextureTransform2d_(texture,
                                         center,
                                         translation,
                                         rotation,
                                         scale),
            default);
      }
    }
  }

  private static Matrix3x2 CalculateTextureTransform2d_(
      IReadOnlyTexture texture,
      Vector3? textureCenter,
      Vector3? textureTranslation,
      Vector3? textureRotationRadians,
      Vector3? textureScale) {
    var scrollingTexture = texture as IScrollingTexture;

    if ((textureTranslation == null && scrollingTexture == null) &&
        textureScale == null &&
        textureRotationRadians == null) {
      return Matrix3x2.Identity;
    }

    var secondsSinceStart
        = (float) FrameTime.ElapsedTimeSinceApplicationOpened.TotalSeconds;

    Vector2? center = null;
    if (textureCenter != null) {
      center = textureCenter.Value.Xy();
    }

    Vector2? translation = null;
    if (textureTranslation != null || scrollingTexture != null) {
      translation = new Vector2((textureTranslation?.X ?? 0) +
                                secondsSinceStart *
                                (scrollingTexture?.ScrollSpeedX ?? 0),
                                (textureTranslation?.Y ?? 0) +
                                secondsSinceStart *
                                (scrollingTexture?.ScrollSpeedY ?? 0));
    }

    Vector2? scale = null;
    if (textureScale != null) {
      scale = textureScale.Value.Xy();
    }

    return SystemMatrix3x2Util.FromCtrss(center,
                                         translation,
                                         textureRotationRadians?.Z,
                                         scale,
                                         null);
  }

  private static Matrix4x4 CalculateTextureTransform3d_(
      IReadOnlyTexture texture,
      Vector3? textureCenter,
      Vector3? textureTranslation,
      Vector3? textureRotationRadians,
      Vector3? textureScale) {
    var scrollingTexture = texture as IScrollingTexture;

    if ((textureTranslation == null && scrollingTexture == null) &&
        textureScale == null &&
        textureRotationRadians == null) {
      return Matrix4x4.Identity;
    }

    var secondsSinceStart
        = (float) FrameTime.ElapsedTimeSinceApplicationOpened.TotalSeconds;

    Vector3? translation = null;
    if (textureTranslation != null || scrollingTexture != null) {
      translation = new Vector3((textureTranslation?.X ?? 0) +
                                secondsSinceStart *
                                (scrollingTexture?.ScrollSpeedX ?? 0),
                                (textureTranslation?.Y ?? 0) +
                                secondsSinceStart *
                                (scrollingTexture?.ScrollSpeedY ?? 0),
                                textureTranslation?.Z ?? 0);
    }

    Quaternion? rotation = null;
    if (textureRotationRadians != null) {
      rotation = QuaternionUtil.CreateZyxRadians(textureRotationRadians.Value.X,
                                          textureRotationRadians.Value.Y,
                                          textureRotationRadians.Value.Z);
    }

    return SystemMatrix4x4Util.FromCtrs(textureCenter, translation, rotation, textureScale);
  }

  public (bool is2d, Matrix3x2 twoDMatrix, Matrix4x4 threeDMatrix)? GetMatrix(
      IReadOnlyTexture texture) => this.texturesToMatrices_[texture];
}