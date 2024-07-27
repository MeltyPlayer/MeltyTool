using Avalonia.SettingsFactory.Core;

using uni.config;

namespace uni.ui.avalonia.settings;

public class SettingsViewModel
    : ISettingsBase, IDebugSettings, IExporterSettings, IExtractorSettings,
      IThirdPartySettings, IViewerSettings {
  private const string CATEGORY_DEBUG = "Debug";
  private const string CATEGORY_EXPORTER = "Exporter";
  private const string CATEGORY_EXTRACTOR = "Extractor";
  private const string CATEGORY_THIRD_PARTY = "Third Party";
  private const string CATEGORY_VIEWER = "Viewer";
  
  private static Config Config_ => Config.Instance;

  public bool RequiresInput { get; set; }

  public ISettingsBase Save() {
    Config_.SaveSettings();
    return this;
  }

  // Debug Settings
  [Setting("Verbose Console",
           "Whether to print verbose logs to the console.",
           Category = CATEGORY_DEBUG)]
  public bool VerboseConsole {
    get => Config_.DebugSettings.VerboseConsole;
    set => Config_.DebugSettings.VerboseConsole = value;
  }

  // Exporter Settings
  [Setting("Export All Textures",
           "Whether to export all textures read from the format, even if they're not used in the exported model.",
           Category = CATEGORY_EXPORTER)]
  public bool ExportAllTextures {
    get => Config_.ExporterSettings.ExportAllTextures;
    set => Config_.ExporterSettings.ExportAllTextures = value;
  }

  [Setting("Exported Formats",
           "Which model formats to export.",
           Category = CATEGORY_EXPORTER)]
  public string[] ExportedFormats {
    get => Config_.ExporterSettings.ExportedFormats;
    set => Config_.ExporterSettings.ExportedFormats = value;
  }

  [Setting("Exported Model Scale Source",
           "How models should be scaled when exporting.",
           Category = CATEGORY_EXPORTER)]
  public ScaleSourceType ExportedModelScaleSource {
    get => Config_.ExporterSettings.ExportedModelScaleSource;
    set => Config_.ExporterSettings.ExportedModelScaleSource = value;
  }

  // Extractor Settings
  [Setting("Use Multithreading to Extract ROMs",
           "Whether files should be extracted from ROMs in parallel via multithreading.",
           Category = CATEGORY_EXTRACTOR)]
  public bool UseMultithreadingToExtractRoms {
    get => Config_.ExtractorSettings.UseMultithreadingToExtractRoms;
    set => Config_.ExtractorSettings.UseMultithreadingToExtractRoms = value;
  }

  // Third Party Settings
  [Setting("Export Bone Scale Animations Separately",
           "Whether to export bone scale animations in a separate file.",
           Category = CATEGORY_THIRD_PARTY)]
  public bool ExportBoneScaleAnimationsSeparately {
    get => Config_.ThirdPartySettings.ExportBoneScaleAnimationsSeparately;
    set => Config_.ThirdPartySettings.ExportBoneScaleAnimationsSeparately
        = value;
  }

  // Viewer Settings
  [Setting("Automatically Play Game Audio For Model",
           "Whether to automatically play audio from the current game when viewing a model.",
           Category = CATEGORY_VIEWER)]
  public bool AutomaticallyPlayGameAudioForModel {
    get => Config_.ViewerSettings.AutomaticallyPlayGameAudioForModel;
    set => Config_.ViewerSettings.AutomaticallyPlayGameAudioForModel = value;
  }

  [Setting("Show Grid",
           "Whether to render the grid in the 3D view.",
           Category = CATEGORY_VIEWER)]
  public bool ShowGrid {
    get => Config_.ViewerSettings.ShowGrid;
    set => Config_.ViewerSettings.ShowGrid = value;
  }

  [Setting("Show Skeleton",
           "Whether to render the skeleton in the 3D view.",
           Category = CATEGORY_VIEWER)]
  public bool ShowSkeleton {
    get => Config_.ViewerSettings.ShowSkeleton;
    set => Config_.ViewerSettings.ShowSkeleton = value;
  }

  [Setting("Viewer Model Scale Source",
           "How models should be scaled when rendering.",
           Category = CATEGORY_VIEWER)]
  public ScaleSourceType ViewerModelScaleSource {
    get => Config_.ViewerSettings.ViewerModelScaleSource;
    set => Config_.ViewerSettings.ViewerModelScaleSource = value;
  }
}