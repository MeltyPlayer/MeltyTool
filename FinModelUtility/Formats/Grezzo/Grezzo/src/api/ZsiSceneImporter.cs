using System.Drawing;
using System.IO;
using System.Linq;

using fin.color;
using fin.io;
using fin.scene;
using fin.schema.vector;

using grezzo.schema.zsi;

using schema.binary;

namespace grezzo.api;

public class ZsiSceneImporter : ISceneImporter<ZsiSceneFileBundle> {
  private readonly CmbModelBuilder cmbModelBuilder_ = new();

  public IScene Import(ZsiSceneFileBundle sceneFileBundle) {
      var zsiFile = sceneFileBundle.ZsiFile;

      var zsi = zsiFile.ReadNew<Zsi>(Endianness.LittleEndian);

      var finScene = new SceneImpl {
          FileBundle = sceneFileBundle,
          Files = sceneFileBundle.Files.ToHashSet(),
      };

      var finLighting = finScene.CreateLighting();
      if (zsi.EnvironmentSettings.Any()) {
        var zsiLightSettings = zsi.EnvironmentSettings.First();

        finLighting.AmbientLightColor = zsiLightSettings.SceneAmbientColor;
        finLighting.AmbientLightStrength = 1;

        var light0 = finLighting.CreateLight();
        light0.SetColor(zsiLightSettings.LightColor0);
        light0.SetNormal(zsiLightSettings.LightNormal0);

        var light1 = finLighting.CreateLight();
        light1.SetColor(zsiLightSettings.LightColor1);
        light1.SetNormal(zsiLightSettings.LightNormal1);
      } else {
        var colorWhite = FinColor.FromSystemColor(Color.White);

        finLighting.AmbientLightColor = colorWhite;
        finLighting.AmbientLightStrength = .5f;

        var light0 = finLighting.CreateLight();
        light0.SetColor(colorWhite);
        light0.SetNormal(new Vector3f() {
            X = 0.57715f,
            Y = 0.57715f,
            Z = 0.57715f
        });

        var light1 = finLighting.CreateLight();
        light1.SetColor(FinColor.FromRgbFloats(0, .2f, .3f));
        light1.SetNormal(new Vector3f() {
            X = -0.57715f,
            Y = -0.57715f,
            Z = -0.57715f
        });
      }

      this.AddZsiMeshAsNewArea_(sceneFileBundle, finScene, zsi);

      foreach (var roomFileName in zsi.RoomFileNames) {
        var roomZsiFile = zsiFile.AssertGetParent()
                                 .AssertGetExistingFile(
                                     Path.GetFileName(roomFileName));
        var roomZsi = roomZsiFile.ReadNew<Zsi>(Endianness.LittleEndian);
        this.AddZsiMeshAsNewArea_(sceneFileBundle, finScene, roomZsi);
      }

      return finScene;
    }

  private void AddZsiMeshAsNewArea_(ZsiSceneFileBundle fileBundle,
                                    IScene finScene,
                                    Zsi zsi) {
      var finArea = finScene.AddArea();
      foreach (var meshHeader in zsi.MeshHeaders) {
        foreach (var meshEntry in meshHeader.MeshEntries) {
          var finObject = finArea.AddObject();

          var opaqueMesh = meshEntry.OpaqueMesh;
          if (opaqueMesh != null) {
            finObject.AddSceneModel(
                this.cmbModelBuilder_.BuildModel(fileBundle, opaqueMesh));
          }

          var translucentMesh = meshEntry.TranslucentMesh;
          if (translucentMesh != null) {
            finObject.AddSceneModel(
                this.cmbModelBuilder_.BuildModel(fileBundle, translucentMesh));
          }
        }
      }
    }
}