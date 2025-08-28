using System;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Threading;

using fin.io;
using fin.ui.avalonia;
using fin.ui.avalonia.images;

using marioartist.api;
using marioartist.schema;

using marioartisttool.services;
using marioartisttool.util;

using ReactiveUI;

using schema.binary;

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
                                VerticalAlignment = VerticalAlignment.Center
                            };
                            stackPanel.Children.Add(textBlock);

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
                                             PixelPoint pixelPoint)
    => new(AssetLoaderUtil.LoadBitmap(cursorImageName), pixelPoint);
}