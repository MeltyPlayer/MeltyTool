using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reactive.Linq;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Templates;
using Avalonia.Data;
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

using MarioArtistTool.fileTree;

using marioartisttool.services;
using marioartisttool.util;

using ReactiveUI;

using schema.binary;

using Brush = Avalonia.Media.Brush;
using Color = Avalonia.Media.Color;
using Image = Avalonia.Controls.Image;

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

      var fileCursorScale = 1;
      var fileCursorObservable = new LoopingObservable<Cursor>(.1f,
        LoadCursorFromAsset_("file_1.png", PixelPoint.Origin, fileCursorScale),
        LoadCursorFromAsset_("file_2.png", PixelPoint.Origin, fileCursorScale),
        LoadCursorFromAsset_("file_3.png", PixelPoint.Origin, fileCursorScale));

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
                            } else if (x is MfsTreeDirectory d) {
                              var bbom = new BucketBitmapObservableManager();
                              var bucketImage = bbom.BucketImage;
                              var hatImage = bbom.HatImage;

                              var bucket = new Image {
                                  Margin = new Thickness(0, -20, -16, 0),
                              };
                              bucket.Bind(Image.SourceProperty, bucketImage);

                              if (d.Children.Any()) {
                                var bucketPanel = new Grid {
                                    Width = 32,
                                    Height = 32
                                };
                                bucketPanel.Children.Add(bucket);

                                var hat = new Image {
                                    Width = 16,
                                    Height = 8,
                                    ZIndex = 2,
                                    VerticalAlignment = VerticalAlignment.Top,
                                    Margin = new Thickness(0, -4, -5, 0),
                                };
                                hat.Bind(Image.SourceProperty, hatImage);
                                bucketPanel.Children.Add(hat);

                                stackPanel.Children.Add(bucketPanel);
                              } else {
                                stackPanel.Children.Add(bucket);
                              }
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

                            if (x is MfsTreeFile) {
                              stackPanel.Children.Add(textBlock);
                            } else if (x is MfsTreeDirectory) {
                              var childCount = x.Children.Count();

                              var manyFilesImage
                                  = AssetLoaderUtil.LoadBitmap(
                                      "icon_many_files.png");
                              var fileImage
                                  = AssetLoaderUtil.LoadBitmap("icon_file.png");

                              var fileCount = childCount % 7;
                              var manyFilesCount = (childCount - fileCount) / 7;

                              var childPanel = new WrapPanel();
                              for (var i = 0; i < manyFilesCount; ++i) {
                                childPanel.Children.Add(new Image {
                                    Source = manyFilesImage,
                                    Margin = new Thickness(1, 1 + 1, 1, 1),
                                    Width = 11,
                                    Height = 12,
                                });
                              }

                              for (var i = 0; i < fileCount; ++i) {
                                childPanel.Children.Add(new Image {
                                    Source = fileImage,
                                    Margin = new Thickness(1, 1 + 4, 1, 1),
                                    Width = 8,
                                    Height = 8,
                                });
                              }

                              stackPanel.Children.Add(
                                  new StackPanel {
                                      Orientation = Orientation.Vertical,
                                      Margin = new Thickness(6, 0, 0, 0),
                                      Children = {
                                          textBlock,
                                          childPanel,
                                      },
                                  });
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
                                Child = stackPanel,
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
                                             PixelPoint pixelPoint,
                                             int scale = 1)
    => new(AssetLoaderUtil.LoadBitmap($"cursors/{cursorImageName}", scale),
           new PixelPoint(pixelPoint.X * scale, pixelPoint.Y * scale));
}