using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;

using fin.config.avalonia.services;
using fin.data.queues;
using fin.io;
using fin.model.io;
using fin.ui;
using fin.ui.avalonia.observables;
using fin.util.enumerables;
using fin.util.io;

using marioartist.api;

using marioartisttool.config;
using marioartisttool.services;
using marioartisttool.util;

using ReactiveUI;

namespace marioartisttool.file_select;

public class FileSelectTopBarViewModel : BViewModel {
  public IReadOnlyList<IModelFileBundle> AllModelFileBundles {
    get;
    set {
      this.RaiseAndSetIfChanged(ref field, value);

      this.IsExportAllEnabled = value.Count > 0;
      this.ExportAllTip = value.Count == 1
          ? "Export 1 file"
          : $"Export all {value.Count} files";
    }
  }

  public IModelFileBundle? CurrentModelFileBundle {
    get;
    set {
      this.RaiseAndSetIfChanged(ref field, value);
      this.IsExportCurrentEnabled = value != null;
    }
  }

  public string ExportAllTip {
    get;
    set => this.RaiseAndSetIfChanged(ref field, value);
  }

  public bool IsExportAllEnabled {
    get;
    set => this.RaiseAndSetIfChanged(ref field, value);
  }

  public bool IsExportCurrentEnabled {
    get;
    set => this.RaiseAndSetIfChanged(ref field, value);
  }
}

public partial class FileSelectTopBar : UserControl {
  private static readonly LoopingObservable<Cursor> GRAB_CURSOR_;

  static FileSelectTopBar() {
    var grabOrigin = new PixelPoint(2, 2);
    var grabCursor0 = AssetLoaderUtil.LoadCursor("grab_0.png", grabOrigin);
    var grabCursor1 = AssetLoaderUtil.LoadCursor("grab_1.png", grabOrigin);
    var grabCursor2 = AssetLoaderUtil.LoadCursor("grab_2.png", grabOrigin);

    GRAB_CURSOR_ = new LoopingObservable<Cursor>(.1f,
                                                 grabCursor0,
                                                 grabCursor1,
                                                 grabCursor2,
                                                 grabCursor2,
                                                 grabCursor1);
  }

  public FileSelectTopBar() {
    var dataContext = new FileSelectTopBarViewModel();
    this.DataContext = dataContext;

    InitializeComponent();

    MfsFileSystemService.OnFileSystemLoaded += root => {
      var fileBundles = new List<IModelFileBundle>();

      if (root != null) {
        var directoryQueue = new FinQueue<MfsTreeDirectory>(root);
        while (directoryQueue.TryDequeue(out var current)) {
          fileBundles.AddRange(
              current.Children.OfType<MfsTreeFile>()
                     .Select(f => f.FileType.ToLower() switch {
                         ".ma3d1" =>
                             (IModelFileBundle) new Ma3d1ModelFileBundle(f),
                         ".tstlt" => new TstltModelFileBundle(f),
                         _        => null,
                     })
                     .Nonnull());

          directoryQueue.Enqueue(current.Children.OfType<MfsTreeDirectory>());
        }
      }

      dataContext.AllModelFileBundles = fileBundles;
    };

    MfsFileSystemService.OnFileSelected += file
        => dataContext.CurrentModelFileBundle
            = file?.FileType.ToLower() switch {
                ".ma3d1" => new Ma3d1ModelFileBundle(file),
                ".tstlt" => new TstltModelFileBundle(file),
                _        => null,
            };

    this.MeltyButton.Bind(Button.CursorProperty, GRAB_CURSOR_);

    AnimateButton_(this.DiskSwapButton,
                   this.DiskSwapIcon,
                   AssetLoaderUtil.LoadBitmap(
                       "file_select/top_bar/disk_swap/idle.png"),
                   AssetLoaderUtil.LoadBitmaps(
                       i => $"file_select/top_bar/disk_swap/anim_{i}.png",
                       6),
                   null,
                   AssetLoaderUtil.LoadBitmap(
                       $"file_select/top_bar/disk_swap/anim_0.png"),
                   async () => {
                     var originalBackground = this.DiskSwapButton.Background;
                     var originalHeight = this.DiskSwapIcon.Height;

                     this.DiskSwapButton.Background
                         = new SolidColorBrush(Color.FromRgb(9, 56, 1));
                     this.DiskSwapIcon.Height = 18;

                     await MfsFileSystemService
                         .PromptUserForDiskFileAndLoadIfValid();

                     this.DiskSwapButton.Background = originalBackground;
                     this.DiskSwapIcon.Height = originalHeight;
                   });

    AnimateButton_(this.ExportAllButton,
                   this.ExportAllIcon,
                   null,
                   [
                       AssetLoaderUtil.LoadBitmap(
                           "file_select/top_bar/export_all/hover.png")
                   ],
                   AssetLoaderUtil.LoadBitmap(
                       "file_select/top_bar/export_all/disabled.png"),
                   AssetLoaderUtil.LoadBitmap(
                       "file_select/top_bar/export_all/click_0.png"),
                   () => ExportFileBundles_(dataContext.AllModelFileBundles));

    AnimateButton_(this.ExportCurrentButton,
                   this.ExportCurrentIcon,
                   null,
                   [
                       AssetLoaderUtil.LoadBitmap(
                           "file_select/top_bar/export_current/hover.png")
                   ],
                   AssetLoaderUtil.LoadBitmap(
                       "file_select/top_bar/export_current/disabled.png"),
                   AssetLoaderUtil.LoadBitmap(
                       "file_select/top_bar/export_current/click_0.png"),
                   () => ExportFileBundles_(
                       [dataContext.CurrentModelFileBundle]));
  }

