using System;
using System.Collections.Generic;

using ConfigFactory.Core;
using ConfigFactory.Core.Attributes;

using uni.config;


namespace uni.ui.avalonia.settings;

public class SettingsViewModel
    : ConfigModule<SettingsViewModel>,
      GeneralSettings.IDebugSettings,
      ExporterSettings.IExporterGeneralSettings,
      ExporterSettings.IExporterThirdPartySettings,
      IExtractorSettings,
      IViewerSettings {
  private const string CATEGORY_GENERAL = "General";
  private const string GROUP_GENERAL_DEBUG = "Debug";

  private const string CATEGORY_EXPORTER = "Exporter";
  private const string GROUP_EXPORTER_GENERAL = "General";
  private const string GROUP_EXPORTER_THIRD_PARTY = "Third Party";

  private const string GROUP_EXTRACTOR = "Extractor";
  private const string GROUP_VIEWER = "Viewer";

  private static Config Config_ => Config.Instance;

  public event Action OnClose;


  public override void Reset() {
    base.Reset();
    Config.ReloadSettings();
    this.OnClose?.Invoke();
  }

  public override void Save() {
    base.Save();
    Config.SaveSettings();
    this.OnClose?.Invoke();
  }

  // Debug Settings
  [property: Config(
      Category = CATEGORY_GENERAL,
      Group = GROUP_GENERAL_DEBUG,
      Header = "Verbose Console",
      Description = "Whether to print verbose logs to the console.")]
  public bool VerboseConsole {
    get => Config_.General.Debug.VerboseConsole;
    set => Config_.General.Debug.VerboseConsole = value;
  }

  // Exporter Settings
  [property: Config(
      Category = CATEGORY_EXPORTER,
      Group = GROUP_EXPORTER_GENERAL,
      Header = "Export All Textures",
      Description
          = "Whether to export all textures read from the format, even if they're not used in the exported model.")]
  public bool ExportAllTextures {
    get => Config_.Exporter.General.ExportAllTextures;
    set => Config_.Exporter.General.ExportAllTextures = value;
  }

  [property: Config(
      Category = CATEGORY_EXPORTER,
      Group = GROUP_EXPORTER_GENERAL,
      Header = "Exported Formats",
      Description = "Which model formats to export.")]
  public HashSet<ExportedFormat> ExportedFormats {
    get => Config_.Exporter.General.ExportedFormats;
    set => Config_.Exporter.General.ExportedFormats = value;
  }

  [property: Config(
      Category = CATEGORY_EXPORTER,
      Group = GROUP_EXPORTER_GENERAL,
      Header = "Exported Model Scale Source",
      Description = "How models should be scaled when exporting.")]
  public ScaleSourceType ExportedModelScaleSource {
    get => Config_.Exporter.General.ExportedModelScaleSource;
    set => Config_.Exporter.General.ExportedModelScaleSource = value;
  }

  // Third Party Settings
  [property: Config(
      Category = CATEGORY_EXPORTER,
      Group = GROUP_EXPORTER_THIRD_PARTY,
      Header = "Export Bone Scale Animations Separately",
      Description
          = "Whether to export bone scale animations in a separate file.")]
  public bool ExportBoneScaleAnimationsSeparately {
    get => Config_.Exporter.ThirdParty.ExportBoneScaleAnimationsSeparately;
    set => Config_.Exporter.ThirdParty.ExportBoneScaleAnimationsSeparately
        = value;
  }

  // Extractor Settings
  [property: Config(
      Category = GROUP_EXTRACTOR,
      Header = "Cache File Hierarchies",
      Description
          = "Whether to cache extracted file hierarchies. Reading the file hierarchy from a structure is faster than querying the file system, so this will speed up future launches.")]
  public bool CacheFileHierarchies {
    get => Config_.Extractor.CacheFileHierarchies;
    set => Config_.Extractor.CacheFileHierarchies = value;
  }

  [property: Config(
      Category = GROUP_EXTRACTOR,
      Header = "Extract ROMs in Parallel",
      Description
          = "Whether to extract ROMs in parallel using multithreading.")]
  public bool ExtractRomsInParallel {
    get => Config_.Extractor.ExtractRomsInParallel;
    set => Config_.Extractor.ExtractRomsInParallel = value;
  }

  // Viewer Settings
  [property: Config(
      Category = GROUP_VIEWER,
      Header = "Automatically Play Game Audio For Model",
      Description
          = "Whether to automatically play audio from the current game when viewing a model.")]
  public bool AutomaticallyPlayGameAudioForModel {
    get => Config_.Viewer.AutomaticallyPlayGameAudioForModel;
    set => Config_.Viewer.AutomaticallyPlayGameAudioForModel = value;
  }

  [property: Config(
      Category = GROUP_VIEWER,
      Header = "Show Grid",
      Description = "Whether to render the grid in the 3D view.")]
  public bool ShowGrid {
    get => Config_.Viewer.ShowGrid;
    set => Config_.Viewer.ShowGrid = value;
  }

  [property: Config(
      Category = GROUP_VIEWER,
      Header = "Show Skeleton",
      Description = "Whether to render the skeleton in the 3D view.")]
  public bool ShowSkeleton {
    get => Config_.Viewer.ShowSkeleton;
    set => Config_.Viewer.ShowSkeleton = value;
  }

  [property: Config(
      Category = GROUP_VIEWER,
      Header = "Viewer Model Scale Source",
      Description = "How models should be scaled when rendering.")]
  public ScaleSourceType ViewerModelScaleSource {
    get => Config_.Viewer.ViewerModelScaleSource;
    set => Config_.Viewer.ViewerModelScaleSource = value;
  }
}