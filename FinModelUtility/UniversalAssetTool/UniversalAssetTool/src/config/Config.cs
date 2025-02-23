using fin.common;
using fin.config;
using fin.data.dictionaries;
using fin.io;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using schema.autoInterface;

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
    => DirectoryConstants.CONFIG_FILE.Serialize(Instance);
}

public partial class GeneralSettings {
  public DebugSettings Debug { get; } = new();

  [GenerateInterface]
  public partial class DebugSettings {
    public bool VerboseConsole { get; set; }
  }
}

[GenerateInterface]
public partial class ViewerSettings {
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

[GenerateInterface]
public partial class ExtractorSettings {
  public bool CacheFileHierarchies {
    get => FinConfig.CacheFileHierarchies;
    set => FinConfig.CacheFileHierarchies = value;
  }
  public bool ExtractRomsInParallel { get; set; }

  public bool VerifyCachedFileHierarchySize {
    get => FinConfig.VerifyCachedFileHierarchySize;
    set => FinConfig.VerifyCachedFileHierarchySize = value;
  }
}

[JsonConverter(typeof(StringEnumConverter))]
public enum ExportedFormat {
  DAE = 1,
  FBX,
  GLB,
  GLTF,
  OBJ,
}

public static class ExportedFormatUtil {
  public static readonly BidirectionalDictionary<ExportedFormat, string> map_
      = new() {
          [ExportedFormat.DAE] = ".dae",
          [ExportedFormat.FBX] = ".fbx",
          [ExportedFormat.GLB] = ".glb",
          [ExportedFormat.GLTF] = ".gltf",
          [ExportedFormat.OBJ] = ".obj",
      };

  public static string AsFileType(this ExportedFormat format) => map_[format];
  public static ExportedFormat AsFormat(this string fileType) => map_[fileType];
}

public partial class ExporterSettings {
  public ExporterGeneralSettings General { get; } = new();
  public ExporterThirdPartySettings ThirdParty { get; } = new();

  [GenerateInterface]
  public partial class ExporterGeneralSettings {
    public HashSet<ExportedFormat> ExportedFormats { get; set; } = [];
    public bool ExportAllTextures { get; set; }

    [JsonConverter(typeof(StringEnumConverter))]
    public ScaleSourceType ExportedModelScaleSource { get; set; } =
      ScaleSourceType.NONE;
  }

  [GenerateInterface]
  public partial class ExporterThirdPartySettings {
    public bool ExportBoneScaleAnimationsSeparately { get; set; }
  }
}