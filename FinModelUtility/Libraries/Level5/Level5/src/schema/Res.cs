using fin.io;
using fin.schema;

using level5.decompression;

using schema.binary;

namespace level5.schema;

public class Resource {
  public class Material {
    public string Name { get; set; }
    public int Index { get; set; } = -1;
    public string TexName { get; set; }
  }

  public string ModelName { get; set; }

  Dictionary<uint, string> ResourceNames { get; set; } =
    new Dictionary<uint, string>();

  public List<string> TextureNames { get; } = [];

  public List<Material> Materials { get; } = [];

  public string GetResourceName(uint crc) {
    if (this.ResourceNames.ContainsKey(crc))
      return this.ResourceNames[crc];

    return "";
  }

  [Unknown]
  public Resource(IReadOnlyGenericFile file) {
    var data = file.ReadAllBytes();
    data = new Level5Decompressor().Decompress(data);
    using (var r = new SchemaBinaryReader(new MemoryStream(data),
                                          Endianness.LittleEndian)) {
      var magic = new string(r.ReadChars(6));
      if (magic != "CHRC00" && magic != "CHRN01")
        throw new FormatException("RES file is corrupt");

      // -----------------------
      var unknown0 = r.ReadInt16();
      var stringTableOffset = r.ReadInt16() << 2;
      var unknown1 = r.ReadInt16();
      var materialTableOffset = r.ReadInt16() << 2;
      var materialTableSectionCount = r.ReadInt16();
      var resourceNodeOffsets = r.ReadInt16() << 2;
      var resourceNodeCount = r.ReadInt16();

      r.Position = (uint)stringTableOffset;
      while (!r.Eof) {
        string mname = r.ReadStringNT();
        if (mname == "")
          break;
        if (!this.ResourceNames.ContainsKey(Crc32.Crc32C(mname)))
          this.ResourceNames.Add(Crc32.Crc32C(mname), mname);
      }

      r.Position = (uint)materialTableOffset;
      for (int i = 0; i < materialTableSectionCount; i++) {
        var offset = r.ReadInt16() << 2;
        var count = r.ReadInt16();
        var unknown = r.ReadInt16();
        var size = r.ReadInt16();

        if (unknown == 0x270F)
          continue;

        var temp = r.Position;
        for (int j = 0; j < count; j++) {
          r.Position = (uint)(offset + j * size);
          var key = r.ReadUInt32();
          string resourceName = (this.ResourceNames.ContainsKey(key)
              ? this.ResourceNames[key]
              : key.ToString("X"));
          //Console.WriteLine(resourceName + " " + unknown.ToString("X") + " " + size.ToString("X"));

          if (unknown is 0xDC or 0xE6) {
            // TODO: Default libs
            ;
          } else if (unknown == 0xF0) {
            this.TextureNames.Add(resourceName);
          } else if (unknown == 0x122) {
            Material mat = new Material();
            mat.Name = resourceName;
            r.Position += 12;
            key = r.ReadUInt32();
            resourceName = (this.ResourceNames.ContainsKey(key)
                ? this.ResourceNames[key]
                : key.ToString("X"));
            mat.TexName = resourceName;
            // Console.WriteLine(resourceName + " " + unknown.ToString("X") + " " + size.ToString("X"));
            this.Materials.Add(mat);
          } else {
            ;
          }
        }

        r.Position = temp;
      }

      r.Position = (uint)resourceNodeOffsets;
      for (int i = 0; i < resourceNodeCount; i++) {
        var offset = r.ReadInt16() << 2;
        var count = r.ReadInt16();
        var unknown = r.ReadInt16();
        var size = r.ReadInt16();

        if (unknown == 0x270F)
          continue;

        var temp = r.Position;
        r.Position = (uint)offset;
        for (int j = 0; j < count; j++) {
          var key = r.ReadUInt32();
          //Console.WriteLine((ResourceNames.ContainsKey(key) ? ResourceNames[key] : key.ToString("X")) + " " + unknown.ToString("X") + " " + size.ToString("X"));
          r.Position += (uint)(size - 4);
        }

        r.Position = temp;
      }
    }
  }
}