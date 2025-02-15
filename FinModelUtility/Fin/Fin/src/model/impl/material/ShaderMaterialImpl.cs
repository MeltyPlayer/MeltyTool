using System.Collections.Generic;

namespace fin.model.impl;

public partial class ModelImpl<TVertex> {
  private partial class MaterialManagerImpl {
    public IShaderMaterial AddShaderMaterial(string vertexShader,
                                             string fragmentShader) {
      var material = new ShaderMaterialImpl(vertexShader, fragmentShader);
      this.materials_.Add(material);
      return material;
    }
  }

  private class ShaderMaterialImpl(string vertexShader, string fragmentShader)
      : BMaterialImpl, IShaderMaterial {
    private readonly Dictionary<string, IReadOnlyTexture> textureByUniform_
        = new();

    public string VertexShader { get; set; } = vertexShader;
    public string FragmentShader { get; set; } = fragmentShader;

    public IReadOnlyDictionary<string, IReadOnlyTexture> TextureByUniform
      => this.textureByUniform_;

    public void AddTexture(string uniformName, IReadOnlyTexture texture)
      => this.textureByUniform_[uniformName] = texture;

    public override IEnumerable<IReadOnlyTexture> Textures
      => this.textureByUniform_.Values;
  }
}