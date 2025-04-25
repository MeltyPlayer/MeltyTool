using System;
using System.IO;

using CommunityToolkit.HighPerformance;

using fin.util.asserts;

using NUnit.Framework;

using schema.binary;

namespace fin.schema.vector;

internal partial class Vector3sTests {
  [Test]
  public void TestWritesAsExpectedVector3s() {
    var vec3s = new Vector3s();
    vec3s.X = 123;
    vec3s.Y = 456;
    vec3s.Z = 789;

    var bw = new SchemaBinaryWriter();
    vec3s.Write(bw);

    var ms = new MemoryStream();
    bw.CompleteAndCopyTo(ms);

    Asserts.SpansEqual<short>(ms.ToArray().AsSpan().Cast<byte, short>(),
    [
        123,
        456,
        789,
    ]);
  }

  [Test]
  public void TestReadsAsExpectedVector3s() {
    short[] shorts = [123, 456, 789];
    var ms = new MemoryStream(shorts.AsSpan().AsBytes().ToArray());
 
    var br = new SchemaBinaryReader(ms);
    var vec3s = br.ReadNew<Vector3s>();

    Asserts.Equal(123, vec3s.X);
    Asserts.Equal(456, vec3s.Y);
    Asserts.Equal(789, vec3s.Z);
  }
}