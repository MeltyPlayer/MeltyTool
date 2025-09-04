using System.Reactive.Subjects;

using Avalonia.Media.Imaging;

using marioartisttool.util;

namespace MarioArtistTool.fileTree;

public class BucketBitmapObservableManager {
  private static readonly Bitmap IDLE_IMAGE_
      = AssetLoaderUtil.LoadBitmap("bucket/idle.png");

  private static readonly Bitmap WAVE_1_IMAGE_
      = AssetLoaderUtil.LoadBitmap("bucket/wave_1.png");

  private static readonly Bitmap WAVE_2_IMAGE_
      = AssetLoaderUtil.LoadBitmap("bucket/wave_2.png");

  private static readonly Bitmap WAVE_3_IMAGE_
      = AssetLoaderUtil.LoadBitmap("bucket/wave_3.png");

  private static readonly Bitmap WAVE_4_IMAGE_
      = AssetLoaderUtil.LoadBitmap("bucket/wave_4.png");

  private static readonly Bitmap HAT_IMAGE_
      = AssetLoaderUtil.LoadBitmap("bucket/hat.png");

  public ReplaySubject<Bitmap> BucketImage { get; } = new(1);
  public ReplaySubject<Bitmap?> HatImage { get; } = new(1);

  public BucketBitmapObservableManager() {
    this.BucketImage.OnNext(IDLE_IMAGE_);
    this.HatImage.OnNext(HAT_IMAGE_);
  }

  public bool IsMouseOver { get; set; }
  public bool IsOpen { get; set; }
}