  private static async Task ExportFileBundles_(
      IReadOnlyList<IModelFileBundle> fileBundles) {
    var storageProvider = TopLevelService.Instance.StorageProvider;
    var config = Config.INSTANCE;

    var mostRecentDirectory = config.MostRecentOutputDirectory ?? "./";

    var startLocation
        = await storageProvider
            .TryGetFolderFromPathAsync(mostRecentDirectory);

    var selectedStorageFolders
        = await storageProvider
            .OpenFolderPickerAsync(new FolderPickerOpenOptions {
                SuggestedStartLocation = startLocation,
                Title = "Select output directory",
            });
    if (selectedStorageFolders is not { Count: 1 }) {
      return;
    }

    var selectedStorageFolder = selectedStorageFolders[0];
    var outputDirectory
        = new FinDirectory(selectedStorageFolder.Path.LocalPath);

    config.MostRecentOutputDirectory = outputDirectory.FullPath;
    config.Save();

    ExportService.ExportBundles(fileBundles, outputDirectory);
  }

  private static void AnimateButton_(
      Button button,
      Image icon,
      Bitmap? idleBitmap,
      Bitmap[] hoverBitmaps,
      Bitmap? disabledBitmap,
      Bitmap clickedBitmap,
      Func<Task> onClick) {
    button.Bind(Button.CursorProperty, GRAB_CURSOR_);

    var isEnabledObservable = button.GetObservable(Button.IsEnabledProperty);
    var isFocusedObservable = button.GetObservable(Button.IsFocusedProperty);
    var isPointerOverObservable
        = button.GetObservable(Button.IsPointerOverProperty);

    var isClickedSubject = new BehaviorSubject<bool>(false);

    var currentBitmap =
        Observable.CombineLatest(isClickedSubject,
                                 isEnabledObservable,
                                 isFocusedObservable,
                                 isPointerOverObservable)
                  .Select(states => {
                    var isClicked = states[0];
                    var isEnabled = states[1];
                    var isFocused = states[2];
                    var isPointerOver = states[3];

                    if (isClicked) {
                      return Observable.Return(clickedBitmap);
                    }

                    if (!isEnabled) {
                      return Observable.Return(disabledBitmap);
                    }

                    if (!isPointerOver && !isFocused) {
                      return Observable.Return(idleBitmap);
                    }

                    if (hoverBitmaps.Length == 1) {
                      return Observable.Return(hoverBitmaps[0]);
                    }

                    return new LoopingObservable<Bitmap>(.1f, hoverBitmaps);
                  })
                  .Switch();
    icon.Bind(Image.SourceProperty, currentBitmap);

    button.Click += async (_, _) => {
      isClickedSubject.OnNext(true);
      await onClick();
      isClickedSubject.OnNext(false);
    };
  }

  private void MeltyButton_OnClick(object? sender, RoutedEventArgs e)
    => WebBrowserUtil.OpenGithub();

  private void ToggleBallMode_(object? sender, RoutedEventArgs e)
    => EasterEggService.ΔIsInBallMode.OnNext(
        this.BallModeToggleButton.IsChecked ?? false);
}