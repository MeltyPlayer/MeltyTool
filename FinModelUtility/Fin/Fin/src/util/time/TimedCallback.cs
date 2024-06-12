using System;
using System.Threading;
using System.Threading.Tasks;

namespace fin.util.time {
  public class TimedCallback : IDisposable {
    private readonly Timer impl_;
    private float periodSeconds_;

    ~TimedCallback() => this.Dispose();
    public void Dispose() => this.impl_.Dispose();

    public static TimedCallback WithFrequency(Action callback, float hertz)
      => WithPeriod(callback, 1 / hertz);

    public static TimedCallback WithPeriod(Action callback,
                                           float periodSeconds)
      => new(callback, periodSeconds);

    public TimedCallback(Action callback, float periodSeconds) {
      this.impl_ = new Timer(_ => callback(),
                             null,
                             0,
                             (long) (periodSeconds * 1000));
      this.periodSeconds_ = periodSeconds;
    }

    public float Frequency {
      get => 1 / this.PeriodSeconds;
      set => this.PeriodSeconds = 1 / value;
    }

    public float PeriodSeconds {
      get => this.periodSeconds_;
      set =>
          this.impl_.Change(0, (long) ((this.periodSeconds_ = value) * 1000));
    }
  }
}