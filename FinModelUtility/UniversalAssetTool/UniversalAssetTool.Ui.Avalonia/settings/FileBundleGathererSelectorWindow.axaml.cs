using System.Collections.Generic;
using System.Linq;

using Avalonia.Controls;

using fin.io.bundles;
using fin.ui;
using fin.util.progress;

using uni.games;

namespace uni.ui.avalonia.settings;

public class FileBundleGathererSelectorWindowViewModelForDesigner
    : BFileBundleGathererSelectorWindowViewModel {
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

  public override IReadOnlyList<INamedFileBundleGatherer> FileBundleGatherers {
    get;
  }
    = new[] {
          new StubFileBundleGatherer("desktop_1",
                                     FileBundleGathererPlatform.DESKTOP,
                                     true),
          new StubFileBundleGatherer("snes_2",
                                     FileBundleGathererPlatform.SNES,
                                     false),
          new StubFileBundleGatherer("n64_3",
                                     FileBundleGathererPlatform.N64,
                                     false),
          new StubFileBundleGatherer("gamecube_4",
                                     FileBundleGathererPlatform.GAMECUBE,
                                     true),
          new StubFileBundleGatherer("ds_5",
                                     FileBundleGathererPlatform.DS,
                                     true),
          new StubFileBundleGatherer("wii_6",
                                     FileBundleGathererPlatform.WII,
                                     true),
          new StubFileBundleGatherer("3ds_7",
                                     FileBundleGathererPlatform.THREE_DS,
                                     true),
      }
      .OrderBy(g => g.IsAvailable)
      .ToArray();
}

public class FileBundleGathererSelectorWindowViewModel
    : BFileBundleGathererSelectorWindowViewModel {
  public override IReadOnlyList<INamedFileBundleGatherer> FileBundleGatherers {
    get;
  }
    = ExtractorUtil.GetAllNamedFileBundleGatherers()
                   .Where(g => g.IsListed)
                   .OrderBy(g => g.IsAvailable)
                   .ToArray();
}

public abstract class BFileBundleGathererSelectorWindowViewModel : BViewModel {
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

  public abstract IReadOnlyList<INamedFileBundleGatherer> FileBundleGatherers {
    get;
  }
}

public partial class FileBundleGathererSelectorWindow : Window {
  public FileBundleGathererSelectorWindow() {
    InitializeComponent();
  }
}