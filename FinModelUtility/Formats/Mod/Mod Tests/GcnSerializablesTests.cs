using System.Numerics;

using fin.schema.vector;

using mod.schema.mod;
using mod.schema.mod.collision;

using NUnit.Framework;

using schema.binary;

using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;
using Plane = mod.schema.mod.collision.Plane;

namespace mod.schema {
  public class GcnSerializablesTests {
    [Test]
    public void TestBaseCollTriInfo()
      => TestGcnSerializableSimple(new BaseCollTriInfo());

    [Test]
    public void TestBaseRoomInfo()
      => TestGcnSerializableSimple(new BaseRoomInfo());

    [Test]
    public void TestCollGroup() {
      var collGroup = new CollGroup();
      collGroup.unknown1 = [1, 2, 3, 4];

      TestGcnSerializableExisting(collGroup);
    }

    [Test]
    public void TestCollGrid() {
      var collGrid = new CollGrid();

      collGrid.boundsMin = new Vector3(1, 2, 3);
      collGrid.boundsMax = new Vector3(4, 5, 6);
      collGrid.unknown1 = 7;
      collGrid.gridX = 2;
      collGrid.gridY = 3;
      for (var i = 0; i < 2 * 3; ++i) {
        collGrid.unknown2.Add(i);
      }

      var collGroup = new CollGroup();
      collGroup.unknown1 = [1, 2, 3, 4];

      collGrid.groups.Add(collGroup);
      collGrid.groups.Add(collGroup);

      TestGcnSerializableExisting(collGrid);
    }

    /*[Test]
    public void TestEnvelope() => TestGcnSerializableSimple(new Envelope());*/

    [Test]
    public void TestJoint() {
      var joint = new Joint();

      joint.parentIdx = 1;
      joint.flags = 2;
      joint.boundsMin.Set(3.1f, 4.1f, 5.1f);
      joint.boundsMax.Set(6.1f, 7.1f, 8.1f);
      joint.volumeRadius = 9.1f;
      joint.scale.Set(10.1f, 11.1f, 12.1f);
      joint.rotation.Set(13.1f, 14.1f, 15.1f);
      joint.position.Set(16.1f, 17.1f, 18.1f);
      joint.matpolys = [
          new JointMatPoly {
              matIdx = 19,
              meshIdx = 20,
          },
          new JointMatPoly {
              matIdx = 20,
              meshIdx = 21,
          }
      ];

      TestGcnSerializableExisting(joint);
    }

    [Test]
    public void TestJointMatPoly()
      => TestGcnSerializableSimple(new JointMatPoly());

    [Test]
    public void TestMesh() {
      var mesh = new Mesh {
          boneIndex = 1,
          vtxDescriptor = 2
      };

      var meshPacket = new MeshPacket();
      meshPacket.indices = [3, 4];

      var displayList = new DisplayList();
      displayList.flags.intView = 5;
      displayList.cmdCount = 6;
      displayList.dlistData = [7, 8];
      meshPacket.displaylists = [displayList, displayList];

      mesh.packets = [meshPacket, meshPacket];

      TestGcnSerializableExisting(mesh);
    }

    [Test]
    public void TestNbt() => TestGcnSerializableSimple(new Nbt());

    [Test]
    public void TestPlane() => TestGcnSerializableSimple(new Plane());

    [Test]
    public void TestTexture() {
      var texture = new Texture {
          width = 1,
          height = 2,
          format = (Texture.TextureFormat) 3,
      };
      texture.imageData = [5, 6];

      TestGcnSerializableExisting(texture);
    }

    [Test]
    public void TestTextureAttributes() {
      var textureAttributes = new TextureAttributes {
          TextureImageIndex = 1,
          WrapFlags = 2,
          Unk1 = 3,
          WidthPercent = 123f
      };

      TestGcnSerializableExisting(textureAttributes);
    }

    [Test]
    public void TestVector2i() => TestGcnSerializableSimple(new Vector2i());


    [Test]
    public void TestVector3f() => TestGcnSerializableSimple(new Vector3f());

    [Test]
    public void TestVector3i() => TestGcnSerializableSimple(new Vector3i());

    [Test]
    public void TestVtxMatrix() => TestGcnSerializableSimple(new VtxMatrix());

    public static async void TestGcnSerializableSimple(
        IBinaryConvertible gcnSerializable) {
      var dataLen = 100;
      var inData = new byte[dataLen];
      for (var i = 0; i < dataLen; ++i) {
        inData[i] = (byte) i;
      }

      using var reader =
          new SchemaBinaryReader(new MemoryStream(inData),
                                 Endianness.BigEndian);
      gcnSerializable.Read(reader);

      var writer = new SchemaBinaryWriter(Endianness.BigEndian);
      gcnSerializable.Write(writer);

      var outData = new byte[dataLen];
      using var outStream = new MemoryStream(outData);
      await writer.CompleteAndCopyToAsync(outStream);

      Assert.AreEqual(reader.Position,
                      outStream.Position,
                      "Expected reader and writer to move the same amount.");
      for (var i = 0; i < reader.Position; ++i) {
        Assert.AreEqual(inData[i],
                        outData[i],
                        $"Expected data to be equal at index: {i}");
      }
    }

    public static async void TestGcnSerializableExisting(
        IBinaryConvertible gcnSerializable) {
      var dataLen = 300;
      var firstWriter = new SchemaBinaryWriter(Endianness.BigEndian);
      gcnSerializable.Write(firstWriter);

      var firstOutData = new byte[dataLen];
      using var firstOutStream = new MemoryStream(firstOutData);
      await firstWriter.CompleteAndCopyToAsync(firstOutStream);

      using var reader =
          new SchemaBinaryReader(firstOutData, Endianness.BigEndian);
      gcnSerializable.Read(reader);

      var secondWriter =
          new SchemaBinaryWriter(Endianness.BigEndian);
      gcnSerializable.Write(secondWriter);

      var secondOutData = new byte[dataLen];
      using var secondOutStream = new MemoryStream(secondOutData);
      await secondWriter.CompleteAndCopyToAsync(secondOutStream);

      Assert.IsTrue(firstOutStream.Position == reader.Position &&
                    reader.Position == secondOutStream.Position,
                    "Expected all readers & writers to move the same amount.");
      for (var i = 0; i < reader.Position; ++i) {
        Assert.AreEqual(firstOutData[i],
                        secondOutData[i],
                        $"Expected data to be equal at index: {i}");
      }
    }
  }
}