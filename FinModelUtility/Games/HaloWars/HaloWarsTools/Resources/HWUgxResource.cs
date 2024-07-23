#nullable enable


using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Xml;

using Dxt;

using fin.data.lazy;
using fin.io;
using fin.math.rotations;
using fin.math.transform;
using fin.model;
using fin.model.impl;
using fin.model.util;
using fin.scene;
using fin.util.asserts;

using granny3d;

using KSoft.IO;
using KSoft.Phoenix.Xmb;
using KSoft.Shell;

using schema.binary;

namespace HaloWarsTools {
  public class HWUgxResource : HWBinaryResource {
    public ModelImpl Mesh { get; private set; }
    private bool FlipFaces_ { get; set; }

    public static HWUgxResource? FromFile(
        HWContext context,
        string filename,
        bool flipFaces) {
      // Set the extension based on the resource type if the filename doesn't have one
      if (string.IsNullOrEmpty(Path.GetExtension(filename)) &&
          TypeExtensions.TryGetValue(HWResourceType.Ugx,
                                     out string defaultExtension)) {
        filename = Path.ChangeExtension(filename, defaultExtension);
      }

      var resource = (HWUgxResource) CreateResource(context, filename);
      resource.Mesh = new ModelImpl {
          // TODO: Fix this
          FileBundle = null,
          Files = new HashSet<IReadOnlyGenericFile>(),
      };
      resource.FlipFaces_ = flipFaces;
      resource?.Load(File.ReadAllBytes(resource.AbsolutePath));

      return resource;
    }

    protected override void Load(byte[] bytes) {
      base.Load(bytes);

      ImportMesh(bytes, this.Mesh);
    }

    private bool ShouldStopScanning(char value) {
      return value < 46 || value > 122;
    }

    private IList<IMaterial> GetMaterials(IMaterialManager materialManager,
                                          byte[] bytes) {
      var textureFiles = new List<IReadOnlySystemFile>();
      HWBinaryResourceChunk materialChunk =
          GetFirstChunkOfType(HWBinaryResourceChunkType.UGX_MaterialChunk);

      var bdt = new BinaryDataTree();
      bdt.ValidateData = false;

      var chunkStream = new MemoryStream(
          bytes,
          (int) materialChunk.Offset,
          (int) materialChunk.Size,
          false);
      using (chunkStream) {
        var es = new EndianStream(chunkStream, EndianFormat.Little,
                                  permissions: FileAccess.Read);
        es.StreamMode = FileAccess.Read;
        bdt.Serialize(es);
      }

      var lazyTextureDictionary = new LazyCaseInvariantStringDictionary<ITexture>(name
          => LoadTexture(materialManager, name));
      var materials = new List<IMaterial>();

      var xml = bdt.ToXmlDocument();
      foreach (XmlElement ugxMaterials in xml.ChildNodes) {
        foreach (XmlElement ugxMaterial in ugxMaterials) {
          var type = ugxMaterial.GetType();

          var finMaterial = materialManager.AddStandardMaterial();
          finMaterial.Name = $"material_{materials.Count}";

          // TODO: Some textures are still not processed here:
          // - "_op_" seems to be opacity?
          // - "_xf_" is what?

          var maps = ugxMaterial["Maps"];
          if (maps != null) {
            var diffuse = maps["diffuse"];
            var diffuseMap = diffuse?["Map"];
            if (diffuseMap != null) {
              var diffuseMapName = diffuseMap.GetAttribute("Name");
              finMaterial.DiffuseTexture =
                  lazyTextureDictionary[diffuseMapName];
            }

            var normal = maps["normal"];
            var normalMap = normal?["Map"];
            if (normalMap != null) {
              var normalMapName = normalMap.GetAttribute("Name");
              finMaterial.NormalTexture = lazyTextureDictionary[normalMapName];
            }

            var emissive = maps["emissive"];
            var emissiveMap = emissive?["Map"];
            if (emissiveMap != null) {
              var emissiveMapName = emissiveMap.GetAttribute("Name");
              finMaterial.EmissiveTexture =
                  lazyTextureDictionary[emissiveMapName];
            }

            var specular = maps["gloss"];
            var specularMap = specular?["Map"];
            if (specularMap != null) {
              var specularMapName = specularMap.GetAttribute("Name");
              finMaterial.SpecularTexture = lazyTextureDictionary[specularMapName];
            }
          }

          materials.Add(finMaterial);
        }
      }

      /*var skyTextures = new FinDirectory(
                            @"R:\Documents\CSharpWorkspace\Pikmin2Utility\cli\roms\halo_wars\art\environment\sky")
                        .GetExistingFiles()
                        .Where(f => f.Extension == ".ddx");*/
      return materials;
    }

