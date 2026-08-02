using System.Collections.Generic;
using System.Linq;

using Avalonia.Controls;

using fin.io.bundles;
using fin.ui;
using fin.util.progress;

using uni.games;

namespace uni.ui.avalonia.settings;

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

  public FileBundleGathererSelectorWindowViewModelForDesigner() {
    this.FileBundleGatherers
        = new[] {
              new StubFileBundleGatherer("desktop_1", FileBundleGathererPlatform.DESKTOP, true),
              new StubFileBundleGatherer("snes_2", FileBundleGathererPlatform.SNES, false),
              new StubFileBundleGatherer("n64_3", FileBundleGathererPlatform.N64, false),
              new StubFileBundleGatherer("gamecube_4", FileBundleGathererPlatform.GAMECUBE, true),
              new StubFileBundleGatherer("ds_5", FileBundleGathererPlatform.DS, true),
              new StubFileBundleGatherer("wii_6", FileBundleGathererPlatform.WII, true),
              new StubFileBundleGatherer("3ds_7", FileBundleGathererPlatform.THREE_DS, true),
          }
          .OrderBy(g => g.IsAvailable)
          .ToArray();
  }
}

public class FileBundleGathererSelectorWindowViewModel : BViewModel {
  public IReadOnlyList<INamedFileBundleGatherer> FileBundleGatherers {
    get;
    protected set;
  }
    = ExtractorUtil.GetAllNamedFileBundleGatherers()
                   .Where(g => g.IsListed)
                   .OrderBy(g => g.IsAvailable)
                   .ToArray();
}

public partial class FileBundleGathererSelectorWindow : Window {
  public FileBundleGathererSelectorWindow() {
    InitializeComponent();
  }
}