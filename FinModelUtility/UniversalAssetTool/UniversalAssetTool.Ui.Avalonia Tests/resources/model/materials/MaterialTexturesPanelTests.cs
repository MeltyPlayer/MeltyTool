using Avalonia.Headless.NUnit;

using fin.model.impl;

using uni.ui.avalonia.helpers;

using Assert = NUnit.Framework.Assert;

namespace uni.ui.avalonia.resources.model.materials;

public class MaterialTexturesPanelTests {
  [AvaloniaTest]
  public void TestUpdatesSelectedTextureOnClick() {
    var model = ModelImpl.CreateForViewer();

    var mm = model.MaterialManager;
    foreach (var name in new[] { "foo", "bar", "xyz" }) {
      var texture = mm.CreateTexture(ModelDesignerUtil.CreateStubImage(32, 32));
      texture.Name = name;
    }

    var materialTexturesPanel = MaterialTexturesPanel.Bootstrap(
        new MaterialTexturesPanelViewModel {
            ModelAndTextures = (model, mm.Textures)
        });

    // Selects first name based on alphabetical order.
    Assert.AreEqual("bar", materialTexturesPanel.GetSelectedTextureName());

    materialTexturesPanel.ClickText("foo");
    Assert.AreEqual("foo", materialTexturesPanel.GetSelectedTextureName());

    materialTexturesPanel.ClickText("xyz");
    Assert.AreEqual("xyz", materialTexturesPanel.GetSelectedTextureName());
  }
}