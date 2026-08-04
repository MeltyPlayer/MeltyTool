using System.Collections.Generic;
using System.Linq;

using Avalonia.Controls;
using Avalonia.Media;

using fin.io.bundles;
using fin.ui;
using fin.util.progress;

using ReactiveUI;

using uni.config;
using uni.games;

namespace uni.ui.avalonia.settings;

using ExtractorWhichNeedsConfiguration
    = (INamedFileBundleGatherer gatherer, bool stillNeedsToBeConfigured);

public class FileBundleGathererEnablement(
    INamedFileBundleGatherer gatherer,
    bool stillNeedsToBeConfigured,
    bool defaultIsEnabled) : BViewModel {
  public INamedFileBundleGatherer Gatherer => gatherer;
  public bool StillNeedsToBeConfigured => stillNeedsToBeConfigured;

  public bool IsEnabled {
    get;
    set => this.RaiseAndSetIfChanged(ref field, value);
  } = defaultIsEnabled;

  public FontWeight FontWeight => this.StillNeedsToBeConfigured
      ? FontWeight.Bold
      : FontWeight.Normal;
}

public class FileBundleGathererSelectorWindowViewModelForDesigner
    : FileBundleGathererSelectorWindowViewModel {
  private record StubFileBundleGatherer(
      string Name,
      FileBundleGathererPlatform Platform,
      bool IsAvailable)
      : INamedFileBundleGatherer {
    public void GatherFileBundles(
        IFileBundleOrganizer organizer,
        IMutablePercentageProgress mutablePercentageProgress)
      => throw new System.NotImplementedException();
  }

  public FileBundleGathererSelectorWindowViewModelForDesigner() : base(
  [
      (new StubFileBundleGatherer("desktop_1",
                                  FileBundleGathererPlatform.DESKTOP,
                                  true), true),
      (new StubFileBundleGatherer("snes_2",
                                  FileBundleGathererPlatform.SNES,
                                  false), false),
      (new StubFileBundleGatherer("n64_3",
                                  FileBundleGathererPlatform.N64,
                                  false), false),
      (new StubFileBundleGatherer("gamecube_4",
                                  FileBundleGathererPlatform.GAMECUBE,
                                  true), false),
      (new StubFileBundleGatherer("ds_5",
                                  FileBundleGathererPlatform.DS,
                                  true), true),
      (new StubFileBundleGatherer("wii_6",
                                  FileBundleGathererPlatform.WII,
                                  true), false),
      (new StubFileBundleGatherer("3ds_7",
                                  FileBundleGathererPlatform.THREE_DS,
                                  true), false)
  ]) { }
}

public class FileBundleGathererSelectorWindowViewModel : BViewModel {
  public FileBundleGathererSelectorWindowViewModel()
      : this(ExtractorUtil.GetExtractorsWhichNeedConfiguration()) { }

  public FileBundleGathererSelectorWindowViewModel(
      IEnumerable<ExtractorWhichNeedsConfiguration>
          extractorsWhichNeedConfiguration) {
    var configuredGamesToExtract = Config_.Extractor.GamesToExtract ??
                                   new Dictionary<string, bool>();

    this.FileBundleGathererEnablements
        = extractorsWhichNeedConfiguration
          .Select(t => new FileBundleGathererEnablement(
                      t.gatherer,
                      t.stillNeedsToBeConfigured,
                      !configuredGamesToExtract.TryGetValue(
                          t.gatherer.Name,
                          out var configuredValue) || configuredValue))
          .Where(t => t.Gatherer.IsListed)
          .OrderBy(t => t.Gatherer.IsAvailable)
          .ThenBy(t => t.Gatherer.Name)
          .ToArray();
  }

  private static Config Config_ => Config.Instance;

  public string CacheFileHierarchiesHeader { get; }
    = SettingsViewModel.CACHE_FILE_HIERARCHIES_HEADER;

  public string CacheFileHierarchiesDescription { get; }
    = SettingsViewModel.CACHE_FILE_HIERARCHIES_DESCRIPTION;

  public string CleanUpArchivesHeader { get; }
    = SettingsViewModel.CLEAN_UP_ARCHIVES_HEADER;

  public string CleanUpArchivesDescription { get; }
    = SettingsViewModel.CLEAN_UP_ARCHIVES_DESCRIPTION;

  public string ExtractRomsInParallelHeader { get; }
    = SettingsViewModel.EXTRACT_ROMS_IN_PARALLEL_HEADER;

  public string ExtractRomsInParallelDescription { get; }
    = SettingsViewModel.EXTRACT_ROMS_IN_PARALLEL_DESCRIPTION;

  public string VerifyCachedFileHierarchySizeHeader { get; }
    = SettingsViewModel.VERIFY_CACHED_FILE_HIERARCHY_SIZE_HEADER;

  public string VerifyCachedFileHierarchySizeDescription { get; }
    = SettingsViewModel.VERIFY_CACHED_FILE_HIERARCHY_SIZE_DESCRIPTION;

  public IReadOnlyList<FileBundleGathererEnablement>
      FileBundleGathererEnablements {
    get;
    set { this.RaiseAndSetIfChanged(ref field, value); }
  }
}

public partial class FileBundleGathererSelectorWindow : Window {
  public FileBundleGathererSelectorWindow() {
    InitializeComponent();
  }
}