    private ITexture LoadTexture(IMaterialManager materialManager,
                                 string name) {
      var localTexturePath = $"art{name}.ddx";
      var absoluteTexturePath =
          Path.Combine(this.Context.ScratchDirectory, localTexturePath);

      var textureFile = new FinFile(absoluteTexturePath);
      var (textureType, dxt) = DxtDecoder.ReadDds(textureFile);

      var firstMipmap = dxt.First();
      var firstLevel = firstMipmap.First();
      var firstImage = firstLevel.Impl;

      var texture = materialManager.CreateTexture(firstImage);
      texture.Name = name;

      return texture;
    }

    private string GetStringAt(byte[] bytes, int offset) {
      StringBuilder current = new StringBuilder();
      for (int i = offset; i < bytes.Length; i++) {
        char value = (char) bytes[i];
        if (value == 0) {
          break;
        } else {
          current.Append(value);
        }
      }

      return current.ToString();
    }

    private void ImportMesh(byte[] bytes, ModelImpl finModel) {
      var finSkin = finModel.Skin;

      var finMaterials = GetMaterials(finModel.MaterialManager, bytes);
      var nullMaterial = finModel.MaterialManager.AddStandardMaterial();

      int offset = 0;

      offset += 4; // 4 byte magic
      int tableOffset = BinaryUtils.ReadInt32BigEndian(bytes, offset);
      offset += 4;
      offset += 4; // 4 byte reserved
      offset += 4; // file size
      short tableCount = BinaryUtils.ReadInt16BigEndian(bytes, offset);
      offset += 2;
      offset += 2; // 2 byte reserved
      offset += 4; // 4 byte reserved
      offset += 8; // 8 byte reserved

      var boneIds = new byte[4];
      var boneWeights = new byte[4];

      List<MeshTableData> tableData = [];
      offset = tableOffset;
      for (int i = 0; i < tableCount; i++) {
        offset += 4; // 4 byte reserved
        int dataType = BinaryUtils.ReadInt32BigEndian(bytes, offset);
        offset += 4;
        int dataOffset = BinaryUtils.ReadInt32BigEndian(bytes, offset);
        offset += 4;
        int dataLength = BinaryUtils.ReadInt32BigEndian(bytes, offset);
        offset += 4;

        tableData.Add(new MeshTableData(dataType, dataOffset, dataLength));

        offset += 2; // 2 byte reserved
        offset += 2; // 2 byte reserved
        offset += 2; // 2 byte reserved
        offset += 2; // 2 byte reserved
      }

      int vertStart = 0;
      int faceStart = 0;

      Dictionary<int, List<MeshPolygonInfo>> meshArr =
          new Dictionary<int, List<MeshPolygonInfo>>();

      var localFinBones = new List<(IBone, IGrannyBone)>();

      for (int i = 0; i < tableCount; i++) {
        var tableChunk = tableData[i];
        offset = tableChunk.Offset;

        switch (tableChunk.Type) {
          case MeshDataType.GrxChunk: {
            var grxStream =
                new MemoryStream(bytes, tableChunk.Offset, tableChunk.Length);

            using var grxEr =
                new SchemaBinaryReader(grxStream, Endianness.LittleEndian);

            var grannyFileInfo = new GrannyFileInfo();
            grannyFileInfo.Read(grxEr);

            Asserts.Equal(1, grannyFileInfo.SkeletonHeaderList.Count);
            var skeletonHeader = grannyFileInfo.SkeletonHeaderList[0];

            foreach (var grannyBone in skeletonHeader.Bones) {
              var parentIndex = grannyBone.ParentIndex;

              var isRoot = parentIndex == -1;
              var parentFinBone = isRoot
                                      ? finModel.Skeleton.Root
                                      : localFinBones[parentIndex].Item1;

              var position = grannyBone.LocalTransform.Position;
              var rotation =
                  QuaternionUtil.ToEulerRadians(
                      grannyBone.LocalTransform.Orientation);
              var scaleShear = grannyBone.LocalTransform.ScaleShear;

              // Halo Wars coordinates have opposite handedness, so we must flip
              // X depending on how many submodels down we are.
              var xSign = this.FlipFaces_ ? -1 : 1;

              var finBone = 
                  isRoot ?
                      parentFinBone.AddRoot(xSign * position.X, position.Y, position.Z)
                      : parentFinBone.AddChild(xSign * position.X, position.Y, position.Z);

              finBone.LocalTransform.SetRotationRadians(rotation.X,
                    xSign * rotation.Y,
                    xSign * rotation.Z);
              finBone.LocalTransform.SetScale(scaleShear[0].X,
                                              scaleShear[1].Y,
                                              scaleShear[2].Z);

              finBone.Name = grannyBone.Name;

              localFinBones.Add((finBone, grannyBone));
            }


            break;
          }

          case MeshDataType.MeshInfo:
            offset += 2; // 2 byte reserved
            offset += 2; // 2 byte reserved
            offset += 4; // 4 byte reserved
            offset += 48; // 48 byte reserved
            offset += 4; // 4 byte reserved
            offset += 4; // 4 byte reserved

            Dictionary<MeshSubDataType, MeshTableSubData> subDataList =
                new Dictionary<MeshSubDataType, MeshTableSubData>();
            for (int j = 0; j < 6; j++) {
              // Truncating to int because there's no fucking way we need more than 2 billion bytes, none of the files are that big
              int dataCount =
                  (int) BinaryUtils.ReadInt64LittleEndian(bytes, offset);
              offset += 8;
              int dataOffset = tableData[i].Offset +
                               (int) BinaryUtils.ReadInt64LittleEndian(
                                   bytes, offset);
              offset += 8;
              var subData = new MeshTableSubData(dataCount, dataOffset);
              subDataList.Add((MeshSubDataType) (j + 1), subData);
            }

            var boneData = subDataList[MeshSubDataType.BoneData];

            var data = subDataList[MeshSubDataType.MeshData];
            offset = data.Offset;
            for (int j = 0; j < data.Count; j++) {
              var polyInfo = new MeshPolygonInfo();
              polyInfo.MaterialId =
                  BinaryUtils.ReadInt32LittleEndian(bytes, offset);
              offset += 4;
              polyInfo.PolygonId =
                  BinaryUtils.ReadInt32LittleEndian(bytes, offset);
              offset += 4;
              offset += 4; // 4 byte reserved
              polyInfo.BoneId =
                  BinaryUtils.ReadInt32LittleEndian(bytes, offset);
              offset += 4;
              polyInfo.FaceOffset =
                  BinaryUtils.ReadInt32LittleEndian(bytes, offset);
              offset += 4;
              polyInfo.FaceCount =
                  BinaryUtils.ReadInt32LittleEndian(bytes, offset);
              offset += 4;
              polyInfo.VertOffset =
                  BinaryUtils.ReadInt32LittleEndian(bytes, offset);
              offset += 4;
              polyInfo.VertLength =
                  BinaryUtils.ReadInt32LittleEndian(bytes, offset);
              offset += 4;
              polyInfo.VertSize =
                  BinaryUtils.ReadInt32LittleEndian(bytes, offset);
              offset += 4;
              polyInfo.VertCount =
                  BinaryUtils.ReadInt32LittleEndian(bytes, offset);
              offset += 4;

              offset += 16; // 16 byte reserved

              int nameOffset =
                  BinaryUtils.ReadInt32LittleEndian(bytes, offset);
              offset += 4;
              int location =
                  BinaryUtils.ReadInt32LittleEndian(
                      bytes, tableData[i].Offset + nameOffset);
              polyInfo.Name =
                  GetStringAt(bytes, tableData[i].Offset + nameOffset);

              offset += 92; // 92 byte reserved

              if (!meshArr.ContainsKey(polyInfo.PolygonId)) {
                meshArr.Add(polyInfo.PolygonId, []);
              }

              meshArr[polyInfo.PolygonId].Add(polyInfo);
            }

            data = subDataList[MeshSubDataType.BoneData];
            offset = data.Offset;
            for (int j = 0; j < data.Count; j++) {
              int nameOffset =
                  BinaryUtils.ReadInt32LittleEndian(bytes, offset);
              offset += 4;
              string boneName =
                  GetStringAt(bytes, tableData[i].Offset + nameOffset);

              offset += 4; // 4 byte reserved

              offset += 4; // m11
              offset += 4; // m12
              offset += 4; // m13
              offset += 4; // m14

              offset += 4; // m21
              offset += 4; // m22
              offset += 4; // m23
              offset += 4; // m24

              offset += 4; // m31
              offset += 4; // m32
              offset += 4; // m33
              offset += 4; // m34

              offset += 4; // m41
              offset += 4; // m42
              offset += 4; // m43
              offset += 4; // m44

              offset += 4; // parentID
              offset += 4; // 4 byte reserved
            }

            break;
          case MeshDataType.IndexData:
            faceStart = offset;
            break;
          case MeshDataType.VertexData:
            vertStart = offset;
            break;
        }
      }

      foreach (var entry in meshArr) {
        var mesh = finSkin.AddMesh();

        var polygonInfoList = entry.Value;
        for (int i = 0; i < polygonInfoList.Count; i++) {
          var polygonInfo = polygonInfoList[i];
          offset = polygonInfo.VertOffset + vertStart;

          var finVertices = new List<IVertex>();
          for (int j = 0; j < polygonInfo.VertCount; j++) {
            Vector3 position = Vector3.Zero;
            Vector3 normal = Vector3.Zero;
            Vector3 texcoord = Vector3.Zero;

            bool hasBones = false;

            switch (polygonInfo.VertSize) {
              case 0x18:
                ReadPosition(ref position, bytes, ref offset);
                offset += 2; // 2 byte reserved
                ReadNormal(ref normal, bytes, ref offset);
                texcoord.X = BinaryUtils.ReadHalfLittleEndian(bytes, offset);
                offset += 2;
                texcoord.Y = BinaryUtils.ReadHalfLittleEndian(bytes, offset);
                offset += 2;
                break;
              case 0x1c:
                ReadPosition(ref position, bytes, ref offset);
                offset += 2; // 2 byte reserved
                ReadNormal(ref normal, bytes, ref offset);
                texcoord.X = BinaryUtils.ReadHalfLittleEndian(bytes, offset);
                offset += 2;
                texcoord.Y = BinaryUtils.ReadHalfLittleEndian(bytes, offset);
                offset += 2;
                offset += 2; // 2 byte reserved
                offset += 2; // 2 byte reserved
                break;
              case 0x20:
                ReadPosition(ref position, bytes, ref offset);
                offset += 2; // 2 byte reserved
                ReadNormal(ref normal, bytes, ref offset);

                hasBones = true;
                for (var b = 0; b < 4; ++b) {
                  boneIds[b] = BinaryUtils.ReadByteLittleEndian(bytes, offset);
                  offset += 1;
                }
                for (var b = 0; b < 4; ++b) {
                  boneWeights[b] =
                      BinaryUtils.ReadByteLittleEndian(bytes, offset);
                  offset += 1;
                }
                texcoord.X = BinaryUtils.ReadHalfLittleEndian(bytes, offset);
                offset += 2;
                texcoord.Y = BinaryUtils.ReadHalfLittleEndian(bytes, offset);
                offset += 2;
                break;
              case 0x24:
                ReadPosition(ref position, bytes, ref offset);
                offset += 2; // 2 byte reserved
                ReadNormal(ref normal, bytes, ref offset);
                offset += 4; // 4 byte reserved
                offset += 4; // 4 byte reserved
                offset += 4; // 4 byte reserved
                texcoord.X = BinaryUtils.ReadHalfLittleEndian(bytes, offset);
                offset += 2;
                texcoord.Y = BinaryUtils.ReadHalfLittleEndian(bytes, offset);
                offset += 2;
                break;
              case 0x28:
                ReadPosition(ref position, bytes, ref offset);
                offset += 2; // 2 byte reserved
                ReadNormal(ref normal, bytes, ref offset);
                offset += 4; // 4 byte reserved
                offset += 4; // 4 byte reserved
                offset += 4; // 4 byte reserved
                texcoord.X = BinaryUtils.ReadHalfLittleEndian(bytes, offset);
                offset += 2;
                texcoord.Y = BinaryUtils.ReadHalfLittleEndian(bytes, offset);
                offset += 2;
                offset += 4; // 4 byte reserved
                break;
              case 0x2c:
                ReadPosition(ref position, bytes, ref offset);
                offset += 2; // 2 byte reserved
                ReadNormal(ref normal, bytes, ref offset);
                offset += 4; // 4 byte reserved
                offset += 4; // 4 byte reserved
                offset += 4; // 4 byte reserved

                hasBones = true;
                for (var b = 0; b < 4; ++b) {
                  boneIds[b] = BinaryUtils.ReadByteLittleEndian(bytes, offset);
                  offset += 1;
                }
                for (var b = 0; b < 4; ++b) {
                  boneWeights[b] =
                      BinaryUtils.ReadByteLittleEndian(bytes, offset);
                  offset += 1;
                }

                texcoord.X = BinaryUtils.ReadHalfLittleEndian(bytes, offset);
                offset += 2;
                texcoord.Y = BinaryUtils.ReadHalfLittleEndian(bytes, offset);
                offset += 2;
                break;
              case 0x30:
                ReadPosition(ref position, bytes, ref offset);
                offset += 2; // 2 byte reserved
                ReadNormal(ref normal, bytes, ref offset);
                offset += 4; // 4 byte reserved
                offset += 4; // 4 byte reserved
                offset += 4; // 4 byte reserved

                hasBones = true;
                for (var b = 0; b < 4; ++b) {
                  boneIds[b] = BinaryUtils.ReadByteLittleEndian(bytes, offset);
                  offset += 1;
                }
                for (var b = 0; b < 4; ++b) {
                  boneWeights[b] =
                      BinaryUtils.ReadByteLittleEndian(bytes, offset);
                  offset += 1;
                }

                offset += 4; // 4 byte reserved
                texcoord.X = BinaryUtils.ReadHalfLittleEndian(bytes, offset);
                offset += 2;
                texcoord.Y = BinaryUtils.ReadHalfLittleEndian(bytes, offset);
                offset += 2;
                break;
              default:
                continue;
            }

            var finVertex = finSkin.AddVertex(position);
            finVertex.SetLocalNormal(normal);
            finVertex.SetUv(texcoord.X, texcoord.Y);

            if (hasBones) {
              var finBoneWeights =
                  boneWeights
                      .Select((weight, index) => (weight, index))
                      .Where(weightAndIndex => weightAndIndex.weight > 0)
                      .Select(weightAndIndex => {
                        var weight = weightAndIndex.weight / 255f;
                        var index = weightAndIndex.index;

                        var (finBone, grannyBone) =
                            localFinBones[boneIds[index] - 1];
                        return new BoneWeight(finBone, null, weight);
                      })
                      .ToArray();

              finVertex.SetBoneWeights(
                  finSkin.GetOrCreateBoneWeights(
                      VertexSpace.RELATIVE_TO_WORLD, finBoneWeights));
            } else {
              finVertex.SetBoneWeights(
                  finSkin.GetOrCreateBoneWeights(
                      VertexSpace.RELATIVE_TO_WORLD, localFinBones[0].Item1));
            }

            finVertices.Add(finVertex);
          }

          var triangles =
              new (IReadOnlyVertex, IReadOnlyVertex, IReadOnlyVertex)[polygonInfo.FaceCount];

          offset = ((polygonInfo.FaceOffset * 2) + faceStart);
          for (var j = 0; j < polygonInfo.FaceCount; j++) {
            var fa = BinaryUtils.ReadUInt16LittleEndian(bytes, offset);
            offset += 2;
            var fb = BinaryUtils.ReadUInt16LittleEndian(bytes, offset);
            offset += 2;
            var fc = BinaryUtils.ReadUInt16LittleEndian(bytes, offset);
            offset += 2;

            // Halo Wars coordinates have opposite handedness, so we must flip
            // the faces depending on how many submodels down we are.
            if (FlipFaces_) {
              triangles[j] = (finVertices[fa],
                              finVertices[fb],
                              finVertices[fc]);
            } else {
              triangles[j] = (finVertices[fa],
                              finVertices[fc],
                              finVertices[fb]);
            }

            /*if (!materials.ContainsKey(polygonInfo.MaterialId)) {
              materials.Add(polygonInfo.MaterialId,
                            new GenericMaterial(
                                "material_" + (polygonInfo.MaterialId + 1)));
            }

            if (!sections.ContainsKey(polygonInfo.PolygonId)) {
              sections.Add(polygonInfo.PolygonId,
                           new GenericMeshSection(
                               "object_" + (polygonInfo.PolygonId + 1)));
            }*/
          }

          var finPrimitive = mesh.AddTriangles(triangles);

          var materialId = polygonInfo.MaterialId;
          if (materialId < finMaterials.Count) {
            finPrimitive.SetMaterial(finMaterials[materialId]);
          } else {
            finPrimitive.SetMaterial(nullMaterial);
          }
        }
      }
    }

