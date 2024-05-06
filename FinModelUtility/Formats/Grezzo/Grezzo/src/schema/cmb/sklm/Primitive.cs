using System;

using fin.util.strings;

using schema.binary;

namespace grezzo.schema.cmb.sklm {
  public class Primitive : IBinaryConvertible {
    public uint chunkSize;
    public bool isVisible;
    public PrimitiveMode primitiveMode;
    public DataType dataType;
    public ushort indicesCount;
    public uint[] indices;
    public ushort offset { get; set; }

    public void Read(IBinaryReader br) {
      br.AssertString("prm" + AsciiUtil.GetChar(0x20));

      this.chunkSize = br.ReadUInt32();
      this.isVisible = br.ReadUInt32() != 0;

      // Other modes don't exist in OoT3D's shader so we'd never know
      this.primitiveMode = (PrimitiveMode) br.ReadUInt32();
      this.dataType = (DataType) br.ReadUInt32();

      this.indicesCount = br.ReadUInt16();

      this.offset = br.ReadUInt16();
    }

    public void Write(IBinaryWriter bw)
      => throw new NotImplementedException();
  }
}