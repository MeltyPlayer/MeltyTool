using System.Drawing;

using fin.io;
using fin.model;
using fin.model.impl;
using fin.model.util;
using fin.util.sets;

namespace games.pikmin2.route;

public class RouteModelImporter {
  public IModel Import(IReadOnlyGenericFile routeTxt) {
      using var routeReader = routeTxt.OpenReadAsText();
      var route = new RouteParser().Parse(routeReader);

      var model = new ModelImpl {
          FileBundle = null,
          Files = routeTxt.AsSet()
      };
      var skin = model.Skin;
      var mesh = skin.AddMesh();

      var routeMaterial = model.MaterialManager.AddColorMaterial(Color.Magenta);
      routeMaterial.DepthMode = DepthMode.NONE;
      routeMaterial.IgnoreLights = true;

      var linkSet = new HashSet<(int, int)>();

      // TODO: Consider making each waypoint a bone in the skeleton and weighing vertices based on them
      for (var nodeI = 0; nodeI < route.Length; ++nodeI) {
        var node = route[nodeI];

        // Adds link to set
        foreach (var other in node.ChildNodes) {
          var otherI = other.Value.Index;

          var min = Math.Min(nodeI, otherI);
          var max = Math.Max(nodeI, otherI);

          linkSet.Add((min, max));
        }

        // Adds circle
        var nodeValue = node.Value;
        var center = nodeValue.Position;
        var radius = nodeValue.Radius;

        var vertices = new IVertex[30];
        vertices[0] = skin.AddVertex(center);
        for (var i = 0; i < vertices.Length - 1; ++i) {
          var frac = (1f * i) / (vertices.Length - 2);

          var angle = 2 * MathF.PI * frac;
          vertices[1 + i] = skin.AddVertex(
              center.X + radius * MathF.Cos(angle),
              center.Y,
              center.Z + radius * MathF.Sin(angle));
        }

        var triangleFan = mesh.AddTriangleFan(vertices);
        triangleFan.SetMaterial(routeMaterial);
      }

      var lineMaterial = model.MaterialManager.AddColorMaterial(Color.Magenta);
      lineMaterial.DepthMode = DepthMode.NONE;
      lineMaterial.IgnoreLights = true;

      var links = linkSet.ToArray();
      var lineVertices = new IVertex[4 * links.Length];
      for (var i = 0; i < links.Length; ++i) {
        var (nodeI, otherI) = links[i];

        var nodeValue = route[nodeI].Value;
        var otherNodeValue = route[otherI].Value;

        var nodePos = nodeValue.Position;
        var otherNodePos = otherNodeValue.Position;

        var nodeR = nodeValue.Radius;
        var otherNodeR = otherNodeValue.Radius;

        var toDir = MathF.Atan2(otherNodePos.Z - nodePos.Z,
                                otherNodePos.X - nodePos.X);
        var rightDir = toDir - MathF.PI / 2;

        var rightX = MathF.Cos(rightDir);
        var rightY = MathF.Sin(rightDir);

        lineVertices[4 * i + 0] = skin.AddVertex(
            nodePos.X + nodeR * rightX,
            nodePos.Y,
            nodePos.Z + nodeR * rightY);
        lineVertices[4 * i + 1] = skin.AddVertex(
            nodePos.X - nodeR * rightX,
            nodePos.Y,
            nodePos.Z - nodeR * rightY);
        lineVertices[4 * i + 2] = skin.AddVertex(
            otherNodePos.X - otherNodeR * rightX,
            otherNodePos.Y,
            otherNodePos.Z - otherNodeR * rightY);
        lineVertices[4 * i + 3] = skin.AddVertex(
            otherNodePos.X + otherNodeR * rightX,
            otherNodePos.Y,
            otherNodePos.Z + otherNodeR * rightY);
      }

      var lines = mesh.AddQuads(lineVertices);
      lines.SetVertexOrder(VertexOrder.NORMAL)
           .SetMaterial(lineMaterial);

      return model;
    }
}