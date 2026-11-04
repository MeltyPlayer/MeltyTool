using System.Numerics;

using schema.binary;

namespace mp64.schema.form;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/PartyPlanner64/PartyPlanner64/blob/980d043217823d0e5901ee93d3a50e36f728db10/packages/lib/models/FORM.ts#L324
/// </summary>
[BinarySchema]
public sealed partial class Transform : IBinaryConvertible {
  public Vector3 Position { get; set; }
  public Vector3 Rotation { get; set; }
  public Vector3 Scale { get; set; }
}