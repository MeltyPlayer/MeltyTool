using System.Collections.Generic;

namespace fin.model.impl;

public partial class ModelImpl<TVertex> {
  private partial class MaterialManagerImpl {
    public IShaderMaterial AddShaderMaterial(string vertexShader, string fragmentShader) {
      var material = new ShaderMaterialImpl(vertexShader, fragmentShader);
      this.materials_.Add(material);
      return material;
    }
  }

  private class ShaderMaterialImpl(string vertexShader, string fragmentShader)
      : BMaterialImpl, IShaderMaterial {
    public string VertexShader { get; set; } = vertexShader;
    public string FragmentShader { get; set; } = fragmentShader;
    public override IEnumerable<ITexture> Textures => [];
  }
}