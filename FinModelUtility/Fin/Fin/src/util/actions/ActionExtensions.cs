using System;
using System.Threading;
using System.Threading.Tasks;

namespace fin.util.actions;

public static class ActionExtensions {
  public static Action Debounce(this Action func,
                                int milliseconds = 300) {
    var last = 0;
    return () => {
      var current = Interlocked.Increment(ref last);
      Task.Delay(milliseconds)
          .ContinueWith(task => {
            if (current == last) {
              func();
            }
            task.Dispose();
          });
    };
  }
}