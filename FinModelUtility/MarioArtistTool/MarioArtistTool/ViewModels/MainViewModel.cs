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
    = LoadCursorFromAsset_("cursor_thumb_in.png", new PixelPoint(2, 2));

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

                            var textBlock = new TextBlock {
                                Text = x.Name.ToString(),
                                Classes = { "regular" },
                                VerticalAlignment = VerticalAlignment.Center,
                                Foreground
                                    = new SolidColorBrush(
                                        Color.FromRgb(255, 255, 255))
                            };
                            stackPanel.Children.Add(textBlock);

                            Color borderColor;
                            uint marginTop, marginBottom;
                            if (x.Children.Any()) {
                              borderColor = Color.FromRgb(245, 181, 0);
                              marginTop = 4;
                              marginBottom = marginTop / 2;
                            } else {
                              borderColor = Color.FromRgb(255, 255, 255);
                              marginTop = 2;
                              marginBottom = marginTop / 2;
                            }

                            var border = new Border {
                                Child = stackPanel,
                                Padding = new Thickness(2),
                                BorderThickness = new Thickness(3),
                                CornerRadius = new CornerRadius(4),
                                Background
                                    = new SolidColorBrush(
                                        Color.FromRgb(33, 33, 33)),
                                BorderBrush = new SolidColorBrush(borderColor),
                                Margin = new Thickness(
                                    0,
                                    marginTop,
                                    0,
                                    marginBottom)
                            };

                            return border;
                          })),
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
    => new(AssetLoaderUtil.LoadBitmap(cursorImageName), pixelPoint);
}