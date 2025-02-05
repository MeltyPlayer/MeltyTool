using fin.model;

namespace fin.shaders.glsl.source;

public class ShaderShaderSourceGlsl(IReadOnlyShaderMaterial material)
    : IShaderSourceGlsl {
  public string VertexShaderSource { get; } = material.VertexShader;
  public string FragmentShaderSource { get; } = material.FragmentShader;
}