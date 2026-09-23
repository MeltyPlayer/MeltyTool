using System.Collections.Generic;

using fin.io;
using fin.scene;

namespace uni.ui.avalonia.resources.scene;

public static class SceneDesignerUtil {
  public static IReadOnlyScene CreateStubScene() {
    var scene = new SceneImpl {
        FileBundle = null,
        Files = new HashSet<IReadOnlyStandaloneFile>([
            new FinFile(@"C:\Users\Foo\Documents\Bar\123.txt"),
            new FinFile(@"C:\Users\Foo\Documents\Bar\123_model.model"),
            new FinFile(@"C:\Users\Foo\Documents\Bar\123_animation.anim"),
            new FinFile(@"C:\Users\Foo\Documents\Bar\textures\abc.png"),
        ])
    };

    var area0 = scene.AddArea();
    var nodeFoo = area0.AddRootNode();
    nodeFoo.Name = "foo";
    var nodeBar = nodeFoo.AddChildNode();
    nodeBar.Name = "bar";

    var nodeAbc = area0.AddRootNode();
    nodeAbc.Name = "abc";
    var nodeXyz = nodeAbc.AddChildNode();
    nodeXyz.Name = "xyz";

    var area1 = scene.AddArea();
    var node123 = area1.AddRootNode();
    node123.Name = "123";

    return scene;
  }
}