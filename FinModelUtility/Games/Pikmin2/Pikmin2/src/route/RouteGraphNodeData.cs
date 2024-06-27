using fin.data.indexable;
using fin.math.xyz;

namespace games.pikmin2.route;

public interface IRouteGraphNodeData : IIndexable {
  IXyz Position { get; }
  float Radius { get; }
}

public class RouteGraphNodeData : IRouteGraphNodeData {
  public required int Index { get; init; }
  public required IXyz Position { get; init; }
  public required float Radius { get; init; }
}