using fin.image;
using fin.io;

namespace fin.model.util;

public static class MaterialExtensions {
  public static (ITextureMaterial, ITexture) AddSimpleTextureMaterialFromFile(
      this IMaterialManager materialManager,
      IReadOnlyTreeFile file)
    => materialManager.AddSimpleTextureMaterialFromImage(
        FinImage.FromFile(file),
        file.NameWithoutExtension.ToString());

  public static (ITextureMaterial, ITexture) AddSimpleTextureMaterialFromImage(
      this IMaterialManager materialManager,
      IImage image,
      string? name = null) {
    var texture = materialManager.CreateTexture(image);
    var material = materialManager.AddTextureMaterial(texture);
    texture.Name = material.Name = name;
    return (material, texture);
  }
}