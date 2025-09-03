using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Threading;

using fin.util.time;

namespace fin.ui.avalonia.observables;

public class CircularObservable<T> : IObservable<T> {
  private readonly HashSet<IObserver<T>> observers_ = new();

  private T currentValue_;
  private TimedCallback timedCallback_;

  public CircularObservable(
      float seconds,
      params T[] values) {
    this.currentValue_ = values[0];

    var index = 0;
    this.timedCallback_ = TimedCallback.WithPeriod(
        () => {
          index = (index + 1) % values.Length;
          this.currentValue_ = values[index];
          foreach (var observer in this.observers_) {
            observer.OnNext(this.currentValue_);
          }
        },
        seconds);
  }

  public IDisposable Subscribe(IObserver<T> observer) {
    if (this.observers_.Add(observer)) {
      observer.OnNext(this.currentValue_);
    }

    return Disposable.Create(() => this.observers_.Remove(observer));
  }
}