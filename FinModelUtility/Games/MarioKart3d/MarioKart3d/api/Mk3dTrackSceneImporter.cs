using System;
using System.Collections.Generic;

using fin.io;
using fin.scene;
using fin.ui.rendering.gl.scene;
using fin.util.sets;

using gm.api;

namespace mk3d.api;

public sealed record Mk3dTrackSceneFileBundle(IReadOnlyTreeFile PlaceholderFile)
    : ISceneFileBundle {
  public IReadOnlyTreeFile MainFile => this.PlaceholderFile;
}

public sealed class Mk3dTrackSceneImporter
    : ISceneImporter<Mk3dTrackSceneFileBundle> {
  public IScene Import(Mk3dTrackSceneFileBundle fileBundle) {
    var rootDir = fileBundle.PlaceholderFile.AssertGetParent();

    var fileSet = new HashSet<IReadOnlyStandaloneFile>();
    var finScene = new SceneImpl {
        FileBundle = fileBundle,
        Files = fileSet,
    };

    var finArea = finScene.AddArea();

    var rootNode = finArea.AddRootNode();
    rootNode.Name = "root";
    rootNode.SetRotationRadians(0, 0, MathF.PI);

    var smkFiles = rootDir.GetFilesWithFileType(".smk", true);
    foreach (var smkFile in smkFiles) {
      var smkBundle = Mk3dModelFileBundleUtil.FromSmkFile(smkFile);
      var finModel = new D3dModelImporter().Import(smkBundle);
      fileSet.Add(finModel.Files);

      var finNode = rootNode.AddChildNode();
      finNode.Name = smkFile.NameWithoutExtension.ToString();

      finNode.AddComponent(new SimpleModelRenderComponent(finModel));
    }

    return finScene;
  }
}