using System;
using System.Collections.Generic;
using System.Numerics;

using Dxt;

using fin.image;
using fin.model;
using fin.model.impl;

using System.Runtime.CompilerServices;

using Microsoft.Toolkit.HighPerformance.Helpers;


namespace HaloWarsTools {
  public class HWXtdResource : HWBinaryResource {
    public IModel Mesh { get; private set; }

    public IImage AmbientOcclusionTexture { get; private set; }
    public IImage OpacityTexture { get; private set; }

    public static new HWXtdResource FromFile(HWContext context, string filename)
      => GetOrCreateFromFile(context, filename, HWResourceType.Xtd) as
          HWXtdResource;

    protected override void Load(byte[] bytes) {
      base.Load(bytes);

      this.Mesh = this.ImportMesh(bytes);

      this.AmbientOcclusionTexture = ExtractEmbeddedDXT5A(bytes,
        GetFirstChunkOfType(HWBinaryResourceChunkType.XTD_AOChunk));
      this.OpacityTexture =
          ExtractEmbeddedDXT5A(
              bytes,
              GetFirstChunkOfType(HWBinaryResourceChunkType.XTD_AlphaChunk));
    }

    private IImage ExtractEmbeddedDXT5A(byte[] bytes,
                                        HWBinaryResourceChunk chunk) {
      // Get raw embedded DXT5 texture from resource file
      var width = (int) Math.Sqrt(chunk.Size * 2);
      var height = width;

      // For some godforsaken reason, every pair of bytes is flipped so we need
      // to fix it here. This was really annoying to figure out, haha.
      for (var i = 0; i < chunk.Size; i += 2) {
        var offset = (int) chunk.Offset + i;

        var byte0 = bytes[offset + 0];
        var byte1 = bytes[offset + 1];

        bytes[offset + 0] = byte1;
        bytes[offset + 1] = byte0;
      }

      return DxtDecoder.DecompressDxt5a(bytes,
                                        (int) chunk.Offset,
                                        width,
                                        height);
    }

    private IModel ImportMesh(byte[] bytes) {
      MeshNormalExportMode shadingMode = MeshNormalExportMode.Unchanged;

      HWBinaryResourceChunk headerChunk =
          GetFirstChunkOfType(HWBinaryResourceChunkType.XTD_XTDHeader);
      float tileScale =
          BinaryUtils.ReadFloatBigEndian(bytes,
                                         (int) headerChunk.Offset + 12);
      HWBinaryResourceChunk atlasChunk =
          GetFirstChunkOfType(HWBinaryResourceChunkType.XTD_AtlasChunk);

      int gridSize =
          (int) Math.Round(Math.Sqrt((atlasChunk.Size - 32) /
                                     8)); // Subtract the min/range vector sizes, divide by position + normal size, and sqrt for grid size
      int positionOffset = (int) atlasChunk.Offset + 32;
      int normalOffset = positionOffset + gridSize * gridSize * 4;

      // These are stored as ZYX, 4 bytes per component
      Vector3 posCompMin = BinaryUtils
                           .ReadVector3BigEndian(
                               bytes,
                               (int) atlasChunk.Offset)
                           .ReverseComponents();
      Vector3 posCompRange =
          BinaryUtils
              .ReadVector3BigEndian(bytes, (int) atlasChunk.Offset + 16)
              .ReverseComponents();

      var finModel = new ModelImpl(gridSize * gridSize);
      var finMesh = finModel.Skin.AddMesh();

      var finVertices = finModel.Skin.Vertices;

      // Read vertex offsets/normals and add them to the mesh
      ParallelHelper.For(0,
                         finVertices.Count,
                         new GridVertexGenerator(
                             bytes,
                             finVertices,
                             gridSize,
                             tileScale,
                             positionOffset,
                             normalOffset,
                             posCompMin,
                             posCompRange));

      // Generate faces based on terrain grid
      var triangleGridSize = gridSize - 1;
      var triangles = new IVertex[2 * 3 * triangleGridSize * triangleGridSize];
      for (int x = 0; x < triangleGridSize; ++x) {
        for (int z = 0; z < triangleGridSize; ++z) {
          var a = finVertices[GetVertexIndex(x, z, gridSize)];
          var b = finVertices[GetVertexIndex(x + 1, z, gridSize)];
          var c = finVertices[GetVertexIndex(x, z + 1, gridSize)];
          var d = finVertices[GetVertexIndex(x + 1, z + 1, gridSize)];

          var i = 2 * 3 * (triangleGridSize * z + x);

          triangles[i + 0] = a;
          triangles[i + 1] = c;
          triangles[i + 2] = b;

          triangles[i + 3] = d;
          triangles[i + 4] = b;
          triangles[i + 5] = c;
        }
      }

      finMesh.AddTriangles(triangles)
             .SetVertexOrder(VertexOrder.NORMAL);

      return finModel;
    }

