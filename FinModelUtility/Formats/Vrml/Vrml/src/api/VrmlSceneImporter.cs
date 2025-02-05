using System.Drawing;
using System.Numerics;

using fin.color;
using fin.math;
using fin.scene;
using fin.schema.vector;
using fin.ui;
using fin.util.linq;
using fin.util.sets;

using vrml.schema;

namespace vrml.api;

public class VrmlSceneImporter : ISceneImporter<VrmlSceneFileBundle> {
  public IScene Import(VrmlSceneFileBundle fileBundle) {
    var wrlFile = fileBundle.WrlFile.Impl;
    using var wrlFileStream = wrlFile.OpenRead();
    var fileSet = fileBundle.WrlFile.AsFileSet();

    var finScene = new SceneImpl { FileBundle = fileBundle, Files = fileSet };

    var area = finScene.AddArea();
    var obj = area.AddObject();

    var vrmlScene = VrmlParser.Parse(wrlFileStream);
    var allVrmlNodes = vrmlScene.GetAllChildren();

    if (allVrmlNodes.TryGetFirstWhereIs<INode, IBackgroundNode>(
            out var backgroundNode)) {
      var skyColor = backgroundNode.SkyColor;
      var r = (byte) (skyColor.X * 255);
      var g = (byte) (skyColor.Y * 255);
      var b = (byte) (skyColor.Z * 255);
      area.BackgroundColor = Color.FromArgb(255, r, g, b);

      area.CreateCustomSkyboxObject();
    }

    var finLighting = finScene.CreateLighting();

    var hasHeadlight = true;
    if (hasHeadlight) {
      var headlight = finLighting.CreateLight();
      headlight.Strength = .7f;

      var camera = Camera.Instance;

      var position = new Vector3f();
      var normal = new Vector3f();

      var lightingOwner = area.AddObject();
      lightingOwner.SetOnTickHandler(_ => {
        position.X = camera.X;
        position.Y = camera.Y;
        position.Z = camera.Z;
        headlight.SetPosition(position);

        normal.X = camera.XNormal;
        normal.Y = camera.YNormal;
        normal.Z = camera.ZNormal;
        headlight.SetNormal(normal);
      });
    }

    if (allVrmlNodes.TryGetWhereIs<INode, IDirectionalLightNode>(
            out var directionalLightNodes)) {
      if (directionalLightNodes.Length == 1) {
        finLighting.AmbientLightStrength
            = directionalLightNodes[0].AmbientIntensity;
      } else {
        var ambientDirectionalLight = directionalLightNodes.SingleOrDefault(d => d.Direction.IsRoughly0());
        if (ambientDirectionalLight != null) {
          directionalLightNodes = directionalLightNodes
                                  .Where(d => d != ambientDirectionalLight)
                                  .ToArray();
          finLighting.AmbientLightStrength = ambientDirectionalLight.Intensity;
          finLighting.AmbientLightColor = FinColor.FromRgb(ambientDirectionalLight.Color);
        }
      }


      // TODO: Handle child lights, inherit transforms?
      foreach (var directionalLightNode in directionalLightNodes) {
        var finLight = finLighting.CreateLight();
        finLight.SetColor(FinColor.FromRgb(directionalLightNode.Color));
        finLight.SetNormal(Vector3.Normalize(directionalLightNode.Direction));
        finLight.Strength = directionalLightNode.Intensity / directionalLightNodes.Length;
      }
    }

    obj.AddSceneModel(
        new VrmlModelImporter().Import(vrmlScene, fileBundle, fileSet));

    return finScene;
  }
}