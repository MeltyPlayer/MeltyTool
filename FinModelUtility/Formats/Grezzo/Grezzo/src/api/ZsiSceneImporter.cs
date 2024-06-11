using fin.io;
using fin.scene;

using grezzo.schema.zsi;

using schema.binary;

namespace grezzo.api {
  public class ZsiSceneImporter : ISceneImporter<ZsiSceneFileBundle> {
    public IScene Import(ZsiSceneFileBundle sceneFileBundle) {
      var zsiFile = sceneFileBundle.ZsiFile;

      var zsi = zsiFile.ReadNew<Zsi>(Endianness.LittleEndian);

      var finScene = new SceneImpl();

      var cmbModelBuilder = new CmbModelBuilder();
      foreach (var meshHeader in zsi.MeshHeaders) {
        var finArea = finScene.AddArea();

        foreach (var meshEntry in meshHeader.MeshEntries) {
          var finObject = finArea.AddObject();

          var opaqueMesh = meshEntry.OpaqueMesh;
          if (opaqueMesh != null) {
            finObject.AddSceneModel(cmbModelBuilder.BuildModel(opaqueMesh));
          }

          var translucentMesh = meshEntry.TranslucentMesh;
          if (translucentMesh != null) {
            finObject.AddSceneModel(
                cmbModelBuilder.BuildModel(translucentMesh));
          }
        }
      }

      return finScene;
    }
  }
}