    private readonly struct GridVertexGenerator : IAction {
      private readonly byte[] bytes_;
      private readonly IReadOnlyList<IVertex> vertices_;
      private readonly int gridSize_;
      private readonly float tileScale_;
      private readonly int positionOffset_;
      private readonly int normalOffset_;
      private readonly Vector3 posCompMin_;
      private readonly Vector3 posCompRange_;

      public GridVertexGenerator(
          byte[] bytes,
          IReadOnlyList<IVertex> vertices,
          int gridSize,
          float tileScale,
          int positionOffset,
          int normalOffset,
          Vector3 posCompMin,
          Vector3 posCompRange) {
        this.bytes_ = bytes;
        this.vertices_ = vertices;
        this.gridSize_ = gridSize;
        this.tileScale_ = tileScale;
        this.positionOffset_ = positionOffset;
        this.normalOffset_ = normalOffset;
        this.posCompMin_ = posCompMin;
        this.posCompRange_ = posCompRange;
      }

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public void Invoke(int index) {
        var x = index % this.gridSize_;
        var z = (index - x) / this.gridSize_;

        int offset = index * 4;

        // Get offset position and normal for this vertex
        Vector3 position =
            ReadVector3Compressed(this.bytes_, this.positionOffset_ + offset) *
            this.posCompRange_ -
            this.posCompMin_;

        // Positions are relative to the terrain grid, so shift them by the grid position
        position += new Vector3(x, 0, z) * this.tileScale_;

        Vector3 normal =
            ConvertDirectionVector(
                Vector3.Normalize(
                    ReadVector3Compressed(this.bytes_,
                                          this.normalOffset_ + offset) *
                    2.0f -
                    Vector3.One));

        // Simple UV based on original, non-warped terrain grid
        Vector3 texCoord = new Vector3(x / ((float) this.gridSize_ - 1),
                                       z / ((float) this.gridSize_ - 1),
                                       0);

        var vertex = this.vertices_[index];
        vertex.SetLocalPosition(Unsafe.As<Vector3, Position>(ref position))
              .SetLocalNormal(Unsafe.As<Vector3, Normal>(ref normal))
              .SetUv(texCoord.X, texCoord.Y);
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetVertexIndex(int x, int z, int gridSize)
      => z * gridSize + x;

    private const uint K_BIT_MASK_10 = (1 << 10) - 1;

    private const float INVERSE_K_BIT_MASK_10 =
        1f / HWXtdResource.K_BIT_MASK_10;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector3 ReadVector3Compressed(byte[] bytes, int offset) {
      // Inexplicably, position and normal vectors are encoded inside 4 bytes. ~10 bits per component
      // This seems okay for directions, but positions suffer from stairstepping artifacts
      uint v = BinaryUtils.ReadUInt32LittleEndian(bytes, offset);
      uint x = (v >> 0) & K_BIT_MASK_10;
      uint y = (v >> 10) & K_BIT_MASK_10;
      uint z = (v >> 20) & K_BIT_MASK_10;
      return new Vector3(x, y, z) * INVERSE_K_BIT_MASK_10;
    }

    private static Vector3 ConvertPositionVector(Vector3 vector) {
      return new Vector3(vector.X, -vector.Z, vector.Y);
    }

    // TODO: This might not be right
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector3 ConvertDirectionVector(Vector3 vector) {
      return new Vector3(vector.Z, vector.X, vector.Y);
    }
  }
}