using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Threading;

using fin.model.impl;

using uni.ui.avalonia.helpers;

using Assert = NUnit.Framework.Assert;

namespace uni.ui.avalonia.resources.model.materials;

public class MaterialShadersPanelTests {
  [AvaloniaTest]
  public void TestUpdatesSelectedTextureOnClick() {
    var model = ModelImpl.CreateForViewer();

    var mm = model.MaterialManager;
    var material = mm.AddShaderMaterial("foo", "bar");

    var materialTexturesPanel = MaterialShadersPanel.Bootstrap(
        new MaterialShadersPanelViewModel {
            ModelAndMaterial = (model, material)
        });

    // Shows vertex source by default
    Assert.AreEqual("foo", materialTexturesPanel.GetDisplayedCode());

    materialTexturesPanel.ClickText("Fragment");
    Assert.AreEqual("bar", materialTexturesPanel.GetDisplayedCode());

    materialTexturesPanel.ClickText("Vertex");
    Assert.AreEqual("foo", materialTexturesPanel.GetDisplayedCode());
  }
}