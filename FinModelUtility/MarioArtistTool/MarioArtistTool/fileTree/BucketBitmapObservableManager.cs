using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

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

  public BucketBitmapState CurrentState { get; private set; }
    = BucketBitmapState.IDLE;

  private CancellationTokenSource? lastCancellationTokenSource_;

  public ReplaySubject<Bitmap> BucketImage { get; } = new(1);
  public ReplaySubject<Bitmap?> HatImage { get; } = new(1);

  public BucketBitmapObservableManager() {
    this.BucketImage.OnNext(IDLE_IMAGE_);
    this.HatImage.OnNext(HAT_IMAGE_);
  }

  public bool IsMouseOver {
    get;
    set {
      field = value;
      this.UpdateState_();
    }
  }

  public bool IsOpen {
    get;
    set {
      field = value;
      this.UpdateState_();
    }
  }

  private void UpdateState_() {
    this.lastCancellationTokenSource_?.Cancel();

    var newCancellationTokenSource = new CancellationTokenSource();
    this.lastCancellationTokenSource_ = newCancellationTokenSource;

    var from = this.CurrentState;

    BucketBitmapState to = BucketBitmapState.IDLE;

    if (this.IsOpen) { }

    if (this.IsMouseOver) {
      to = BucketBitmapState.WAVING;
    }

    Task.Run(async () => {
      await foreach (var next in BucketBitmapStateUtils.GetPath(
                   from,
                   to,
                   newCancellationTokenSource.Token)) {
        this.CurrentState = next;
        var nextBucketImage = next switch {
            BucketBitmapState.IDLE => IDLE_IMAGE_,
            BucketBitmapState.WAVE_1_IN or BucketBitmapState.WAVE_1_OUT
                => WAVE_1_IMAGE_,
            BucketBitmapState.WAVE_2_IN or BucketBitmapState.WAVE_2_OUT
                => WAVE_2_IMAGE_,
            BucketBitmapState.WAVE_3_IN or BucketBitmapState.WAVE_3_OUT
                => WAVE_3_IMAGE_,
            BucketBitmapState.WAVE_4_IN or BucketBitmapState.WAVE_4_OUT
                => WAVE_4_IMAGE_,
        };

        this.BucketImage.OnNext(nextBucketImage);
      }
    });
  }
}