using AutoInterfaceAttributes;

using fin.config;
using fin.io;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using uni.platforms;

namespace uni.config;

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

[AutoInterface]
public class ViewerSettings : IViewerSettings {
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

[AutoInterface]
public class ExtractorSettings : IExtractorSettings {
  public bool UseMultithreadingToExtractRoms { get; set; }
}

[AutoInterface]
public class ExporterSettings : IExporterSettings {
  public string[] ExportedFormats { get; set; } = Array.Empty<string>();
  public bool ExportAllTextures { get; set; }

  [JsonConverter(typeof(StringEnumConverter))]
  public ScaleSourceType ExportedModelScaleSource { get; set; } =
    ScaleSourceType.NONE;
}

[AutoInterface]
public class DebugSettings : IDebugSettings {
  public bool VerboseConsole { get; set; }
}

[AutoInterface]
public class ThirdPartySettings : IThirdPartySettings {
  public bool ExportBoneScaleAnimationsSeparately { get; set; }
}