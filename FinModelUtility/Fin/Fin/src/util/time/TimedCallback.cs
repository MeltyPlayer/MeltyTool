using System;
using System.Threading;

namespace fin.util.time;

public class TimedCallback : IDisposable {
  private readonly Action callback_;
  private Timer? impl_;

  public TimedCallback(Action callback, float periodSeconds) {
    this.callback_ = callback;
    this.PeriodSeconds = periodSeconds;
    this.Active = true;
  }

  ~TimedCallback() => this.Dispose();
  public void Dispose() => this.impl_.Dispose();

  public static TimedCallback WithFrequency(Action callback, float hertz)
    => WithPeriod(callback, 1 / hertz);

  public static TimedCallback WithPeriod(Action callback,
                                         float periodSeconds)
    => new(callback, periodSeconds);

  public float Frequency {
    get => 1 / this.PeriodSeconds;
    set => this.PeriodSeconds = 1 / value;
  }

  public float PeriodSeconds {
    get;
    set {
      field = value;
      this.impl_?.Change(0, (long) (this.PeriodSeconds * 1000));
    }
  }

  public bool Active {
    get;
    set {
      if (field && !value) {
        this.impl_?.Dispose();
        this.impl_ = null;
      } else if (!field && value) {
        this.impl_ = new Timer(_ => this.callback_(),
                               null,
                               0,
                               (long) (this.PeriodSeconds * 1000));
      } 

      field = value;
    }
  }
}