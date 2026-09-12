using Avalonia.Controls;
using Avalonia.Headless.NUnit;

using fin.model.impl;
using fin.util.asserts;

using uni.ui.avalonia.helpers;
using uni.ui.avalonia.resources.model;

using Assert = NUnit.Framework.Assert;

namespace uni.ui.avalonia.resources.texture;

public class TextureListTests {
  [AvaloniaTest]
  public void TestSortsTexturesByName() {
    var model = ModelImpl.CreateForViewer();

    var mm = model.MaterialManager;
    foreach (var name in new[]
                 { "foo", "bar", "xyz", "item 1", "item 10", "item 2", }) {
      var texture = mm.CreateTexture(ModelDesignerUtil.CreateStubImage(32, 32));
      texture.Name = name;
    }

    var textureList = TextureList.Bootstrap(new TextureListViewModel {
        ModelAndTextures = (model, mm.Textures)
    });

    Asserts.SequenceEqual(
        [
            "bar",
            "foo",
            "item 1",
            "item 2",
            "item 10",
            "xyz"
        ],
        textureList.GetTextureNames());
  }
}