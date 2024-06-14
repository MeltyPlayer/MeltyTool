using fin.config;
using fin.io;
using fin.io.bundles;
using fin.model;
using fin.model.util;
using fin.scene;

using uni.config;
using uni.platforms;

namespace uni.model {
  public interface IScaleSource {
    float GetScale(ISceneInstance scene, IFileBundle fileBundle);
    float GetScale(IReadOnlyModel model, IFileBundle fileBundle);
  }

  public class ScaleSource : IScaleSource {
    private readonly IScaleSource impl_;

    public ScaleSource(ScaleSourceType type) {
      this.impl_ = type switch {
          ScaleSourceType.NONE => new NullScaleSource(),
          ScaleSourceType.MIN_MAX_BOUNDS => new MinMaxBoundsScaleSource(),
          ScaleSourceType.GAME_CONFIG => new GameConfigScaleSource(),
          _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
      };
    }

    public float GetScale(ISceneInstance scene, IFileBundle fileBundle)
      => this.impl_.GetScale(scene, fileBundle);

    public float GetScale(IReadOnlyModel model, IFileBundle fileBundle)
      => this.impl_.GetScale(model, fileBundle);
  }

  public class NullScaleSource : IScaleSource {
    public float GetScale(ISceneInstance _1, IFileBundle _2) => 1;
    public float GetScale(IReadOnlyModel _1, IFileBundle _2) => 1;
  }

  public class MinMaxBoundsScaleSource : IScaleSource {
    public float GetScale(ISceneInstance scene, IFileBundle _)
      => new SceneMinMaxBoundsScaleCalculator().CalculateScale(scene);

    public float GetScale(IReadOnlyModel model, IFileBundle _)
      => new ModelMinMaxBoundsScaleCalculator().CalculateScale(model);
  }

  public class GameConfigScaleSource : IScaleSource {
    public float GetScale(ISceneInstance scene, IFileBundle fileBundle)
      => this.TryToGetScaleFromGameConfig_(fileBundle, out float scale)
          ? scale
          : 1;

    public float GetScale(IReadOnlyModel model, IFileBundle fileBundle)
      => this.TryToGetScaleFromGameConfig_(fileBundle, out float scale)
          ? scale
          : 1;

    private bool TryToGetScaleFromGameConfig_(IFileBundle fileBundle,
                                              out float scale) {
      var gameName = fileBundle.GameName;
      if (gameName != null &&
          DirectoryConstants.GAME_CONFIG_DIRECTORY.TryToGetExistingFile(
              $"{gameName}.json",
              out var gameConfigFile)) {
        var gameConfig = gameConfigFile.Deserialize<GameConfig>();
        scale = gameConfig.Scale;
        return true;
      }

      scale = 1;
      return false;
    }
  }
}