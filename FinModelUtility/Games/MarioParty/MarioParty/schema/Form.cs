using schema.binary;

namespace mp64.schema;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/PartyPlanner64/PartyPlanner64/blob/980d043217823d0e5901ee93d3a50e36f728db10/packages/lib/models/FORM.ts
/// </summary>
[BinarySchema]
public sealed partial class Form : IBinaryDeserializable {
  private readonly string magic_ = "FORM";
}