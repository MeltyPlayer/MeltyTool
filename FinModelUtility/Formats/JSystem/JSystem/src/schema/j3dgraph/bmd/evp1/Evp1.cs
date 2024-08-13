using System;
using System.Linq;

using fin.schema.data;
using fin.schema.matrix;

using schema.binary;
using schema.binary.attributes;


namespace jsystem.schema.j3dgraph.bmd.evp1;

[BinarySchema]
[LocalPositions]
[Endianness(Endianness.BigEndian)]
public partial class Evp1 : IBinaryConvertible {
  private readonly AutoStringMagicUInt32SizedSection<Evp1Data> impl_ =
      new("EVP1") {TweakReadSize = -8};

  [Skip]
  public Evp1Data Data => this.impl_.Data;
}

[BinarySchema]
public partial class Evp1Data : IBinaryConvertible {
  private ushort envelopeCount_;
  private readonly ushort padding_ = ushort.MaxValue;

  [WPointerTo(nameof(envelopeSizes_))]
  private uint envelopeSizeOffset_;

  [WPointerTo(nameof(envelopeIndices_))]
  private uint envelopeIndicesOffset_;

  [WPointerTo(nameof(envelopeWeights_))]
  private uint envelopeWeightsOffset_;

  [WPointerTo(nameof(InverseBindMatrices))]
  private uint inverseBindMatricesOffset_;

  [RSequenceLengthSource(nameof(envelopeCount_))]
  [RAtPosition(nameof(envelopeSizeOffset_))]
  private byte[] envelopeSizes_;

  [Skip]
  private int TotalEnvelopeSize => this.envelopeSizes_.Sum(i => i);

  [RAtPosition(nameof(envelopeIndicesOffset_))]
  [RSequenceLengthSource(nameof(TotalEnvelopeSize))]
  private ushort[] envelopeIndices_;

  [RAtPosition(nameof(envelopeWeightsOffset_))]
  [RSequenceLengthSource(nameof(TotalEnvelopeSize))]
  private float[] envelopeWeights_;

  [Skip]
  private int MaxInverseBindMatrixIndex => this.envelopeIndices_.Length > 0
      ? this.envelopeIndices_.Max() + 1
      : 0;

  [RAtPosition(nameof(inverseBindMatricesOffset_))]
  [RSequenceLengthSource(nameof(MaxInverseBindMatrixIndex))]
  public Matrix3x4f[] InverseBindMatrices { get; set; }

  [Skip]
  public MultiMatrix[] WeightedIndices { get; private set; }

  [ReadLogic]
  private void SetUpWeightedIndices_(IBinaryReader br) {
    this.WeightedIndices = new MultiMatrix[this.envelopeCount_];

    var envelopeI = 0;
    for (var i = 0; i < this.envelopeCount_; ++i) {
      var envelopeSize = this.envelopeSizes_[i];

      this.WeightedIndices[i] = new MultiMatrix {
          Indices = this.envelopeIndices_.AsSpan()
                        .Slice(envelopeI, envelopeSize)
                        .ToArray(),
          Weights = this.envelopeWeights_.AsSpan()
                        .Slice(envelopeI, envelopeSize)
                        .ToArray(),
      };

      envelopeI += envelopeSize;
    }
  }

  public class MultiMatrix {
    public ushort[] Indices { get; init; }
    public float[] Weights { get; init; }
  }
}