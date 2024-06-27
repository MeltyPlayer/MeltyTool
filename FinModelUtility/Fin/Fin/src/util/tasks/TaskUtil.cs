using System;
using System.Diagnostics;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace fin.util.tasks;

public static class TaskUtil {
  public static void ThenSetResult<T>(this Task<T> task,
                                      TaskCompletionSource<T> source)
    => task.ContinueWith(t => source.SetResult(t.Result));


  public static Task<TTo> CastChecked<TFrom, TTo>(this Task<TFrom> task)
      where TFrom : INumber<TFrom>
      where TTo : INumber<TTo>
    => task.ContinueWith(t => TTo.CreateChecked(t.Result));

  public static Task<TTo> CastSaturating<TFrom, TTo>(this Task<TFrom> task)
      where TFrom : INumber<TFrom>
      where TTo : INumber<TTo>
    => task.ContinueWith(t => TTo.CreateSaturating(t.Result));

  public static Task<TTo> CastTruncating<TFrom, TTo>(this Task<TFrom> task)
      where TFrom : INumber<TFrom>
      where TTo : INumber<TTo>
    => task.ContinueWith(t => TTo.CreateTruncating(t.Result));


  public static Task<TNumber> Add<TNumber>(
      this Task<TNumber> lhsTask,
      Task<TNumber> rhsTask)
      where TNumber : INumber<TNumber>
    => Task.WhenAll(lhsTask, rhsTask)
           .ContinueWith(tasks => tasks.Result[0] + tasks.Result[1]);

  public static async Task<TNumber> Add<TNumber>(
      this Task<TNumber> lhsTask,
      TNumber rhs)
      where TNumber : INumber<TNumber> {
    var lhs = await lhsTask;
    return lhs + rhs;
  }


  public static Task<TNumber> Subtract<TNumber>(
      this Task<TNumber> lhsTask,
      Task<TNumber> rhsTask)
      where TNumber : INumber<TNumber>
    => Task.WhenAll(lhsTask, rhsTask)
           .ContinueWith(tasks => tasks.Result[0] - tasks.Result[1]);

  public static async Task<TNumber> Subtract<TNumber>(
      this Task<TNumber> lhsTask,
      TNumber rhs)
      where TNumber : INumber<TNumber> {
    var lhs = await lhsTask;
    return lhs - rhs;
  }


  public static Task RunExpensiveButAccurateTickHandler(
      double frequency,
      Action handler,
      CancellationToken? cancellationToken = null)
    => Task.Run(() => {
      var stopwatch = new Stopwatch();
      var targetPeriod = 1 / frequency;
      var targetTicks = Stopwatch.Frequency * targetPeriod;

      while (!cancellationToken?.IsCancellationRequested ?? true) {
        stopwatch.Restart();

        handler();

        // Very expensive, but FAR more accurate than Thread.sleep
        var i = 0;
        while (stopwatch.ElapsedTicks < targetTicks) {
          ++i;
        }
      }
    });
}