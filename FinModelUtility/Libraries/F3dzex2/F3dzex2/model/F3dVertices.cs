using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;

using f3dzex2.displaylist.opcodes;
using f3dzex2.image;

using fin.math.matrix.four;
using fin.model;
using fin.model.impl;
using fin.util.enums;

namespace f3dzex2.model;

public interface IF3dVertices {
  void ClearVertices();

  void LoadVertices(IReadOnlyList<F3dVertex> vertices, int startIndex);
  F3dVertex GetVertexDefinition(int index);
  IVertex GetOrCreateVertexAtIndex(byte index);

  Color OverrideVertexColor { get; set; }
  Matrix4x4 Matrix { get; set; }
}

public class F3dVertices(IN64Hardware n64Hardware, ModelImpl model)
    : IF3dVertices {
  private const int VERTEX_COUNT = 32;

  private readonly F3dVertex[] vertexDefinitions_ =
      new F3dVertex[VERTEX_COUNT];

  private readonly IVertex?[] vertices_ = new IVertex?[VERTEX_COUNT];

  private readonly IReadOnlyBoneWeights?[] boneWeights_
      = new IReadOnlyBoneWeights?[VERTEX_COUNT];

  private readonly Color[] overrideVertexColors_ = new Color[VERTEX_COUNT];

  private readonly Matrix4x4[] matrices_ = new Matrix4x4[VERTEX_COUNT];


  public void ClearVertices() {
    Array.Fill(this.vertices_, null);
    Array.Fill(this.boneWeights_, null);
    Array.Fill(this.overrideVertexColors_, Color.White);
    Array.Fill(this.matrices_, Matrix4x4.Identity);
  }

  public void LoadVertices(IReadOnlyList<F3dVertex> newVertices,
                           int startIndex) {
    for (var i = 0; i < newVertices.Count; ++i) {
      var index = startIndex + i;
      this.vertexDefinitions_[index] = newVertices[i];
      this.vertices_[index] = null;
      this.boneWeights_[index] = n64Hardware.Rsp.ActiveBoneWeights;
      this.overrideVertexColors_[index] = this.OverrideVertexColor;
      this.matrices_[index] = this.Matrix;
    }
  }


  public F3dVertex GetVertexDefinition(int index)
    => this.vertexDefinitions_[index];

  public IVertex GetOrCreateVertexAtIndex(byte index) {
    var existing = this.vertices_[index];
    if (existing != null) {
      return existing;
    }

    var definition = this.vertexDefinitions_[index];

    var matrix = this.matrices_[index];

    var position = definition.GetPosition();
    ProjectionUtil.ProjectPosition(matrix, ref position);

    var materialParams = n64Hardware.Rdp.Tmem.GetMaterialParams();
    var textureParams0 = materialParams.TextureParams0;
    var textureParams1 = materialParams.TextureParams1;
    var bmpWidth0 = Math.Max(textureParams0?.Width ?? 0, (ushort) 0);
    var bmpHeight0 = Math.Max(textureParams0?.Height ?? 0, (ushort) 0);
    var bmpWidth1 = Math.Max(textureParams1?.Width ?? 0, (ushort) 0);
    var bmpHeight1 = Math.Max(textureParams1?.Height ?? 0, (ushort) 0);

    var newVertex = model.Skin.AddVertex(position);
    newVertex.SetUv(0, definition.GetUv(
                        n64Hardware.Rsp.TexScaleXFloat /
                        (bmpWidth0 * 32),
                        n64Hardware.Rsp.TexScaleYFloat /
                        (bmpHeight0 * 32)));
    newVertex.SetUv(1, definition.GetUv(
                        n64Hardware.Rsp.TexScaleXFloat /
                        (bmpWidth1 * 32),
                        n64Hardware.Rsp.TexScaleYFloat /
                        (bmpHeight1 * 32)));

    var activeBoneWeights = this.boneWeights_[index];
    if (activeBoneWeights != null) {
      newVertex.SetBoneWeights(activeBoneWeights);
    }

    if (n64Hardware.Rsp.GeometryMode.CheckFlag(
            GeometryMode.G_LIGHTING)) {
      var normal = definition.GetNormal();
      ProjectionUtil.ProjectNormal(matrix, ref normal);
      newVertex.SetLocalNormal(normal);
      // TODO: Get rid of this, seems to come from combiner instead
      newVertex.SetColor(this.overrideVertexColors_[index]);
    } else {
      newVertex.SetColor(definition.GetColor());
    }

    this.vertices_[index] = newVertex;
    return newVertex;
  }

  public Color OverrideVertexColor { get; set; } = Color.White;
  public Matrix4x4 Matrix { get; set; } = Matrix4x4.Identity;
}