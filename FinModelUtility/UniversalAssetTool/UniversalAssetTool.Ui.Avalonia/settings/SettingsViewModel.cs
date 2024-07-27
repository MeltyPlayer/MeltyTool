using ConfigFactory.Core;
using ConfigFactory.Core.Attributes;

using uni.config;

namespace uni.ui.avalonia.settings;

public partial class SettingsViewModel
    : ConfigModule<SettingsViewModel>, IDebugSettings, IExporterSettings,
      IExtractorSettings, IThirdPartySettings, IViewerSettings {
  private const string CATEGORY_DEBUG = "Debug";
  private const string CATEGORY_EXPORTER = "Exporter";
  private const string CATEGORY_EXTRACTOR = "Extractor";
  private const string CATEGORY_THIRD_PARTY = "Third Party";
  private const string CATEGORY_VIEWER = "Viewer";

  private static Config Config_ => Config.Instance;

  // Debug Settings
  [property: Config(
      Category = CATEGORY_DEBUG,
      Header = "Verbose Console",
      Description = "Whether to print verbose logs to the console.")]
  public bool VerboseConsole {
    get => Config_.DebugSettings.VerboseConsole;
    set => Config_.DebugSettings.VerboseConsole = value;
  }

  // Exporter Settings
  [property: Config(
      Category = CATEGORY_EXPORTER,
      Header = "Export All Textures",
      Description
          = "Whether to export all textures read from the format, even if they're not used in the exported model.")]
  public bool ExportAllTextures {
    get => Config_.ExporterSettings.ExportAllTextures;
    set => Config_.ExporterSettings.ExportAllTextures = value;
  }

  [property: Config(
      Category = CATEGORY_EXPORTER,
      Header = "Exported Formats",
      Description = "Which model formats to export.")]
  public string[] ExportedFormats {
    get => Config_.ExporterSettings.ExportedFormats;
    set => Config_.ExporterSettings.ExportedFormats = value;
  }

  [property: Config(
      Category = CATEGORY_EXPORTER,
      Header = "Exported Model Scale Source",
      Description = "How models should be scaled when exporting.")]
  public ScaleSourceType ExportedModelScaleSource {
    get => Config_.ExporterSettings.ExportedModelScaleSource;
    set => Config_.ExporterSettings.ExportedModelScaleSource = value;
  }

  // Extractor Settings
  [property: Config(
      Category = CATEGORY_EXTRACTOR,
      Header = "Use Multithreading to Extract ROMs",
      Description
          = "Whether files should be extracted from ROMs in parallel via multithreading.")]
  public bool UseMultithreadingToExtractRoms {
    get => Config_.ExtractorSettings.UseMultithreadingToExtractRoms;
    set => Config_.ExtractorSettings.UseMultithreadingToExtractRoms = value;
  }

  // Third Party Settings
  [property: Config(
      Category = CATEGORY_THIRD_PARTY,
      Header = "Export Bone Scale Animations Separately",
      Description
          = "Whether to export bone scale animations in a separate file.")]
  public bool ExportBoneScaleAnimationsSeparately {
    get => Config_.ThirdPartySettings.ExportBoneScaleAnimationsSeparately;
    set => Config_.ThirdPartySettings.ExportBoneScaleAnimationsSeparately
        = value;
  }

  // Viewer Settings
  [property: Config(
      Category = CATEGORY_VIEWER,
      Header = "Automatically Play Game Audio For Model",
      Description
          = "Whether to automatically play audio from the current game when viewing a model.")]
  public bool AutomaticallyPlayGameAudioForModel {
    get => Config_.ViewerSettings.AutomaticallyPlayGameAudioForModel;
    set => Config_.ViewerSettings.AutomaticallyPlayGameAudioForModel = value;
  }

  [property: Config(
      Category = CATEGORY_VIEWER,
      Header = "Show Grid",
      Description = "Whether to render the grid in the 3D view.")]
  public bool ShowGrid {
    get => Config_.ViewerSettings.ShowGrid;
    set => Config_.ViewerSettings.ShowGrid = value;
  }

  [property: Config(
      Category = CATEGORY_VIEWER,
      Header = "Show Skeleton",
      Description = "Whether to render the skeleton in the 3D view.")]
  public bool ShowSkeleton {
    get => Config_.ViewerSettings.ShowSkeleton;
    set => Config_.ViewerSettings.ShowSkeleton = value;
  }

  [property: Config(
      Category = CATEGORY_VIEWER,
      Header = "Viewer Model Scale Source",
      Description = "How models should be scaled when rendering.")]
  public ScaleSourceType ViewerModelScaleSource {
    get => Config_.ViewerSettings.ViewerModelScaleSource;
    set => Config_.ViewerSettings.ViewerModelScaleSource = value;
  }
}