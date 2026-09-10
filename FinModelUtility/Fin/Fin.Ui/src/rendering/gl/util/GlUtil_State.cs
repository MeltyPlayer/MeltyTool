using fin.data.dictionaries;

using OpenTK.Windowing.Common;

namespace fin.ui.rendering.gl;

public partial class GlState;

public static partial class GlUtil {
  private static NullFriendlyDictionary<object?, GlState> stateByKey_ = new();

  private static Stack<GlState> stateStack_ = new();

  private static GlState currentState_;

  public static void SwitchContext(IGraphicsContext? context) {
    if (!stateByKey_.TryGetValue(context, out var state)) {
      stateByKey_.Add(context, state = new GlState());
    }

    currentState_ = state;
    context?.MakeCurrent();
  }

  public static void SwitchContext(object? any) {
    if (!stateByKey_.TryGetValue(any, out var state)) {
      stateByKey_.Add(any, state = new GlState());
    }

    currentState_ = state;
  }

  public static void PushContext() {
    stateStack_.Push(currentState_);
    currentState_ = new GlState();
  }

  public static void PopContext() => currentState_ = stateStack_.Pop();
}