    private void ReadPosition(ref Vector3 position,
                              byte[] bytes,
                              ref int offset) {
      // Halo Wars coordinates have opposite handedness, so we must flip X
      // depending on how many submodels down we are.
      var xSign = this.FlipFaces_ ? -1 : 1;
      position.X = xSign * BinaryUtils.ReadHalfLittleEndian(bytes, offset);
      offset += 2;
      position.Y = BinaryUtils.ReadHalfLittleEndian(bytes, offset);
      offset += 2;
      position.Z = BinaryUtils.ReadHalfLittleEndian(bytes, offset);
      offset += 2;
    }

    private void ReadNormal(ref Vector3 normal,
                            byte[] bytes,
                            ref int offset) {
      // Halo Wars coordinates have opposite handedness, so we must flip X
      // depending on how many submodels down we are.
      var xSign = this.FlipFaces_ ? -1 : 1;
      normal.X = xSign * BinaryUtils.ReadFloatLittleEndian(bytes, offset);
      offset += 4;
      normal.Y = BinaryUtils.ReadFloatLittleEndian(bytes, offset);
      offset += 4;
      normal.Z = BinaryUtils.ReadFloatLittleEndian(bytes, offset);
      offset += 4;
    }

    public struct MeshPolygonInfo {
      public int MaterialId;
      public int PolygonId;
      public int BoneId;
      public int FaceOffset;
      public int FaceCount;
      public int VertOffset;
      public int VertLength;
      public int VertSize;
      public int VertCount;
      public string Name;
    }

    public struct MeshTableData(int dataType, int dataOffset, int dataLength) {
      public MeshDataType Type = (MeshDataType) dataType;
      public int Offset = dataOffset;
      public int Length = dataLength;
    }

    public struct MeshTableSubData(int dataCount, int dataOffset) {
      public int Offset = dataOffset;
      public int Count = dataCount;
    }

    public enum MeshDataType {
      MeshInfo = 0x700,
      IndexData = 0x701,
      VertexData = 0x702,
      GrxChunk = 0x703,
    }

    public enum MeshSubDataType {
      MeshData = 1,
      BoneData = 2,
      LinkData = 3,
      MeshId = 4,
      MinBound = 5,
      MaxBound = 6
    }
  }
}