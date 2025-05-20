using fin.data;

namespace fin.picross;

public interface IPicrossDefinition : IReadOnlyGrid<bool> {
  string Name { get; set; }
}