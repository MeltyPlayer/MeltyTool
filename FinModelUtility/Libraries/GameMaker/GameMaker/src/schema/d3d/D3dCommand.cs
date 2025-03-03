using fin.schema;

using schema.text;
using schema.text.reader;

namespace gm.schema.d3d;

public enum D3dCommandType {
  BEGIN,
  END,
  VERTEX,
  VERTEX_COLOR,
  VERTEX_TEXTURE,
  VERTEX_TEXTURE_COLOR,
  VERTEX_NORMAL,
  VERTEX_NORMAL_COLOR,
  VERTEX_NORMAL_TEXTURE,
  VERTEX_NORMAL_TEXTURE_COLOR,
  BLOCK,
  CYLINDER,
  CONE,
  ELLIPSOID,
  WALL,
  FLOOR,
}

public enum D3dPrimitiveType {
  POINT_LIST = 1,
  LINE_LIST,
  LINE_STRIP,
  TRIANGLE_LIST,
  TRIANGLE_STRIP,
  TRIANGLE_FAN
}

public struct D3dCommand : ITextDeserializable {
  public D3dCommandType CommandType { get; private set; }
  public float[] Parameters { get; private set; }

  public void Read(ITextReader tr) {
    this.CommandType = (D3dCommandType) tr.ReadInt32();
    this.Parameters = tr.ReadSingles(
        TextReaderConstants.WHITESPACE_STRINGS,
        TextReaderConstants.NEWLINE_STRINGS);
  }
}