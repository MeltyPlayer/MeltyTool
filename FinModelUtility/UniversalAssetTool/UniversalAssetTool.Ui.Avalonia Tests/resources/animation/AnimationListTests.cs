using Avalonia.Headless.NUnit;

using fin.model.impl;
using fin.util.asserts;

using uni.ui.avalonia.helpers;

using Assert = NUnit.Framework.Assert;

namespace uni.ui.avalonia.resources.animation;

public class AnimationListTests {
  [AvaloniaTest]
  public void TestSortsAnimationsByName() {
    var model = ModelImpl.CreateForViewer();

    var am = model.AnimationManager;
    foreach (var name in new[] { "foo", "bar", "xyz", "item 1", "item 10", "item 2", }) {
      var animation = am.AddAnimation();
      animation.Name = name;
    }

    var textureList = AnimationList.Bootstrap(new AnimationListViewModel {
        Animations = am.Animations
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
        textureList.GetAnimationNames());
  }
}