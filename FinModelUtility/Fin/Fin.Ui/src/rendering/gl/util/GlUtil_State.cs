using fin.data.dictionaries;

using OpenTK.Windowing.Common;

namespace fin.ui.rendering.gl {
  public partial class GlState { }

  public static partial class GlUtil {
    private static NullFriendlyDictionary<object?, GlState>
        stateByKey_ = new();

    private static GlState currentState_;

    public static void SwitchContext(IGraphicsContext? context) {
      if (!GlUtil.stateByKey_.TryGetValue(context, out var state)) {
        GlUtil.stateByKey_.Add(context, state = new GlState());
      }

      currentState_ = state;
    }

    public static void SwitchContext(object? any) {
      if (!GlUtil.stateByKey_.TryGetValue(any, out var state)) {
        GlUtil.stateByKey_.Add(any, state = new GlState());
      }

      currentState_ = state;
    }
  }
}