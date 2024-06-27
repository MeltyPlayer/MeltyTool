using BenchmarkDotNet.Attributes;

using CommunityToolkit.HighPerformance;

using SixLabors.ImageSharp.PixelFormats;

namespace benchmarks;

public class CopyingValue {
  private static int size_ = 10;

  private int[] intSrc_ = new int[size_];
  private int[] intDst_ = new int[size_];
  private Rgba32[] rgbaSrc_ = new Rgba32[size_];
  private Rgba32[] rgbaDst_ = new Rgba32[size_];

  private readonly int n_ = 10000;


  [Benchmark]
  public void UsingSpanCopyRgba() {
      for (var i = 0; i < n_; ++i) {
        var srcSpan = this.rgbaSrc_.AsSpan();
        var dstSpan = this.rgbaDst_.AsSpan();

        for (var p = 0; p < size_; ++p) {
          srcSpan.Slice(p, 1).CopyTo(dstSpan.Slice(p, 1));
        }
      }
    }

  [Benchmark]
  public void UsingSpanCopyInt() {
      for (var i = 0; i < n_; ++i) {
        var srcSpan = this.intSrc_.AsSpan();
        var dstSpan = this.intDst_.AsSpan();

        for (var p = 0; p < size_; ++p) {
          srcSpan.Slice(p, 1).CopyTo(dstSpan.Slice(p, 1));
        }
      }
    }

  [Benchmark]
  public void UsingSpanCopyMismatch() {
      for (var i = 0; i < n_; ++i) {
        var srcSpan = this.intSrc_.AsSpan().AsBytes();
        var dstSpan = this.rgbaDst_.AsSpan().AsBytes();

        for (var p = 0; p < size_; ++p) {
          srcSpan.Slice(4 * p, 4).CopyTo(dstSpan.Slice(4 * p, 4));
        }
      }
    }


  [Benchmark]
  public void UsingSpanCopyRgbaCommon() {
      var srcSpan = this.rgbaSrc_.AsSpan();
      var dstSpan = this.rgbaDst_.AsSpan();

      for (var i = 0; i < n_; ++i) {
        for (var p = 0; p < size_; ++p) {
          srcSpan.Slice(p, 1).CopyTo(dstSpan.Slice(p, 1));
        }
      }
    }

  [Benchmark]
  public void UsingSpanCopyIntCommon() {
      var srcSpan = this.intSrc_.AsSpan();
      var dstSpan = this.intDst_.AsSpan();

      for (var i = 0; i < n_; ++i) {
        for (var p = 0; p < size_; ++p) {
          srcSpan.Slice(p, 1).CopyTo(dstSpan.Slice(p, 1));
        }
      }
    }

  [Benchmark]
  public void UsingSpanCopyMismatchCommon() {
      var srcSpan = this.intSrc_.AsSpan().AsBytes();
      var dstSpan = this.rgbaDst_.AsSpan().AsBytes();

      for (var i = 0; i < n_; ++i) {
        for (var p = 0; p < size_; ++p) {
          srcSpan.Slice(4 * p, 4).CopyTo(dstSpan.Slice(4 * p, 4));
        }
      }
    }


    
  [Benchmark]
  public void UsingSetRgbaViaSpan() {
      for (var i = 0; i < n_; ++i) {
        var srcSpan = this.rgbaSrc_.AsSpan();
        var dstSpan = this.rgbaDst_.AsSpan();

        for (var p = 0; p < size_; ++p) {
          dstSpan[p] = srcSpan[p];
        }
      }
    }

  [Benchmark]
  public void UsingSetRgbaViaSpanCommon() {
      var srcSpan = this.rgbaSrc_.AsSpan();
      var dstSpan = this.rgbaDst_.AsSpan();

      for (var i = 0; i < n_; ++i) {
        for (var p = 0; p < size_; ++p) {
          dstSpan[p] = srcSpan[p];
        }
      }
    }

  [Benchmark]
  public void UsingSetRgbaViaArray() {
      for (var i = 0; i < n_; ++i) {
        for (var p = 0; p < size_; ++p) {
          this.rgbaDst_[p] = this.rgbaSrc_[p];
        }
      }
    }


    
  [Benchmark]
  public void UsingSetIntViaSpan() {
      for (var i = 0; i < n_; ++i) {
        var srcSpan = this.intSrc_.AsSpan();
        var dstSpan = this.intDst_.AsSpan();

        for (var p = 0; p < size_; ++p) {
          dstSpan[p] = srcSpan[p];
        }
      }
    }

  [Benchmark]
  public void UsingSetIntViaSpanCommon() {
      var srcSpan = this.intSrc_.AsSpan();
      var dstSpan = this.intDst_.AsSpan();

      for (var i = 0; i < n_; ++i) {
        for (var p = 0; p < size_; ++p) {
          dstSpan[p] = srcSpan[p];
        }
      }
    }

  [Benchmark]
  public void UsingSetIntViaArray() {
      for (var i = 0; i < n_; ++i) {
        for (var p = 0; p < size_; ++p) {
          this.intDst_[p] = this.intSrc_[p];
        }
      }
    }
}