using System;
using System.Collections.Generic;
using System.Reactive.Disposables;

using fin.util.time;

namespace fin.ui.avalonia.observables;

public class LoopingObservable<T> : IObservable<T> {
  private readonly HashSet<IObserver<T>> observers_ = new();

  private T currentValue_;
  private TimedCallback timedCallback_;

  public LoopingObservable(
      float periodSeconds,
      params T[] values) : this(periodSeconds, 0, values) { }

  public LoopingObservable(
      float periodSeconds,
      int resetOffset,
      params T[] values) {
    this.currentValue_ = values[0];

    var index = 0;
    this.timedCallback_ = TimedCallback.WithPeriod(
        () => {
          if (++index >= values.Length) {
            index = resetOffset;
          }

          this.currentValue_ = values[index];
          foreach (var observer in this.observers_) {
            observer.OnNext(this.currentValue_);
          }
        },
        periodSeconds);
  }

  public IDisposable Subscribe(IObserver<T> observer) {
    if (this.observers_.Add(observer)) {
      observer.OnNext(this.currentValue_);
    }

    return Disposable.Create(() => this.observers_.Remove(observer));
  }
}