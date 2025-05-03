using schema.readOnly;

namespace fin.model;

[GenerateReadOnly]
public partial interface INamed {
  string Name { get; set; }
}