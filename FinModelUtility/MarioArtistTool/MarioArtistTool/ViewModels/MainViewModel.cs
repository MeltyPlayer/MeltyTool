using System.Collections.Generic;
using System.Linq;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;

using fin.io;
using fin.ui.avalonia;
using fin.ui.avalonia.images;
using fin.ui.avalonia.observables;

using marioartist.api;
using marioartist.schema;
using marioartist.schema.mfs;

using marioartisttool.services;
using marioartisttool.util;

using ReactiveUI;

using schema.binary;

namespace marioartisttool.ViewModels;

public class MainViewModelForDesigner : MainViewModel {
  public MainViewModelForDesigner() {
    var rootSubdirs = new LinkedList<MfsTreeDirectory>();
    var root
        = new MfsTreeDirectory(null,
                               new MfsDirectory { Name = "/" },
                               rootSubdirs,
                               []);

    var subdir1Files = new LinkedList<MfsTreeFile>();
    var subdir1
        = new MfsTreeDirectory(root,
                               new MfsDirectory { Name = "subdir1" },
                               [],
                               subdir1Files);
    rootSubdirs.AddLast(subdir1);

    var subdir2Files = new LinkedList<MfsTreeFile>();
    var subdir2
        = new MfsTreeDirectory(root,
                               new MfsDirectory { Name = "subdir2" },
                               [],
                               subdir2Files);
    rootSubdirs.AddLast(subdir2);

    MfsFileSystemService.LoadFileSystem(root);
  }
}

public class MainViewModel : ViewModelBase {
  public Cursor Cursor { get; }
    = LoadCursorFromAsset_("thumb_in.png", new PixelPoint(2, 2));

  public HierarchicalTreeDataGridSource<MfsTreeIoObject>? FileSystemTreeSource {
    get;
    set => this.RaiseAndSetIfChanged(ref field, value);
  }

  public MainViewModel() {
    MfsFileSystemService.OnFileSystemLoaded += root => {
      if (root == null) {
        this.FileSystemTreeSource = null;
        return;
      }

      var fileCursorObservable = new CircularObservable<Cursor>(.1f,
        LoadCursorFromAsset_("file_1.png", PixelPoint.Origin),
        LoadCursorFromAsset_("file_2.png", PixelPoint.Origin),
        LoadCursorFromAsset_("file_3.png", PixelPoint.Origin));

      this.FileSystemTreeSource
          = new HierarchicalTreeDataGridSource<MfsTreeIoObject>(
              root.Children) {
              Columns = {
                  new HierarchicalExpanderColumn<MfsTreeIoObject>(
                      new TemplateColumn<MfsTreeIoObject>(
                          "Name",
                          new FuncDataTemplate<MfsTreeIoObject>((x, _) => {
                            if (x == null) {
                              return null;
                            }

                            var stackPanel = new StackPanel {
                                Orientation = Orientation.Horizontal,
                            };

                            if (x is MfsTreeFile mfsTreeFile) {
                              using var br
                                  = mfsTreeFile.OpenReadAsBinary(
                                      Endianness.BigEndian);

                              var thumbnail = new Argb1555Image(24, 24);
                              thumbnail.Read(br);

                              var finImage = thumbnail.ToImage();
                              var avaloniaImage = finImage.AsAvaloniaImage();

                              var icon = new Image {
                                  Source = avaloniaImage,
                              };

                              stackPanel.Children.Add(icon);
                            }

                            var brushWhite
                                = new SolidColorBrush(
                                    Color.FromRgb(255, 255, 255));
                            var brushYellow
                                = new SolidColorBrush(
                                    Color.FromRgb(245, 181, 0));

                            var textBlock = new TextBlock {
                                Text = x.Name.ToString(),
                                Classes = {
                                    x is MfsTreeDirectory ? "h3" : "h4"
                                },
                                Padding = new Thickness(0),
                                VerticalAlignment = VerticalAlignment.Center,
                                Foreground = brushWhite
                            };
                            stackPanel.Children.Add(textBlock);

                            Control borderChild = stackPanel;
                            if (x is MfsTreeDirectory) {
                              var childCount = x.Children.Count();
                              var childIcon
                                  = AssetLoaderUtil.LoadBitmap("icon_file.png");

                              var childPanel = new WrapPanel();
                              for (var i = 0; i < childCount; ++i) {
                                childPanel.Children.Add(new Image {
                                    Source = childIcon,
                                    Width = 8,
                                    Height = 8,
                                    Margin = new Thickness(1),
                                });
                              }

                              borderChild = new StackPanel {
                                  Orientation = Orientation.Vertical,
                                  Children = {
                                      stackPanel,
                                      childPanel,
                                  }
                              };
                            }

                            Brush borderBrush;
                            uint marginTop, marginBottom;
                            if (x is MfsTreeDirectory) {
                              borderBrush = brushYellow;
                              marginTop = 4;
                              marginBottom = marginTop / 2;
                            } else {
                              borderBrush = brushWhite;
                              marginTop = 2;
                              marginBottom = marginTop / 2;
                            }

                            var border = new Border {
                                Child = borderChild,
                                Padding = new Thickness(2),
                                BorderThickness = new Thickness(3),
                                CornerRadius = new CornerRadius(4),
                                Background
                                    = new SolidColorBrush(
                                        Color.FromRgb(33, 33, 33)),
                                BorderBrush = borderBrush,
                                Margin = new Thickness(
                                    0,
                                    marginTop,
                                    2,
                                    marginBottom),
                            };

                            if (x is MfsTreeFile) {
                              border.Bind(Border.CursorProperty,
                                          fileCursorObservable);
                            }

                            return border;
                          }),
                          null,
                          GridLength.Star),
                      x => x.Children)
              }
          };

      Dispatcher.UIThread.Invoke(() => {
        var rowSelection = this.FileSystemTreeSource.RowSelection!;
        rowSelection.SelectionChanged += (_, e) => {
          var selectedItems = e.SelectedItems;
          if (selectedItems.Count == 0) {
            return;
          }

          if (selectedItems[0] is MfsTreeFile file) {
            MfsFileSystemService.SelectFile(file);
          }
        };
      });
    };
  }

  private static Cursor LoadCursorFromAsset_(string cursorImageName,
                                             PixelPoint pixelPoint)
    => new(AssetLoaderUtil.LoadBitmap($"cursors/{cursorImageName}"),
           pixelPoint);
}