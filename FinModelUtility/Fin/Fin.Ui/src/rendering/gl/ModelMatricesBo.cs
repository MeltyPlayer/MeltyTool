using System.Numerics;

namespace fin.ui.rendering.gl;

public interface IModelMatricesBo : IDisposable {
  void UpdateData(in Matrix4x4 modelMatrix,
                  ReadOnlySpan<Matrix4x4> boneMatrices);

  void Bind();
}

public static partial class ModelMatricesBo {
  public static IModelMatricesBo New(int boneCount)
    => OpenGlFeatureService.SupportsSsbos
        ? new ModelMatricesSsbo(boneCount)
        : new ModelMatricesUbo(boneCount);
}