using System.Linq;

using schema.binary;
using schema.binary.attributes;

namespace grezzo.schema.cmb.mats;

[BinarySchema]
public sealed partial class Mats : IBinaryConvertible {
  [Skip]
  public required Version Version { get; init; }

  [SequenceLengthSource(SchemaIntegerType.UINT32)]
  public Material[] Materials { get; set; }

  [Skip]
  private uint TotalCombinerCount_
    => (uint) this.Materials
                  .SelectMany(material => material.texEnvStagesIndices)
                  .Max() + 1;

  [RSequenceLengthSource(nameof(TotalCombinerCount_))]
  public Combiner[] Combiners { get; set; }
}