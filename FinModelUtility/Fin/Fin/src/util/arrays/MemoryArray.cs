using System;

namespace fin.util.arrays {
  public interface IMemoryArray<T> {
    bool TryUpdate(ReadOnlySpan<T> newValues);
    Memory<T> CurrentMemory { get; }
    Span<T> CurrentSpan { get; }
  }

  public class MemoryArray<T>(int maxSize) : IMemoryArray<T> {
    private readonly T[] impl_ = new T[maxSize];
    private bool hasSetFirst_;

    public bool TryUpdate(ReadOnlySpan<T> newValues) {
      var didUpdate = false;
      if (!this.hasSetFirst_ || this.CurrentMemory.Length != newValues.Length) {
        this.hasSetFirst_ = true;
        didUpdate = true;
        this.CurrentMemory = this.impl_.AsMemory(0, newValues.Length);
      }

      var currentSpan = this.CurrentSpan;
      for (var i = 0; i < currentSpan.Length; ++i) {
        var currentValue = currentSpan[i];
        var newValue = newValues[i];

        didUpdate = newValue?.Equals(currentValue) ?? currentValue != null;

        currentSpan[i] = newValue;
      }

      return didUpdate;
    }

    public Memory<T> CurrentMemory { get; private set; }
    public Span<T> CurrentSpan => this.CurrentMemory.Span;
  }
}