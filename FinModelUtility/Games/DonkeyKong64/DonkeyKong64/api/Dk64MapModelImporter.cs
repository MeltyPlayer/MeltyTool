using dk64.schema.map;

using f3dzex2.combiner;
using f3dzex2.displaylist;
using f3dzex2.displaylist.opcodes;
using f3dzex2.displaylist.opcodes.f3dzex2;
using f3dzex2.image;
using f3dzex2.io;
using f3dzex2.model;

using fin.data.dictionaries;
using fin.io;
using fin.model;
using fin.model.io;
using fin.model.io.importers;
using fin.util.sets;

using schema.binary;

namespace dk64.api;

public sealed record Dk64MapModelFileBundle(
    IReadOnlyTreeFile MapFile,
    IReadOnlyTreeDirectory TexturesDirectory)
    : IModelFileBundle<Dk64MapModelFileBundle, Dk64MapModelImporter> {
  public IReadOnlyTreeFile MainFile => this.MapFile;
}

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/magcius/noclip.website/blob/main/src/DonkeyKong64/scenes.ts#L715
/// </summary>
public sealed class Dk64MapModelImporter
    : IModelImporter<Dk64MapModelImporter, Dk64MapModelFileBundle> {
  public IModel Import(Dk64MapModelFileBundle fileBundle) {
    using var mapBr = fileBundle.MapFile.OpenReadAsBinary(Endianness.BigEndian);
    var map = mapBr.ReadNew<Map>();

    var mapSectionByMeshId
        = map.MapSections.ToListDictionary(s => (uint) s.MeshId);

    var displayListTuples
        = new List<(MapChunk mapChunk, long dlStart, int vertStartIndex)>();

    foreach (var mapChunk in map.MapChunks) {
      for (var iDL = 0; iDL < 4; iDL++) {
        var dlOffsetAndSize = mapChunk.DlTable[iDL];
        var dlOffset = dlOffsetAndSize.Offset;
        var dlSize = dlOffsetAndSize.Size;
        if (dlOffset != -1 && dlSize != 0) {
          var snoopPresent = false;

          var currF3dexOffset = map.DlStart + dlOffset;
          var currF3dexCnt = dlSize;
          do {
            mapBr.Position = currF3dexOffset;
            var command = mapBr.ReadByte();

            // Load vertex segment buffer?
            if (command == 0x00) {
              snoopPresent = true;

              mapBr.Position = currF3dexOffset + 4;
              var sectionId = mapBr.ReadUInt32();

              if (mapSectionByMeshId.TryGetList(
                      sectionId,
                      out var mapSections)) {
                // TODO: What to do with the other sections?
                displayListTuples.Add(
                    (mapChunk, currF3dexOffset - map.DlStart,
                     (mapChunk.VertexOffsetAndSize.Offset / 0x10) +
                     mapSections[0].VertexOffsets[iDL]));
              }
            }

            currF3dexOffset += 8;
            currF3dexCnt -= 8;
          } while (currF3dexCnt > 0);

          if (!snoopPresent) {
            // More than 5 segments to mapChunk
            // Include Start as DL
            displayListTuples.Add(
                (mapChunk,
                 dlOffset,
                 mapChunk.VertexOffsetAndSize.Offset / 0x10));
          }
        }
      }
    }

    var n64Hardware = new N64Hardware<SlicedN64Memory>() {
        IgnoreZMode = true
    };
    var n64Memory
        = n64Hardware.Memory = new SlicedN64Memory(fileBundle.MapFile);
    var tmem = new NoclipTmem(n64Hardware);
    var rdp = n64Hardware.Rdp = new Rdp {
        Tmem = tmem
    };
    var rsp = n64Hardware.Rsp = new Rsp();

    var files = fileBundle.MapFile.AsFileSet();
    var dlModelBuilder = new DlModelBuilder(n64Hardware, fileBundle, files);

    var dlReader = new DisplayListReader();
    var opcodeParser = new F3dzex2OpcodeParser();

    foreach (var (mapChunk, dlStart, vertStartIndex) in displayListTuples) {
      var vertexStart = (uint) (map.VertStart + vertStartIndex * 0x10);
      n64Memory.SetSegment(
          6,
          new SliceSegmentChunk {
              OffsetInRom = vertexStart,
              Length = map.VertEnd - vertexStart
          });
      n64Memory.SetSegment(
          7,
          new SliceSegmentChunk {
              OffsetInRom = map.DlStart,
              Length = map.DlEnd - map.DlStart
          });
      n64Memory.SetJitSegment(
          0,
          address => {
            var textureFile
                = fileBundle.TexturesDirectory.AssertGetExistingFile(
                    $"texture{address}.bin");
            files.Add(textureFile);
            // This is stupid.
            n64Hardware.DeinterleaveImages = tmem.Dxt == 0;
            return textureFile.OpenRead();
          });

      rsp.GeometryMode = GeometryMode.G_SHADE | GeometryMode.G_LIGHTING;
      rdp.OtherModeL = 0x0C192078;
      /*rspState.gDPSetOtherModeH(OtherModeH_Layout.G_MDSFT_TEXTFILT, 2, TextFilt.G_TF_BILERP << OtherModeH_Layout.G_MDSFT_TEXTFILT);
      // some objects seem to assume this gets set, might rely on stage rendering first*/
      rdp.Tmem.GsDpSetTile(
          N64ColorFormat.RGBA,
          BitsPerTexel._16BPT,
          0,
          0x100,
          (TileDescriptorIndex) 5,
          0,
          0,
          0,
          0,
          0,
          0,
          0);
      rdp.CycleType = CycleType.TWO_CYCLE;
      rdp.ZMode = ZMode.ZMODE_XLU;

      var displayList = dlReader.ReadDisplayList(
          n64Memory,
          opcodeParser,
          (uint) (0x07000000 | dlStart));

      dlModelBuilder.AddDl(displayList);
    }

    return dlModelBuilder.Model;
  }
}