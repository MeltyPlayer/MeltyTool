using System.Numerics;

using fin.animation.keyframes;
using fin.image.effects;
using fin.model;

using marioartist.schema.talent_studio.face;

namespace marioartist.api;

/// <summary>
///   Super annoying, but we should do the warp in software for deterministic
///   test results.
/// </summary>
public static class TstltExpressionImageGenerator {
  public static void GenerateExpressionTextures(
      IMaterialManager dstMaterialManager,
      IAnimationManager dstAnimationManager,
      IReadOnlyTexture baseFaceTexture,
      Expression[] expressions) {
    var baseFaceImage = baseFaceTexture.Image;

    var meshWarp = new MeshWarp(
        baseFaceImage.Width,
        baseFaceImage.Height,
        Expression.WIDTH,
        Expression.HEIGHT);

    var conversionFactor
        = new Vector2(baseFaceImage.Width, baseFaceImage.Height) /
          new Vector2(Expression.WIDTH - 1, Expression.HEIGHT - 1) /
          16;

    for (var i = 0; i < expressions.Length; ++i) {
      var expression = expressions[i];
      var expressionName = i switch {
          0 => "normal",
          1 => "laugh",
          2 => "angry",
          3 => "sad",
          4 => "free",
          5 => "sleep",
      };

      for (var xI = 0; xI < Expression.WIDTH; ++xI) {
        for (var yI = 0; yI < Expression.HEIGHT; ++yI) {
          var expressionPin = expression.Pins[xI * Expression.HEIGHT + yI];

          expressionPin = (expressionPin - new Vector2(88, 24)) * conversionFactor;

          var meshWarpPin = meshWarp.Pins[xI, yI];
          meshWarpPin.Position = expressionPin;
        }
      }

      var expressionImage = meshWarp.WarpImage(baseFaceImage);

      var expressionTexture
          = dstMaterialManager.CreateTexture(expressionImage);
      expressionTexture.Name = expressionName;

      var expressionAnimation = dstAnimationManager.AddAnimation();
      expressionAnimation.Name = expressionName;

      var textureTracks = expressionAnimation.AddTextureTracks(baseFaceTexture);
      textureTracks.UseFlipbookSwapKeyframes()
                   .SetKeyframe(0, expressionTexture);
    }
  }
}