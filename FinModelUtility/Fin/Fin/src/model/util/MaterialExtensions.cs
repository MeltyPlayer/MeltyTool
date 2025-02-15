using fin.image;
using fin.io;

namespace fin.model.util;

public static class MaterialExtensions {
  public static (ITextureMaterial, ITexture) AddSimpleTextureMaterialFromFile(
      this IMaterialManager materialManager,
      IReadOnlyGenericFile file) {
    var image = FinImage.FromFile(file);
    var texture = materialManager.CreateTexture(image);
    var material = materialManager.AddTextureMaterial(texture);
    return (material, texture);
  }
}