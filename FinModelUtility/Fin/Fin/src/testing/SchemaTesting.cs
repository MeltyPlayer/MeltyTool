using System.Threading.Tasks;

using NUnit.Framework;

using schema.binary;
using schema.binary.testing;

namespace fin.testing;

public static class SchemaTesting {
  public static Task<byte[]> GetSchemaBinaryWriterBytes(
      SchemaBinaryWriter bw)
    => BinarySchemaAssert.GetEndianBinaryWriterBytes(bw);

  public static async Task WritesAndReadsIdentically<T>(
      T value,
      Endianness endianess = Endianness.LittleEndian,
      bool assertExactEndPositions = true)
      where T : IBinaryConvertible, new() {
    var ew = new SchemaBinaryWriter(endianess);
    value.Write(ew);

    var actualBytes = await GetSchemaBinaryWriterBytes(ew);

    var er = new SchemaBinaryReader(actualBytes, endianess);
    await ReadsAndWritesIdentically<T>(er, assertExactEndPositions);
  }

  public static async Task ReadsAndWritesIdentically<T>(
      IBinaryReader br,
      bool assertExactEndPositions = true)
      where T : IBinaryConvertible, new() {
    var readerStartPos = br.Position;
    var instance = br.ReadNew<T>();
    var expectedReadLength = br.Position - readerStartPos;

    var ew = new SchemaBinaryWriter(br.Endianness);
    instance.Write(ew);

    var actualBytes = await GetSchemaBinaryWriterBytes(ew);

    br.Position = readerStartPos;
    var expectedBytes = br.ReadBytes(actualBytes.Length);
    CollectionAssert.AreEqual(expectedBytes, actualBytes);

    if (assertExactEndPositions) {
      Assert.AreEqual(expectedReadLength, actualBytes.Length);
    }
  }
}