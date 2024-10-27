using System;

using NUnit.Framework;

using SharpGLTF.Geometry.VertexTypes;

namespace fin.model.io.exporters.gltf;

public class GltfBuilderUtilTests {
  [Test]
  [TestCase(false, false, ExpectedResult = typeof(VertexPosition))]
  [TestCase(true, false, ExpectedResult = typeof(VertexPositionNormal))]
  [TestCase(true, true, ExpectedResult = typeof(VertexPositionNormalTangent))]
  public Type TestGetGeometryType(bool hasNormals, bool hasTangents) 
    => GltfBuilderUtil.GetGeometryType(hasNormals, hasTangents);

  [Test]
  [TestCase(0, 0, ExpectedResult = typeof(VertexEmpty))]
  [TestCase(0, 1, ExpectedResult = typeof(VertexTexture1))]
  [TestCase(0, 2, ExpectedResult = typeof(VertexTexture2))]
  [TestCase(0, 3, ExpectedResult = typeof(VertexTexture3))]
  [TestCase(0, 4, ExpectedResult = typeof(VertexTexture4))]
  [TestCase(0, 5, ExpectedResult = typeof(VertexTexture4))]
  [TestCase(1, 0, ExpectedResult = typeof(VertexColor1))]
  [TestCase(1, 1, ExpectedResult = typeof(VertexColor1Texture1))]
  [TestCase(1, 2, ExpectedResult = typeof(VertexColor1Texture2))]
  [TestCase(1, 3, ExpectedResult = typeof(VertexColor1Texture3))]
  [TestCase(1, 4, ExpectedResult = typeof(VertexColor1Texture4))]
  [TestCase(1, 5, ExpectedResult = typeof(VertexColor1Texture4))]
  [TestCase(2, 0, ExpectedResult = typeof(VertexColor2))]
  [TestCase(2, 1, ExpectedResult = typeof(VertexColor2Texture1))]
  [TestCase(2, 2, ExpectedResult = typeof(VertexColor2Texture2))]
  [TestCase(2, 3, ExpectedResult = typeof(VertexColor2Texture3))]
  [TestCase(2, 4, ExpectedResult = typeof(VertexColor2Texture4))]
  [TestCase(2, 5, ExpectedResult = typeof(VertexColor2Texture4))]
  [TestCase(3, 0, ExpectedResult = typeof(VertexColor2))]
  public Type TestGetMaterialType(int colorCount, int uvCount)
    => GltfBuilderUtil.GetMaterialType(colorCount, uvCount);

  [Test]
  [TestCase(0, ExpectedResult = typeof(VertexEmpty))]
  [TestCase(1, ExpectedResult = typeof(VertexJoints4))]
  [TestCase(4, ExpectedResult = typeof(VertexJoints4))]
  [TestCase(5, ExpectedResult = typeof(VertexJoints8))]
  [TestCase(8, ExpectedResult = typeof(VertexJoints8))]
  public Type TestGetSkinningType(int weightCount)
    => GltfBuilderUtil.GetSkinningType(weightCount);
}