using fin.util.enumerables;

namespace vrml.schema;

public static class VrmlExtensions {
  public static IEnumerable<INode> GetAllChildren(this IGroupNode root)
    => root.Children.SelectMany(node => node switch {
        IGroupNode groupNode => groupNode.GetAllChildren(),
        _                    => node.Yield()
    });
}