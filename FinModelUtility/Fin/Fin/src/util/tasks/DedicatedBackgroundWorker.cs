using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

using fin.util.asserts;

namespace fin.util.tasks;

public class DedicatedBackgroundWorker : IDisposable {
  private readonly
      BlockingCollection<(Func<object> WorkItem, TaskCompletionSource<object>
          Tcs)>
      queue_ = new();

  private readonly Thread workerThread_;

  public DedicatedBackgroundWorker() {
    this.workerThread_ = new Thread(this.ProcessQueue_) {
        IsBackground = true, // Keeps it from blocking process shutdown
        Name = "DedicatedWorkerThread"
    };
    this.workerThread_.Start();
  }

  private void ProcessQueue_() {
    foreach (var (workItem, tcs) in this.queue_.GetConsumingEnumerable()) {
      try {
        // This always executes on the exact same background thread
        var result = workItem();
        tcs.SetResult(result);
      } catch (Exception ex) {
        tcs.SetException(ex);
      }
    }
  }

  public Task<T> EnqueueWorkAsync<T>(Func<T> work) where T : class {
    var tcs = new TaskCompletionSource<object>(
        TaskCreationOptions.RunContinuationsAsynchronously);
    this.queue_.Add((work, tcs));
    return CastBackTo_<T>(tcs.Task);
  }

  public void Dispose() {
    this.queue_.CompleteAdding();
    this.workerThread_.Join();
    this.queue_.Dispose();
  }

  private static Task<T> CastBackTo_<T>(Task<object> task)
      where T : class
    => task.ContinueWith(
        t => t.GetAwaiter().GetResult().AssertAsA<T>(),
        CancellationToken.None,
        TaskContinuationOptions.ExecuteSynchronously,
        TaskScheduler.Default);
}