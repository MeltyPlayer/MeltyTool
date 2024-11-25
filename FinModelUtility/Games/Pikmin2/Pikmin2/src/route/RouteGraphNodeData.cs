using System.Numerics;

using fin.data.indexable;

namespace games.pikmin2.route;

public interface IRouteGraphNodeData : IIndexable {
  Vector3 Position { get; }
  float Radius { get; }
}

public class RouteGraphNodeData : IRouteGraphNodeData {
  public required int Index { get; init; }
  public required Vector3 Position { get; init; }
  public required float Radius { get; init; }
}