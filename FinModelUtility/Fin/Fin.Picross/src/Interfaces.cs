using fin.data;

using schema.readOnly;

namespace fin.picross;

[GenerateReadOnly]
public partial interface IPicrossDefinition : IGrid<bool> {
  string Name { get; set; }
}