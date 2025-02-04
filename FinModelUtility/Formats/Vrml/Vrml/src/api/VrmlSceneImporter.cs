using System.Drawing;

using fin.color;
using fin.scene;
using fin.util.enumerables;
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

    var children = this.GetAllChildren_(vrmlScene);

    if (children.TryGetFirstWhereIs<INode, IBackgroundNode>(
            out var backgroundNode)) {
      var skyColor = backgroundNode.SkyColor;
      var r = (byte) (skyColor.X * 255);
      var g = (byte) (skyColor.Y * 255);
      var b = (byte) (skyColor.Z * 255);
      area.BackgroundColor = Color.FromArgb(255, r, g, b);

      area.CreateCustomSkyboxObject();
    }

    if (children.TryGetWhereIs<INode, IDirectionalLightNode>(
            out var directionalLightNodes)) {
      var finLighting = finScene.CreateLighting();

      if (directionalLightNodes.Length == 1) {
        finLighting.AmbientLightStrength
            = directionalLightNodes[0].AmbientIntensity;
      }

      // TODO: Handle child lights, inherit transforms?
      foreach (var directionalLightNode in directionalLightNodes) {
        var finLight = finLighting.CreateLight();
        finLight.SetColor(FinColor.FromRgb(directionalLightNode.Color));
        finLight.SetNormal(directionalLightNode.Direction);
        finLight.Strength = directionalLightNode.Intensity;
      }
    }

    obj.AddSceneModel(
        new VrmlModelImporter().Import(vrmlScene, fileBundle, fileSet));

    return finScene;
  }

  private IEnumerable<INode> GetAllChildren_(IGroupNode root)
    => root.Children.SelectMany(node => node switch {
        IGroupNode groupNode => this.GetAllChildren_(groupNode),
        _                    => node.Yield()
    });
}