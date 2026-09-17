using System.Numerics;

using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using Avalonia.VisualTree;

using fin.math.xyz;
using fin.model.impl;
using fin.model.util;
using fin.util.asserts;

using Material.Icons;

using uni.ui.avalonia.helpers;

namespace uni.ui.avalonia.resources.model.skeleton;

public class SkeletonPanelTests {
  [AvaloniaTest]
  public void TestDisplaysBonesInExpectedOrder() {
    var model = ModelImpl.CreateForViewer();

    var root = model.Skeleton.Root;

    var foo = root.AddChild(Vector3.Zero);
    foo.Name = "foo";

    var bar = foo.AddChild(Vector3.Zero);
    bar.Name = "bar";

    var abc = root.AddChild(Vector3.Zero);
    abc.Name = "abc";

    var xyz = abc.AddChild(Vector3.Zero);
    xyz.Name = "xyz";

    var skeletonPanel = SkeletonPanel.Bootstrap(new SkeletonPanelViewModel {
        Model = model,
    });

    Asserts.SequenceEqual(
        [
            (MaterialIconKind.CardsDiamondOutline, "foo"),
            (MaterialIconKind.CardsDiamondOutline, "bar"),
            (MaterialIconKind.CardsDiamondOutline, "abc"),
            (MaterialIconKind.CardsDiamondOutline, "xyz"),
        ],
        skeletonPanel.Single<FullHierarchyTree>().GetAllIconsAndText());
  }
}