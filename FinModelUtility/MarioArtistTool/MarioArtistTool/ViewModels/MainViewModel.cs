using System;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;

using fin.ui.avalonia;

using marioartist.api;

using marioartisttool.services;

using ReactiveUI;

namespace marioartisttool.ViewModels;

public partial class MainViewModel : ViewModelBase {
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

                            var textBlock = new TextBlock {
                                Text = x.Name.ToString(),
                                Classes = { "regular" }
                            };

                            // TODO: Load image from file
                            /*var icon = new MaterialIcon {
                                Kind = x.Icon.Value,
                                Margin = new Thickness(-24, 0, 4, 0),
                                Height = 16,
                                Width = 16
                            };*/

                            var stackPanel = new StackPanel {
                                Orientation = Orientation.Horizontal,
                            };
                            // TODO: Add icon here
                            stackPanel.Children.AddRange([textBlock]);

                            return stackPanel;
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
                                             PixelPoint pixelPoint) {
    using var s
        = AssetLoader.Open(
            new Uri($"avares://MarioArtistTool/Assets/{cursorImageName}"));
    var bitmap = new Bitmap(s);
    return new Cursor(bitmap, pixelPoint);
  }
}