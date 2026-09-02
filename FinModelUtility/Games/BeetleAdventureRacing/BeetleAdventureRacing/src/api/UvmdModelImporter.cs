using System.Drawing;
using System.Numerics;

using bar.schema;

using f3dzex2.combiner;
using f3dzex2.displaylist;
using f3dzex2.displaylist.opcodes;
using f3dzex2.displaylist.opcodes.f3dzex2;
using f3dzex2.image;
using f3dzex2.io;
using f3dzex2.model;

using fin.data.lazy;
using fin.io;
using fin.io.bundles;
using fin.model;
using fin.model.impl;
using fin.model.io;
using fin.model.io.importers;
using fin.model.util;
using fin.util.enums;
using fin.util.sets;

using schema.binary;

namespace bar.api;

using UvtxData
    = (Uvtx uvtx0, Uvtx? uvtx1, IReadOnlyList<(byte, ISegmentChunk)> segments,
    IDisplayList displayList);

public sealed record UvmdModelFileBundle(
    IReadOnlyTreeFile MainFile,
    IReadOnlyTreeDirectory RootDirectory)
    : IModelFileBundle;

public sealed class UvmdModelFileImporter
    : IModelImporter<UvmdModelFileBundle> {
  public IModel Import(UvmdModelFileBundle fileBundle)
    => Import(fileBundle, true);

  public static IModel
      Import(UvmdModelFileBundle fileBundle, bool fixRotation) {
    var fileChunks
        = fileBundle.MainFile.ReadNew<FileChunks>(Endianness.BigEndian);
    if (fileChunks.Chunks.Count == 0) {
      return new ModelImpl {
          FileBundle = fileBundle,
          Files = fileBundle.MainFile.AsFileSet(),
      };
    }

    var uvmd
        = new SchemaBinaryReader(fileChunks.Chunks[0].Buffer,
                                 Endianness.BigEndian).ReadNew<Uvmd>();
    return FromMaterialMeshes(
        fileBundle,
        fileBundle.RootDirectory,
        uvmd.Lods[0].Billboard,
        uvmd.Lods[0].ModelParts.Select(p => p.MaterialMeshes),
        true,
        uvmd.Transforms);
  }

  public static IModel FromMaterialMeshes(
      IFileBundle fileBundle,
      IReadOnlyTreeDirectory rootDirectory,
      bool isBillboard,
      IEnumerable<UvmdMaterialMesh> materialMeshes,
      bool fixRotation)
    => FromMaterialMeshes(fileBundle,
                          rootDirectory,
                          isBillboard,
                          [materialMeshes],
                          fixRotation,
                          null);

  public static IModel FromMaterialMeshes(
      IFileBundle fileBundle,
      IReadOnlyTreeDirectory rootDirectory,
      bool isBillboard,
      IEnumerable<IEnumerable<UvmdMaterialMesh>> materialMeshesByBone,
      bool fixRotation,
      Matrix4x4[]? boneMatrices) {
    var files = fileBundle.MainFile.AsFileSet();
    var n64Hardware = new N64Hardware<SeparateN64Memory> {
        DeinterleaveImages = true
    };
    var n64Memory = n64Hardware.Memory = new SeparateN64Memory();
    var rdp = n64Hardware.Rdp = new Rdp {
        Tmem = new NoclipTmem(n64Hardware),
    };
    var rsp = n64Hardware.Rsp = new Rsp {
        GeometryMode = GeometryMode.G_LIGHTING
    };
    var dlModelBuilder = new DlModelBuilder(n64Hardware, fileBundle, files);
    var finModel = dlModelBuilder.Model;

    var finSkeletonRoot = finModel.Skeleton.Root;
    if (fixRotation) {
      finSkeletonRoot.Transform.LocalRotation
          = Quaternion.CreateFromYawPitchRoll(0, -MathF.PI / 2, 0);
    }

    if (isBillboard) {
      // TODO: What does this need to be?
    }

    var needsToUseBoneWeights = fixRotation || isBillboard;

    var finSkin = finModel.Skin;
    var allFinBoneWeights
        = (boneMatrices ?? [])
          .Select(transform => finSkeletonRoot.AddChild(transform))
          .Select(bone => finSkin.GetOrCreateBoneWeights(
                      VertexSpace.RELATIVE_TO_BONE,
                      bone))
          .ToArray();

    var uvtxByIndex = new LazyDictionary<uint, Uvtx>(uvtxIndex => {
      var uvtxFile
          = rootDirectory.AssertGetExistingFile($"uvtx/{uvtxIndex}.uvtx");

      var fileChunks
          = uvtxFile.ReadNew<FileChunks>(Endianness.BigEndian);
      return new SchemaBinaryReader(fileChunks.Chunks[0].Buffer,
                                    Endianness.BigEndian).ReadNew<Uvtx>();
    });

    var uvtxDataByIndex = new LazyDictionary<uint, UvtxData>(uvtxIndex => {
      var uvtx0 = uvtxByIndex[uvtxIndex];
      var uvtx1 = (uvtx0.OtherUvtxIndex != 0xFFF &&
                   uvtx0.OtherUvtxIndex != (uvtx0.FlagsAndIndex & 0xFFF))
          ? uvtxByIndex[uvtx0.OtherUvtxIndex]
          : null;

      var displayList = new DisplayListReader().ReadDisplayList(
          n64Memory,
          new F3dzex2OpcodeParser(),
          new SchemaBinaryReader(uvtx0.DlCommandsData,
                                 Endianness.BigEndian));

      Span<uint> uvtxAddresses = stackalloc uint[2];
      uvtxAddresses[0] = 0;
      uvtxAddresses[1] = 0x01000000;

      Uvtx?[] uvtxes = new Uvtx?[2];
      uvtxes[0] = uvtx0;
      uvtxes[1] = uvtx1;

      displayList = StubDisplayList_(displayList, uvtxAddresses, uvtxes);

      var segments = new List<(byte, ISegmentChunk)>();
      segments.Add((0, new BytesSegmentChunk {
          OffsetInSegment = 0,
          Bytes = uvtx0.TexelData,
      }));
      if (uvtx1 != null) {
        segments.Add((1, new BytesSegmentChunk {
            OffsetInSegment = 0,
            Bytes = uvtx1.TexelData,
        }));
      }

      if (uvtx0.PalettesData != null) {
        segments.Add((2, new BytesSegmentChunk {
            OffsetInSegment = 0,
            Bytes = uvtx0.PalettesData.SelectMany(p => p).ToArray()
        }));
      }

      return (uvtx0, uvtx1, segments, displayList);
    });

    var i = 0;
    foreach (var materialMeshesForBone in materialMeshesByBone) {
      if (boneMatrices != null) {
        rsp.ActiveBoneWeights = allFinBoneWeights[i];
      } else if (needsToUseBoneWeights) {
        rsp.ActiveBoneWeights
            = finSkin.GetOrCreateBoneWeights(VertexSpace.RELATIVE_TO_BONE,
                                             finSkeletonRoot);
      }

      foreach (var uvmdMaterialMesh in materialMeshesForBone) {
        var uvtxData = SetUpMaterial_(dlModelBuilder,
                                      uvmdMaterialMesh,
                                      uvtxDataByIndex,
                                      n64Hardware.Memory,
                                      rsp,
                                      rdp);
        dlModelBuilder.AddDl(uvmdMaterialMesh.DisplayList);
      }
    }

    return finModel;
  }

  /// <summary>
  ///   Shamelessly stolen from:
  ///   https://github.com/magcius/noclip.website/blob/main/src/BeetleAdventureRacing/MaterialRenderer.ts#L163
  /// </summary>
  private static UvtxData? SetUpMaterial_(
      DlModelBuilder dlModelBuilder,
      UvmdMaterialMesh materialMesh,
      ILazyDictionary<uint, UvtxData> uvtxDataByIndex,
      ISeparateN64Memory memory,
      IRsp rsp,
      IRdp rdp) {
    var renderOpts = materialMesh.RenderOptions;
    var isTextured = materialMesh.UvtxIndex != 0xFFF;

    var otherModeLRenderMode = (uint) 0;
    if (renderOpts.CheckFlag(RenderOptions.UNK_18)) {
      var m = (uint) (renderOpts &
                      (RenderOptions.UNK_17 | RenderOptions.UNK_16));
      if (m == 0)
        otherModeLRenderMode = 0x00112e10;
      if (m == 0x400000)
        otherModeLRenderMode = 0x00112d58;
      if (m == 0x800000)
        otherModeLRenderMode = 0x00104e50;
      if (m == 0xc00000)
        otherModeLRenderMode = 0x00104dd8;
    } else if (renderOpts.CheckFlag(RenderOptions.UNK_17)) {
      var m = (uint) (renderOpts &
                      (RenderOptions.UNK_16 |
                       RenderOptions.ENABLE_DEPTH_CALCULATIONS));

      Uvtx? uvtx = isTextured
          ? uvtxDataByIndex[materialMesh.UvtxIndex]
              .Item1
          : null;
      var usesAlphaBlending = uvtx?.BlendAlpha != 0xff;

      if (m == 0) {
        if (!isTextured)
          otherModeLRenderMode = 0x00104340;
        else
          otherModeLRenderMode = 0x00104240;
      }

      if (m == 0x200000) {
        if (!isTextured)
          otherModeLRenderMode = 0x00104b50;
        else if (usesAlphaBlending)
          otherModeLRenderMode = 0x00105278;
        else
          otherModeLRenderMode = 0x00104a50;
      }

      if (m == 0x400000) {
        if (!isTextured)
          otherModeLRenderMode = 0x001041c8;
        else if ( /* TODO: complicated flag checks */ false)
          otherModeLRenderMode = 0x00103048;
        else
          otherModeLRenderMode = 0x001041c8;
      }

      if (m == 0x600000) {
        if (!isTextured)
          otherModeLRenderMode = 0x001045d8;
        else if (usesAlphaBlending)
          otherModeLRenderMode = 0x00105278;
        else if ( /* TODO: complicated flag checks */ false)
          otherModeLRenderMode = 0x00103078;
        else
          otherModeLRenderMode = 0x001049d8;
      }
    } else {
      var m = (uint) (renderOpts &
                      (RenderOptions.UNK_16 |
                       RenderOptions.ENABLE_DEPTH_CALCULATIONS));
      if (m == 0)
        otherModeLRenderMode = 0x03024000;
      if (m == 0x200000)
        otherModeLRenderMode = 0x00112230;
      if (m == 0x400000)
        otherModeLRenderMode = 0x00102048;
      if (m == 0x600000)
        otherModeLRenderMode = 0x00102078;
    }

    // This sets A to 0 and B to 1 in the first cycle,
    // so the first cycle equation is always just ((P * 0 + M * 1) / (0 + 1)) = (0 + M) / 1 = M
    otherModeLRenderMode |= 0x0c080000;

    rdp.OtherModeL = otherModeLRenderMode;

    var cullBack = renderOpts.CheckFlag(RenderOptions.ENABLE_BACKFACE_CULLING);
    var cullFront
        = renderOpts.CheckFlag(RenderOptions.ENABLE_FRONTFACE_CULLING);
    rsp.CullingMode = (cullFront, cullBack) switch {
        (false, false) => CullingMode.SHOW_BOTH,
        (false, true)  => CullingMode.SHOW_FRONT_ONLY,
        (true, false)  => CullingMode.SHOW_BACK_ONLY,
        (true, true)   => CullingMode.SHOW_NEITHER,
    };

    rsp.Lighting = renderOpts.CheckFlag(RenderOptions.USES_LIGHTING);
    rsp.UvType = renderOpts.CheckFlag(RenderOptions.ENABLE_TEX_GEN_SPHERICAL)
        ? N64UvType.SPHERICAL
        : N64UvType.STANDARD;
    rsp.UseRegisterForPrimColor = true;
    rsp.PrimColor = Color.White;

    rdp.CombinerCycleParams0 = null!;
    rdp.CombinerCycleParams1 = null!;

    UvtxData? uvtxData = null;
    if (isTextured) {
      uvtxData = uvtxDataByIndex[materialMesh.UvtxIndex];
      var (uvtx0, uvtx1, segments, displayList) = uvtxData.Value;
      rdp.PaletteSegmentedAddress = 0x02000000;

      foreach (var (segmentIndex, segment) in segments) {
        memory.SetSegment(segmentIndex, segment);
      }

      dlModelBuilder.AddDl(displayList);
    }

    var isTranslucent = rdp.ForceBlending;
    switch (rdp.CombinerCycleParams0 == null,
            rdp.CombinerCycleParams1 == null) {
      case (false, false): {
        rdp.CycleType = CycleType.TWO_CYCLE;
        break;
      }
      case (false, true): {
        rdp.CycleType = CycleType.ONE_CYCLE;
        break;
      }
      default: {
        rdp.SetSimpleCombinerCycleParams(isTextured, true, isTranslucent);
        break;
      }
    }

    return uvtxData;
  }

  private static IDisplayList? StubDisplayList_(
      IDisplayList? displayList,
      ReadOnlySpan<uint> newTimgAddresses,
      ReadOnlySpan<Uvtx?> uvtxes) {
    if (displayList == null) {
      return null;
    }

    TileDescriptorIndex? primitiveTileIndex = null;
    var newTimgIndex = 0;
    Span<bool> foundSetTileSizes = stackalloc bool[2];
    Span<uint?> foundTmemOffsets = stackalloc uint?[2];

    foreach (var opcode in displayList.OpcodeCommands) {
      switch (opcode) {
        case TextureOpcodeCommand textureOpcodeCommand: {
          primitiveTileIndex = textureOpcodeCommand.TileDescriptorIndex;
          break;
        }
        case SetTimgOpcodeCommand setTimgOpcodeCommand: {
          setTimgOpcodeCommand.TextureSegmentedAddress
              = newTimgAddresses[newTimgIndex++];
          break;
        }
        case SetTileOpcodeCommand setTileOpcodeCommand: {
          if (setTileOpcodeCommand.TileDescriptorIndex ==
              primitiveTileIndex) {
            foundTmemOffsets[0] = setTileOpcodeCommand.OffsetOfTextureInTmem;
          }
          else if (setTileOpcodeCommand.TileDescriptorIndex ==
                   primitiveTileIndex + 1) {
            foundTmemOffsets[1] = setTileOpcodeCommand.OffsetOfTextureInTmem;
          }
          break;
        }
        case SetTileSizeOpcodeCommand setTileSizeOpcodeCommand: {
          if (setTileSizeOpcodeCommand.TileDescriptorIndex ==
              primitiveTileIndex) {
            foundSetTileSizes[0] = true;
          }
          else if (setTileSizeOpcodeCommand.TileDescriptorIndex ==
              primitiveTileIndex + 1) {
            foundSetTileSizes[1] = true;
          }
          break;
        }
      }
    }

    // TODO: Is stubbing in the width/height really necessary, or is there a bug
    // in my TMEM code?
    var newOpcodes = new List<IOpcodeCommand>();
    newOpcodes.AddRange(displayList.OpcodeCommands.Where(o => o is not EndDlOpcodeCommand));

    if (primitiveTileIndex != null) {
      var uvtx0 = uvtxes[0];
      if (!foundSetTileSizes[0]) {
        newOpcodes.Add(new SetTileSizeOpcodeCommand {
            Uls = .5f,
            Ult = .5f,
            Lrs = uvtx0.Width - .5f,
            Lrt = uvtx0.Height - .5f,
            TileDescriptorIndex = primitiveTileIndex.Value,
        });
      }

      var uvtx1 = uvtxes[1];
      if (uvtx1 != null && !foundSetTileSizes[1]) {
        newOpcodes.Add(new SetTileSizeOpcodeCommand {
            Uls = .5f,
            Ult = .5f,
            Lrs = uvtx1.Width - .5f,
            Lrt = uvtx1.Height - .5f,
            TileDescriptorIndex = (TileDescriptorIndex) (primitiveTileIndex + 1),
        });
      } else if (foundTmemOffsets[0] == foundTmemOffsets[1] &&
                 foundTmemOffsets[0] != null) {
        newOpcodes.Add(new SetTileSizeOpcodeCommand {
            Uls = .5f,
            Ult = .5f,
            Lrs = uvtx0.Width - .5f,
            Lrt = uvtx0.Height - .5f,
            TileDescriptorIndex = (TileDescriptorIndex) (primitiveTileIndex + 1),
        });
      }
    }

    return new DisplayList {
        Type = DisplayListType.F3DZEX2,
        OpcodeCommands = newOpcodes,
    };
  }
}