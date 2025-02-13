using schema.readOnly;

namespace fin.math.xy;

[GenerateReadOnly]
public partial interface IXy {
  new float X { get; set; }
  new float Y { get; set; }
}