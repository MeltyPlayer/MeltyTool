using fin.data.dictionaries;

using OpenTK.Windowing.Common;

namespace fin.ui.rendering.gl {
  public partial class GlState { }

  public static partial class GlUtil {
    private static NullFriendlyDictionary<IGraphicsContext?, GlState>
        stateByContext_ = new();

    private static GlState currentState_;

    public static void SwitchContext(IGraphicsContext? context) {
      if (!GlUtil.stateByContext_.TryGetValue(context, out var state)) {
        GlUtil.stateByContext_.Add(context, state = new GlState());
      }

      currentState_ = state;
    }
  }
}