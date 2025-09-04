using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Subjects;

using fin.util.time;

namespace fin.ui.avalonia.observables;

public class LoopingObservable<T> : IObservable<T> {
  private readonly ReplaySubject<T> impl_ = new(1);

  private TimedCallback timedCallback_;

  public LoopingObservable(
      float periodSeconds,
      params T[] values) : this(periodSeconds, 0, values) { }

  public LoopingObservable(
      float periodSeconds,
      int resetOffset,
      params T[] values) {
    var index = 0;
    this.timedCallback_ = TimedCallback.WithPeriod(
        () => {
          var currentValue = values[index];
          if (++index >= values.Length) {
            index = resetOffset;
          }

          this.impl_.OnNext(currentValue);
        },
        periodSeconds);
  }

  public IDisposable Subscribe(IObserver<T> observer)
    => this.impl_.Subscribe(observer);
}