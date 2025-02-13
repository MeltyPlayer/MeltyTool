using schema.readOnly;

namespace fin.math.xyz;

[GenerateReadOnly]
public partial interface IXyz {
  new float X { get; set; }
  new float Y { get; set; }
  new float Z { get; set; }
}