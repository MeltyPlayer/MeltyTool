using fin.data.dictionaries;
using fin.model;

using OpenTK.Windowing.Common;

namespace fin.ui.rendering.gl;

public partial record GlState;

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

  public static void PushState() {
    stateStack_.Push(currentState_);
    currentState_ = currentState_ with { };
  }

  public static void PopState() {
    var previousState = stateStack_.Pop();
    ReapplyState_(previousState);
    currentState_ = previousState;
  }

  private static void ReapplyState_(GlState state) {
    SetBlendingSeparate(
        state.CurrentBlending.colorBlendEquation,
        state.CurrentBlending.colorSrcFactor,
        state.CurrentBlending.colorDstFactor,
        state.CurrentBlending.alphaBlendEquation,
        state.CurrentBlending.alphaSrcFactor,
        state.CurrentBlending.alphaDstFactor,
        state.CurrentBlending.logicOp);
    SetClearColor(state.ClearColor);
    SetCulling(state.CurrentCullingMode);
    SetDepth(state.DepthModeAndCompareType.Item1,
             state.DepthModeAndCompareType.Item2);
    SetFlipFaces(state.FlipFaces);
    BindUboData(state.CurrentUboDataId);
    for (var i = 0; i < state.CurrentUboBufferBaseIdByIndex.Length; ++i) {
      BindUboBufferBase(i, state.CurrentUboBufferBaseIdByIndex[i]);
    }

    BindVao(state.CurrentVaoId);
    BindEbo(state.CurrentEboId);

    for (var i = 0; i < MaterialConstants.MAX_TEXTURES; ++i) {
      BindTexture(i, state.CurrentTextureBindings[i]);
    }
  }
}