using fin.compression;
using fin.io;
using fin.model;
using fin.model.impl;
using fin.model.io.importers;
using fin.util.sets;

using schema.binary;
using schema.binary.attributes;

namespace nitro.api;

public class NsbmdModelImporter : IModelImporter<NsbmdModelFileBundle> {
  public IModel Import(NsbmdModelFileBundle fileBundle) {
    var nsbmdFile = fileBundle.NsbmdFile;

    var files = nsbmdFile.AsFileSet();
    var model = new ModelImpl {
        FileBundle = fileBundle,
        Files = files,
    };

    var nsbmdData
        = new Lz77Decompressor().Decompress(nsbmdFile.OpenReadAsBinary());

    var ms = new MemoryStream(nsbmdData);
    var br = new SchemaBinaryReader(ms);
    var nsbmd = br.ReadNew<Nsbmd>();

    return model;
  }
}

[BinarySchema]
public partial class Nsbmd : IBinaryDeserializable {
  public uint SectionSize { get; set; }
  public uint RenderCommandsOffset { get; set; }
  public uint MaterialsOffset { get; set; }
  public uint PiecesOffset { get; set; }
  public uint InverseBindsOffset { get; set; }

  [SequenceLengthSource(3)]
  public byte[] Unknown1 { get; set; }

  public byte NumObjects { get; set; }
  public byte NumMaterials { get; set; }
  public byte NumPieces { get; set; }

  [SequenceLengthSource(2)]
  public byte[] Unknown2 { get; set; }

  // TODO: Fixed-point float
  public uint UpScale { get; set; }

  // TODO: Fixed-point float
  public uint DownScale { get; set; }

  public ushort NumVerts { get; set; }
  public ushort NumSurfs { get; set; }
  public ushort NumTris { get; set; }
  public ushort NumQuads { get; set; }

  // TODO: Fixed-point half
  public ushort BoundingBoxXMin { get; set; }
  public ushort BoundingBoxYMin { get; set; }
  public ushort BoundingBoxZMin { get; set; }
  public ushort BoundingBoxXMax { get; set; }
  public ushort BoundingBoxYMax { get; set; }
  public ushort BoundingBoxZMax { get; set; }

  [SequenceLengthSource(8)]
  public byte[] Unknown3 { get; set; }
}