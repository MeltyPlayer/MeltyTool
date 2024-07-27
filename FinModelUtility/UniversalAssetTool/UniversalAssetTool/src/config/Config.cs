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
  private static Config? instance_;

  public static Config Instance {
    get {
      if (instance_ == null) {
        ReloadSettings();
      }

      return instance_!;
    }
  }

  public GeneralSettings General { get; } = new();
  public ExporterSettings Exporter { get; } = new();
  public ExtractorSettings Extractor { get; } = new();
  public ViewerSettings Viewer { get; } = new();

  public static void ReloadSettings()
    => instance_ = DirectoryConstants.CONFIG_FILE.Deserialize<Config>();

  public static void SaveSettings()
    => DirectoryConstants.CONFIG_FILE.Serialize(Config.Instance);
}

public class GeneralSettings {
  public DebugSettings Debug { get; } = new();

  [AutoInterface]
  public class DebugSettings : IDebugSettings {
    public bool VerboseConsole { get; set; }
  }
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
  public bool CacheFileHierarchies { get; set; }
  public bool ExtractRomsInParallel { get; set; }
}

public class ExporterSettings {
  public ExporterGeneralSettings General { get; } = new();
  public ExporterThirdPartySettings ThirdParty { get; } = new();

  [AutoInterface]
  public class ExporterGeneralSettings : IExporterGeneralSettings {
    public string[] ExportedFormats { get; set; } = [];
    public bool ExportAllTextures { get; set; }

    [JsonConverter(typeof(StringEnumConverter))]
    public ScaleSourceType ExportedModelScaleSource { get; set; } =
      ScaleSourceType.NONE;
  }

  [AutoInterface]
  public class ExporterThirdPartySettings : IExporterThirdPartySettings {
    public bool ExportBoneScaleAnimationsSeparately { get; set; }
  }
}