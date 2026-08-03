using System;
using System.Collections.Generic;

using ConfigFactory.Core;
using ConfigFactory.Core.Attributes;

using uni.config;


namespace uni.ui.avalonia.settings;

public sealed class SettingsViewModel
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

  public event Action? OnClose;


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
      Header = "Verbose console",
      Description = "Whether to print verbose logs to the console.")]
  public bool VerboseConsole {
    get => Config_.General.Debug.VerboseConsole;
    set => Config_.General.Debug.VerboseConsole = value;
  }

  // Exporter Settings
  [property: Config(
      Category = CATEGORY_EXPORTER,
      Group = GROUP_EXPORTER_GENERAL,
      Header = "Export all textures",
      Description
          = "Whether to export all textures read from the format, even if " +
            "they're not used in the exported model.")]
  public bool ExportAllTextures {
    get => Config_.Exporter.General.ExportAllTextures;
    set => Config_.Exporter.General.ExportAllTextures = value;
  }

  [property: Config(
      Category = CATEGORY_EXPORTER,
      Group = GROUP_EXPORTER_GENERAL,
      Header = "Exported formats",
      Description = "Which model formats to export.")]
  public HashSet<ExportedFormat> ExportedFormats {
    get => Config_.Exporter.General.ExportedFormats;
    set => Config_.Exporter.General.ExportedFormats = value;
  }

  [property: Config(
      Category = CATEGORY_EXPORTER,
      Group = GROUP_EXPORTER_GENERAL,
      Header = "Exported model scale source",
      Description = "How models should be scaled when exporting.")]
  public ScaleSourceType ExportedModelScaleSource {
    get => Config_.Exporter.General.ExportedModelScaleSource;
    set => Config_.Exporter.General.ExportedModelScaleSource = value;
  }

  // Third Party Settings
  [property: Config(
      Category = CATEGORY_EXPORTER,
      Group = GROUP_EXPORTER_THIRD_PARTY,
      Header = "Export bone scale animations separately",
      Description
          = "Whether to export bone scale animations in a separate file.")]
  public bool ExportBoneScaleAnimationsSeparately {
    get => Config_.Exporter.ThirdParty.ExportBoneScaleAnimationsSeparately;
    set => Config_.Exporter.ThirdParty.ExportBoneScaleAnimationsSeparately
        = value;
  }

  public const string CACHE_FILE_HIERARCHIES_HEADER = "Cache file hierarchies";

  public const string CACHE_FILE_HIERARCHIES_DESCRIPTION
      = "Whether to cache extracted file hierarchies. Reading the file " +
        "hierarchy from a cache file is significantly faster than " +
        "querying the file system, so this will decrease latency when " +
        "starting up the UI.";

  // Extractor Settings
  [property: Config(
      Category = GROUP_EXTRACTOR,
      Header = CACHE_FILE_HIERARCHIES_HEADER,
      Description = CACHE_FILE_HIERARCHIES_DESCRIPTION)]
  public bool CacheFileHierarchies {
    get => Config_.Extractor.CacheFileHierarchies;
    set => Config_.Extractor.CacheFileHierarchies = value;
  }

  public const string CLEAN_UP_ARCHIVES_HEADER = "Clean up archives";

  public const string CLEAN_UP_ARCHIVES_DESCRIPTION
      = "Whether to clean up archives after extracting them. This helps " +
        "reduce memory usage and prevents redundant work the next time " +
        "the UI starts up.";

  [property: Config(
      Category = GROUP_EXTRACTOR,
      Header = CLEAN_UP_ARCHIVES_HEADER,
      Description = CLEAN_UP_ARCHIVES_DESCRIPTION)]
  public bool CleanUpArchives {
    get => Config_.Extractor.CleanUpArchives;
    set => Config_.Extractor.CleanUpArchives = value;
  }

  public const string EXTRACT_ROMS_IN_PARALLEL_HEADER
      = "Extract ROMs in parallel";

  public const string EXTRACT_ROMS_IN_PARALLEL_DESCRIPTION
      = "Whether to extract ROMs in parallel using multithreading.";

  [property: Config(
      Category = GROUP_EXTRACTOR,
      Header = EXTRACT_ROMS_IN_PARALLEL_HEADER,
      Description = EXTRACT_ROMS_IN_PARALLEL_DESCRIPTION)]
  public bool ExtractRomsInParallel {
    get => Config_.Extractor.ExtractRomsInParallel;
    set => Config_.Extractor.ExtractRomsInParallel = value;
  }

  public const string VERIFY_CACHED_FILE_HIERARCHY_SIZE_HEADER
      = "Verify cached file hierarchy size";

  public const string VERIFY_CACHED_FILE_HIERARCHY_SIZE_DESCRIPTION
      = "Whether to verify the total directory size of cached file " +
        "hierarchies. This will allow the UI to automatically " +
        "regenerate out-of-date cached file hierarchies, but will " +
        "increase latency when starting up the UI.";

  [property: Config(
      Category = GROUP_EXTRACTOR,
      Header = VERIFY_CACHED_FILE_HIERARCHY_SIZE_HEADER,
      Description = VERIFY_CACHED_FILE_HIERARCHY_SIZE_DESCRIPTION)]
  public bool VerifyCachedFileHierarchySize {
    get => Config_.Extractor.VerifyCachedFileHierarchySize;
    set => Config_.Extractor.VerifyCachedFileHierarchySize = value;
  }

  // Viewer Settings
  [property: Config(
      Category = GROUP_VIEWER,
      Header = "Automatically play game audio for model",
      Description
          = "Whether to automatically play audio from the current game when " +
            "viewing a model.")]
  public bool AutomaticallyPlayGameAudioForModel {
    get => Config_.Viewer.AutomaticallyPlayGameAudioForModel;
    set => Config_.Viewer.AutomaticallyPlayGameAudioForModel = value;
  }

  [property: Config(
      Category = GROUP_VIEWER,
      Header = "Prefer GL native interop",
      Description
          = "Whether to prefer native interop for OpenGL. This will " +
            "significantly improve performance, but isn't always available. " +
            "Might cause some graphical issues and/or crashes, there are " +
            "still some issues to resolve.")]
  public bool PreferGlNativeInterop {
    get => Config_.Viewer.PreferGlNativeInterop;
    set => Config_.Viewer.PreferGlNativeInterop = value;
  }

  [property: Config(
      Category = GROUP_VIEWER,
      Header = "Show grid",
      Description = "Whether to render the grid in the 3D view.")]
  public bool ShowGrid {
    get => Config_.Viewer.ShowGrid;
    set => Config_.Viewer.ShowGrid = value;
  }

  [property: Config(
      Category = GROUP_VIEWER,
      Header = "Show skeleton",
      Description = "Whether to render the skeleton in the 3D view.")]
  public bool ShowSkeleton {
    get => Config_.Viewer.ShowSkeleton;
    set => Config_.Viewer.ShowSkeleton = value;
  }

  [property: Config(
      Category = GROUP_VIEWER,
      Header = "Viewer model scale source",
      Description = "How models should be scaled within the 3D view.")]
  public ScaleSourceType ViewerModelScaleSource {
    get => Config_.Viewer.ViewerModelScaleSource;
    set => Config_.Viewer.ViewerModelScaleSource = value;
  }
}