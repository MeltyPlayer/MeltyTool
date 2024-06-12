using fin.config;
using fin.io;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using uni.platforms;

namespace uni.config {
  public enum ScaleSourceType {
    NONE,
    MIN_MAX_BOUNDS,
    GAME_CONFIG,
  }

  public class Config {
    public static Config Instance { get; } =
      DirectoryConstants.CONFIG_FILE.Deserialize<Config>();

    public ExporterSettings ExporterSettings { get; } = new();
    public ExtractorSettings ExtractorSettings { get; } = new();
    public ViewerSettings ViewerSettings { get; } = new();
    public ThirdPartySettings ThirdPartySettings { get; } = new();
    public DebugSettings DebugSettings { get; } = new();

    public void SaveSettings()
      => DirectoryConstants.CONFIG_FILE.Serialize(Config.Instance);
  }

  public class ViewerSettings {
    public bool AutomaticallyPlayGameAudioForModel { get; set; }

    public bool ShowGrid { get; set; }

    public bool ShowSkeleton {
      get => FinConfig.ShowSkeleton;
      set => FinConfig.ShowSkeleton = value;
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public ScaleSourceType ViewerModelScaleSource { get; set; } =
      ScaleSourceType.MIN_MAX_BOUNDS;
  }

  public class ExtractorSettings {
    public bool UseMultithreadingToExtractRoms { get; set; }
  }

  public class ExporterSettings {
    public string[] ExportedFormats { get; set; } = Array.Empty<string>();
    public bool ExportAllTextures { get; set; }

    [JsonConverter(typeof(StringEnumConverter))]
    public ScaleSourceType ExportedModelScaleSource { get; set; } =
      ScaleSourceType.NONE;
  }

  public class DebugSettings {
    public bool VerboseConsole { get; set; }
  }

  public class ThirdPartySettings {
    public bool ExportBoneScaleAnimationsSeparately { get; set; }
  }
}