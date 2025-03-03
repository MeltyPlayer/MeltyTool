using fin.util.asserts;

using schema.binary;

namespace modl.schema.res.texr;

public class TexrSection : IBinaryConvertible {
  private enum TexrMode {
    BW1,
    BW2
  }

  public string FileName { get; private set; }

  public List<BwTexrFile> Textures { get; } = [];

  public void Read(IBinaryReader br) {
    SectionHeaderUtil.AssertNameAndReadSize(
        br, "TEXR", out var texrLength);
    var expectedTexrSectionEnd = br.Position + texrLength;

    this.FileName = br.ReadString(br.ReadInt32());

    SectionHeaderUtil.ReadNameAndSize(
        br, out var textureSectionName, out var btfLength);
    var mode = textureSectionName switch {
        "XBTF" => TexrMode.BW1,
        "GBTF" => TexrMode.BW2,
        _      => throw new NotSupportedException(),
    };

    var expectedBtfEnd = br.Position + btfLength;

    Asserts.Equal(expectedTexrSectionEnd, expectedBtfEnd);

    this.Textures.Clear();
    var textureCount = br.ReadUInt32();

    var sectionName = mode switch {
        TexrMode.BW1 => "TEXT",
        TexrMode.BW2 => "GTXD",
        _            => throw new ArgumentOutOfRangeException()
    };
    var textureNameLength = mode switch {
        TexrMode.BW1 => 0x10,
        TexrMode.BW2 => 0x20,
        _            => throw new ArgumentOutOfRangeException()
    };

    for (var i = 0; i < textureCount; ++i) {
      var baseOffset = br.Position;
      SectionHeaderUtil.AssertNameAndReadSize(
          br, sectionName, out var textureLength);
      var endOffset = br.Position + textureLength;

      var textureName = br.ReadString(textureNameLength);

      br.Position = baseOffset;
      var data = br.ReadBytes(endOffset - baseOffset);

      this.Textures.Add(new BwTexrFile(textureName, data));
    }

    Asserts.Equal(expectedTexrSectionEnd, br.Position);
  }

  public void Write(IBinaryWriter bw) {
    throw new NotImplementedException();
  }
}

public record BwTexrFile(string Name, byte[] Data);