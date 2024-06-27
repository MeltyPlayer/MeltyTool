using BenchmarkDotNet.Attributes;

namespace benchmarks;

public class CopyingMemory {
  private readonly int n_ = 100000;
    
  private const int SIZE = 12;
    
  private readonly byte[] src_ = new byte[CopyingMemory.SIZE];
  private readonly byte[] dst_ = new byte[CopyingMemory.SIZE];

  private const int SIZE_INT = CopyingMemory.SIZE / 3;

  private readonly int[] srcInt_ = new int[CopyingMemory.SIZE_INT];
  private readonly int[] dstInt_ = new int[CopyingMemory.SIZE_INT];



  [Benchmark]
  public void UsingBufferBlockCopy() {
      for (var i = 0; i < n_; ++i) {
        Buffer.BlockCopy(this.src_, 0, this.dst_, 0, CopyingMemory.SIZE);
      }
    }

  [Benchmark]
  public void UsingArrayCopy() {
      for (var i = 0; i < n_; ++i) {
        Array.Copy(this.src_, this.dst_, SIZE);
      }
    }

  [Benchmark]
  public void UsingSpanCopy() {
      for (var i = 0; i < n_; ++i) {
        this.src_.AsSpan().CopyTo(this.dst_.AsSpan());
      }
    }

  [Benchmark]
  public void UsingForLoop() {
      for (var i = 0; i < n_; ++i) {
        for (var b = 0; b < CopyingMemory.SIZE; ++b) {
          this.dst_[b] = this.src_[b];
        }
      }
    }



  [Benchmark]
  public void UsingBufferBlockCopyInt() {
      for (var i = 0; i < n_; ++i) {
        Buffer.BlockCopy(this.srcInt_, 0, this.dstInt_, 0, CopyingMemory.SIZE_INT);
      }
    }

  [Benchmark]
  public void UsingArrayCopyInt() {
      for (var i = 0; i < n_; ++i) {
        Array.Copy(this.srcInt_, this.dstInt_, SIZE_INT);
      }
    }

  [Benchmark]
  public void UsingSpanCopyInt() {
      for (var i = 0; i < n_; ++i) {
        this.srcInt_.AsSpan().CopyTo(this.dstInt_.AsSpan());
      }
    }

  [Benchmark]
  public void UsingForLoopInt() {
      for (var i = 0; i < n_; ++i) {
        for (var b = 0; b < CopyingMemory.SIZE_INT; ++b) {
          this.dstInt_[b] = this.srcInt_[b];
        }
      }
    }
}