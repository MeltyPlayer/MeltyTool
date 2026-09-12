using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Threading;

using fin.model.impl;

using uni.ui.avalonia.helpers;
using uni.ui.avalonia.resources.model;

using Assert = NUnit.Framework.Assert;

namespace uni.ui.avalonia.resources.texture;

public class TexturesPanelTests {
  [AvaloniaTest]
  public void UpdatesSelectedTextureOnClick() {
    var model = ModelImpl.CreateForViewer();

    var mm = model.MaterialManager;
    foreach (var name in new[] { "foo", "bar", "xyz" }) {
      var texture = mm.CreateTexture(ModelDesignerUtil.CreateStubImage(32, 32));
      texture.Name = name;
    }

    var texturesPanel = new TexturesPanel {
        ViewModel = new TexturesPanelViewModel {
            ModelAndTextures = (model, mm.Textures)
        }
    };

    var window = new Window { Content = texturesPanel };
    window.Show();

    var groupBox = texturesPanel.Single<GroupBox>();

    // Selects first name based on alphabetical order.
    Assert.AreEqual("bar", groupBox.Header);

    texturesPanel.ClickText("foo");
    Assert.AreEqual("foo", groupBox.Header);

    texturesPanel.ClickText("xyz");
    Assert.AreEqual("xyz", groupBox.Header);
  }
}