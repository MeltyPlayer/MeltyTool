using System.Numerics;

using Avalonia.Headless.NUnit;

using fin.model.impl;
using fin.util.asserts;

using Material.Icons;

using uni.ui.avalonia.helpers;

namespace uni.ui.avalonia.resources.model.mesh;

public class MeshesPanelTests {
  [AvaloniaTest]
  public void TestDisplaysMeshesInExpectedOrder() {
    var model = ModelImpl.CreateForViewer();

    var skin = model.Skin;

    var v = skin.AddVertex(Vector3.Zero);

    var foo = skin.AddMesh();
    foo.Name = "foo";

    var primitiveA = foo.AddTriangles(v, v, v);
    var primitiveB = foo.AddTriangleStrip(v, v, v);

    var bar = foo.AddSubMesh();
    bar.Name = "bar";

    var primitiveC = bar.AddPoints(v, v);
    var primitiveD = bar.AddLines(v, v);

    var abc = skin.AddMesh();
    abc.Name = "abc";

    var primitiveE = abc.AddQuads(v, v, v, v);

    var xyz = abc.AddSubMesh();
    xyz.Name = "xyz";

    var viewModel = new MeshesPanelViewModel {
        Model = model,
    };
    viewModel.Impl?.ExpandAll();

    var meshesPanel = MeshesPanel.Bootstrap(viewModel);

    var fullHierarchyTree = meshesPanel.Single<FullHierarchyTree>();

    var values = fullHierarchyTree.GetVisibleIconsAndText().ToArray();

    Asserts.SequenceEqual(
        [
            (MaterialIconKind.ShapeOutline, "foo", [
                (MaterialIconKind.VectorTriangle, "Primitive 0 [TRIANGLES, 3 vertices]"),
                (MaterialIconKind.VectorTriangle, "Primitive 1 [TRIANGLE_STRIP, 3 vertices]"),
                (MaterialIconKind.ShapeOutline, "bar", [
                    (MaterialIconKind.VectorPoint, "Primitive 2 [POINTS, 2 vertices]"),
                    (MaterialIconKind.VectorLine, "Primitive 3 [LINES, 2 vertices]"),
                ]),
            ]),
            (MaterialIconKind.ShapeOutline, "abc", [
                (MaterialIconKind.Rectangle, "Primitive 4 [QUADS, 4 vertices]"),
                (MaterialIconKind.ShapeOutline, "xyz"),
            ]),
        ],
        fullHierarchyTree.GetVisibleIconsAndText());
  }
}