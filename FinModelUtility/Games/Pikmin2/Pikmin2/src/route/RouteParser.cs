using System.Numerics;

using fin.data.nodes;
using fin.schema.vector;
using fin.util.asserts;

namespace games.pikmin2.route;

public class RouteParser {
  public IGraphNode<IRouteGraphNodeData>[] Parse(StreamReader streamReader) {
      var lines = new List<string>();
      string? rawLine;
      while ((rawLine = streamReader.ReadLine()) != null) {
        var commentIndex = rawLine.IndexOf('#');
        if (commentIndex == 0) {
          continue;
        }

        var upToComment =
            (commentIndex > -1 ? rawLine[..commentIndex] : rawLine)
            .Trim();
        if (upToComment.Length > 0) {
          lines.Add(upToComment);
        }
      }

      var lineIndex = 0;

      var nodeCount = int.Parse(lines[lineIndex++]);

      var nodes
          = Enumerable.Range(0, nodeCount)
                      .Select(_ => new GraphNode<IRouteGraphNodeData>())
                      .ToArray();

      for (var i = 0; i < nodeCount; ++i) {
        Asserts.SequenceEqual("{", lines[lineIndex++]);
        var nodeIndex = int.Parse(lines[lineIndex++]);
        var node = nodes[nodeIndex];

        var linkCount = int.Parse(lines[lineIndex++]);
        for (var l = 0; l < linkCount; ++l) {
          var otherNodeIndex = int.Parse(lines[lineIndex++]);
          var otherNode = nodes[otherNodeIndex];

          // If it's two-way, the other node will also have this as an index.
          node.AddOneWayLinkTo(otherNode);
        }

        var floats = lines[lineIndex++]
                     .Split(' ')
                     .Select(float.Parse)
                     .ToArray();
        node.Value = new RouteGraphNodeData {
            Index = nodeIndex,
            Position = new Vector3(floats.AsSpan(0, 3)),
            Radius = floats[3],
        };

        Asserts.SequenceEqual("}", lines[lineIndex++]);
      }

      return nodes;
    }
}