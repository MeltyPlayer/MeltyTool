using fin.data;
using fin.data.dictionaries;

using modl.schema.modl.common;

namespace modl.schema.modl;

public interface IModl {
  List<IBwNode> Nodes { get; }
  ListDictionary<ushort, ushort> CnctParentToChildren { get; }
}

public interface IBwNode {
  string GetIdentifier();
  bool IsHidden { get; }

  BwTransform Transform { get; }
  float Scale { get; set; }
  List<IBwMaterial> Materials { get; }
  List<BwMesh> Meshes { get; }

  VertexUv[][] UvMaps { get; }
  List<VertexPosition> Positions { get; }
  List<VertexNormal> Normals { get; }
}

public interface IBwMaterial {
  string Texture1 { get; }
  string Texture2 { get; }
}