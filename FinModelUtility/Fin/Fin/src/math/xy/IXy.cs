using schema.readOnly;

namespace fin.math.xy;

[GenerateReadOnly]
public partial interface IXy {
  float X { get; set; }
  float Y { get; set; }
}