using fin.model;
using fin.model.impl;
using fin.model.util;

using IronPython.Runtime;

using schema.binary;


// ReSharper disable InconsistentNaming

namespace ModelPluginWrappers.noesis.rapi;

public sealed class Rapi {
  private readonly ModelImpl model_ = ModelImpl.CreateForViewer();

  private string name_;

  private Bytes positionBuffer_;
  private Noesis.NoeFormat positionFormat_;
  private int positionStride_;
  private int positionOffset_;

  public void rpgReset() { }

  public void rpgSetName(string name) => this.name_ = name;

  public void rpgBindPositionBufferOfs(
      Bytes data,
      Noesis.NoeFormat format,
      int stride,
      int offset) {
    this.positionBuffer_ = data;
    this.positionFormat_ = format;
    this.positionStride_ = stride;
    this.positionOffset_ = offset;
  }

  public void rpgCommitTriangles(
      byte[]? indexBufferBytes,
      Noesis.NoeFormat indexDataType,
      int numIndices,
      Noesis.NoePrimitiveType primitiveType,
      bool usePlotMap) {
    if (indexBufferBytes == null) {
      this.CommitTrianglesWithoutIndices_(primitiveType, usePlotMap);
      return;
    }

    throw new NotImplementedException();
  }

  private void CommitTrianglesWithoutIndices_(
      Noesis.NoePrimitiveType primitiveType,
      bool usePlotMap) {
    switch (primitiveType) {
      case Noesis.NoePrimitiveType.RPGEO_POINTS: {
        var skin = this.model_.Skin;

        var mesh = skin.AddMesh();
        mesh.Name = this.name_;

        var vertices = new List<IVertex>();
        var bytes = this.positionBuffer_.ToArray();
        using var br = new SchemaBinaryReader(bytes);

        var i = 0;
        while (true) {
          br.Position = this.positionOffset_ + i * this.positionStride_;
          if (br.Eof) {
            break;
          }

          switch (this.positionFormat_) {
            case Noesis.NoeFormat.RPGEODATA_FLOAT: {
              var xyz = br.ReadVector3();
              vertices.Add(skin.AddVertex(xyz));
              break;
            }
            default: throw new NotImplementedException();
          }

          ++i;
        }

        mesh.AddPoints(vertices.ToArray());
        break;
      }
    }
  }

  public IModel rpgConstructModel() => this.model_;

  public object rpgCreateContext() {
    return new object();
  }
}