using dk64.schema.map;

using f3dzex2.displaylist;
using f3dzex2.displaylist.opcodes;
using f3dzex2.displaylist.opcodes.f3dzex2;
using f3dzex2.image;
using f3dzex2.io;
using f3dzex2.model;

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
    : IModelFileBundle {
  public IReadOnlyTreeFile MainFile => this.MapFile;
}

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/magcius/noclip.website/blob/main/src/DonkeyKong64/scenes.ts#L715
/// </summary>
public sealed class Dk64MapModelImporter
    : IModelImporter<Dk64MapModelFileBundle> {
  public IModel Import(Dk64MapModelFileBundle fileBundle) {
    using var mapBr = fileBundle.MapFile.OpenReadAsBinary(Endianness.BigEndian);
    var map = mapBr.ReadNew<Map>();

    var mapSectionByMeshId = map.MapSections.ToDictionary(s => (uint) s.MeshId);

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

              if (mapSectionByMeshId.TryGetValue(
                      sectionId,
                      out var mapSection)) {
                displayListTuples.Add(
                    (mapChunk, currF3dexOffset - map.DlStart,
                     (mapChunk.VertexOffsetAndSize.Offset / 0x10) +
                     mapSection.VertexOffsets[iDL]));
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

    var n64Hardware = new N64Hardware<SlicedN64Memory>();
    var n64Memory
        = n64Hardware.Memory = new SlicedN64Memory(fileBundle.MapFile);
    var rdp = n64Hardware.Rdp = new Rdp {
        Tmem = new NoclipTmem(n64Hardware),
    };
    var rsp = n64Hardware.Rsp = new Rsp();

    var files = fileBundle.MapFile.AsFileSet();
    var dlModelBuilder = new DlModelBuilder(n64Hardware, fileBundle, files);

    var dlReader = new DisplayListReader();
    var opcodeParser = new F3dzex2OpcodeParser();

    foreach (var (mapChunk, dlStart, vertStartIndex) in displayListTuples) {
      n64Memory.SetSegment(
          6,
          new SliceSegmentChunk {
              OffsetInRom = (uint) (map.VertStart + vertStartIndex * 0x10),
              Length = map.VertEnd - map.VertStart
          });
      n64Memory.SetSegment(
          7,
          new SliceSegmentChunk {
              OffsetInRom = map.DlStart,
              Length = map.DlEnd - map.DlStart
          });
      // TODO: Need to set segment 1 for texture

      rsp.GeometryMode = GeometryMode.G_SHADE | GeometryMode.G_LIGHTING;
      rdp.OtherModeL = 0x0C192078;
      /*rspState.gDPSetOtherModeH(OtherModeH_Layout.G_MDSFT_TEXTFILT, 2, TextFilt.G_TF_BILERP << OtherModeH_Layout.G_MDSFT_TEXTFILT);
      // initially 2-cycle, though this can change
      rspState.gDPSetOtherModeH(OtherModeH_Layout.G_MDSFT_CYCLETYPE, 2, OtherModeH_CycleType.G_CYC_2CYCLE << OtherModeH_Layout.G_MDSFT_CYCLETYPE);
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

      var displayList = dlReader.ReadDisplayList(
          n64Memory,
          opcodeParser,
          (uint) (0x07000000 | dlStart));

      dlModelBuilder.AddDl(displayList);
    }

    return dlModelBuilder.Model;
  }
}