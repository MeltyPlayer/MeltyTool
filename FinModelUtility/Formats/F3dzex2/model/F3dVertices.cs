using System;
using System.Collections.Generic;
using System.Drawing;

using f3dzex2.displaylist.opcodes;
using f3dzex2.image;

using fin.math.matrix.four;
using fin.model;
using fin.model.impl;
using fin.model.util;
using fin.util.enums;

namespace f3dzex2.model;

public interface IF3dVertices {
  void ClearVertices();

  void LoadVertices(IReadOnlyList<F3dVertex> vertices, int startIndex);
  F3dVertex GetVertexDefinition(int index);
  IVertex GetOrCreateVertexAtIndex(byte index);

  Color DiffuseColor { get; set; }
}

public class F3dVertices(IN64Hardware n64Hardware, ModelImpl model)
    : IF3dVertices {
  private const int VERTEX_COUNT = 32;

  private readonly F3dVertex[] vertexDefinitions_ =
      new F3dVertex[VERTEX_COUNT];

  private readonly IVertex?[] vertices_ = new IVertex?[VERTEX_COUNT];

  private readonly IBone?[] bones_ = new IBone?[VERTEX_COUNT];

  private Color diffuseColor_ = Color.White;


  public void ClearVertices() => Array.Fill(this.vertices_, null);

  public void LoadVertices(IReadOnlyList<F3dVertex> newVertices,
                           int startIndex) {
      for (var i = 0; i < newVertices.Count; ++i) {
        var index = startIndex + i;
        this.vertexDefinitions_[index] = newVertices[i];
        this.vertices_[index] = null;
        this.bones_[index] = n64Hardware.Rsp.ActiveBone;
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

      var position = definition.GetPosition();
      ProjectionUtil.ProjectPosition(n64Hardware.Rsp.Matrix.Impl,
                                   ref position);

      var textureParams = n64Hardware.Rdp.Tmem.GetMaterialParams()
                    .TextureParams0;
      var bmpWidth = Math.Max(textureParams.Width, (ushort) 0);
      var bmpHeight = Math.Max(textureParams.Height, (ushort) 0);

      var newVertex = model.Skin.AddVertex(position);
      newVertex.SetUv(definition.GetUv(
                          n64Hardware.Rsp.TexScaleXFloat /
                          (bmpWidth * 32),
                          n64Hardware.Rsp.TexScaleYFloat /
                          (bmpHeight * 32)));

      var activeBone = this.bones_[index];
      if (activeBone != null) {
        newVertex.SetBoneWeights(
            model.Skin.GetOrCreateBoneWeights(
                VertexSpace.RELATIVE_TO_BONE,
                activeBone));
      }

      if (n64Hardware.Rsp.GeometryMode.CheckFlag(
              GeometryMode.G_LIGHTING)) {
        var normal = definition.GetNormal();
        ProjectionUtil.ProjectNormal(n64Hardware.Rsp.Matrix.Impl,
                                   ref normal);
        newVertex.SetLocalNormal(normal);
        // TODO: Get rid of this, seems to come from combiner instead
        newVertex.SetColor(this.DiffuseColor);
      } else {
        newVertex.SetColor(definition.GetColor());
      }

      this.vertices_[index] = newVertex;
      return newVertex;
    }

  public Color DiffuseColor {
    get => this.diffuseColor_;
    set => this.diffuseColor_ = value;
  }
}