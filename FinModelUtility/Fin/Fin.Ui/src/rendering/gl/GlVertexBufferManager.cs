using fin.data;
using fin.model;
using fin.shaders.glsl;

using OpenTK.Graphics.OpenGL4;

namespace fin.ui.rendering.gl;

public partial interface IGlBufferManager : IDisposable {
  int VaoId { get; }
}

public interface IDynamicGlBufferManager : IGlBufferManager {
  void UpdateBuffer();
}

public sealed partial class GlVertexBufferManager : IDynamicGlBufferManager {
  private readonly IReadOnlyModel model_;
  private readonly IModelRequirements modelRequirements_;
  private readonly BufferUsageHint bufferType_;
  private readonly VertexArrayObject vao_;

  public int VaoId => this.vao_.VaoId;

  public static IGlBufferManager CreateStatic(
      IReadOnlyModel model,
      IModelRequirements modelRequirements)
    => new GlVertexBufferManager(model,
                           modelRequirements,
                           BufferUsageHint.StaticDraw);

  public static IDynamicGlBufferManager CreateDynamic(
      IReadOnlyModel model,
      IModelRequirements modelRequirements)
    => new GlVertexBufferManager(model,
                           modelRequirements,
                           BufferUsageHint.DynamicDraw);

  private GlVertexBufferManager(IReadOnlyModel model,
                          IModelRequirements modelRequirements,
                          BufferUsageHint bufferType) {
    this.model_ = model;
    this.modelRequirements_ = modelRequirements;
    this.bufferType_ = bufferType;
    this.vao_ = vaoCache_.GetAndIncrement(
        (this.model_, this.modelRequirements_, bufferType));
  }

  ~GlVertexBufferManager() => this.ReleaseUnmanagedResources_();

  public void Dispose() {
    this.ReleaseUnmanagedResources_();
    GC.SuppressFinalize(this);
  }

  private void ReleaseUnmanagedResources_() {
    vaoCache_.DecrementAndMaybeDispose((this.model_,
                                        this.modelRequirements_,
                                        this.bufferType_));
  }

  private static ReferenceCountCacheDictionary<
          (IReadOnlyModel, IModelRequirements, BufferUsageHint),
          VertexArrayObject>
      vaoCache_ = new(modelAndBufferType => new VertexArrayObject(
                          modelAndBufferType.Item1,
                          modelAndBufferType.Item2,
                          modelAndBufferType.Item3),
                      (_, vao) => vao.Dispose());

  public void UpdateBuffer() => this.vao_.UpdateBuffer();
}