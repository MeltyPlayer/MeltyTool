using schema.readOnly;

namespace fin.math.xyz;

[GenerateReadOnly]
public partial interface IXyz {
  float X { get; set; }
  float Y { get; set; }
  float Z { get; set; }
}