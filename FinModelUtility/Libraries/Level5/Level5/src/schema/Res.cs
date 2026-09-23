using fin.io;
using fin.schema;

using level5.decompression;

using schema.binary;
using schema.binary.attributes;

namespace level5.schema;

public enum ResourceType {
  MATERIAL_1 = 220,
  MATERIAL_2 = 230,
  TEXTURE_DATA = 240,
  MATERIAL_DATA = 290,
  NULL = 9999,
}

public sealed class Resource {
  public sealed class Material {
    public string Name { get; set; }
    public int Index { get; set; } = -1;
    public string TexName { get; set; }
  }

  public string ModelName { get; set; }

  Dictionary<uint, string> ResourceNames { get; set; } =
    new Dictionary<uint, string>();

  Dictionary<uint, (ResourceType, string)> ResourceData { get; set; } = new();

  public List<string> TextureNames { get; } = [];

  public List<Material> Materials { get; } = [];

  public string GetResourceName(uint crc) {
    if (this.ResourceNames.ContainsKey(crc))
      return this.ResourceNames[crc];

    return "";
  }

  public (ResourceType, string)? GetResourceData(uint crc) {
    if (this.ResourceData.TryGetValue(crc, out (ResourceType, string) data)) {
      return data;
    }

    return null;
  }

  [Unknown]
  public Resource(IReadOnlyStandaloneFile file) {
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
        string mname = r.ReadStringNT(StringEncodingType.SHIFT_JIS);
        if (mname == "")
          break;
        if (!this.ResourceNames.ContainsKey(Crc32.Crc32C(mname)))
          this.ResourceNames.Add(Crc32.Crc32C(mname), mname);
      }

      r.Position = (uint)materialTableOffset;
      for (int i = 0; i < materialTableSectionCount; i++) {
        var offset = r.ReadInt16() << 2;
        var count = r.ReadInt16();
        var resourceType = (ResourceType) r.ReadInt16();
        var size = r.ReadInt16();

        if (resourceType == ResourceType.NULL)
          continue;

        var temp = r.Position;
        for (int j = 0; j < count; j++) {
          r.Position = (uint)(offset + j * size);
          var key = r.ReadUInt32();
          string resourceName = (this.ResourceNames.ContainsKey(key)
              ? this.ResourceNames[key]
              : key.ToString("X"));

          this.ResourceData[key] = (resourceType, resourceName);
          
          //Console.WriteLine(resourceName + " " + unknown.ToString("X") + " " + size.ToString("X"));

          if (resourceType is ResourceType.MATERIAL_1
                              or ResourceType.MATERIAL_2) {
            // TODO: Default libs
            ;
          } else if (resourceType == ResourceType.TEXTURE_DATA) {
            this.TextureNames.Add(resourceName);
          } else if (resourceType == ResourceType.MATERIAL_DATA) {
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