using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;

namespace HaloWarsTools;

public enum MeshNormalExportMode {
  Unchanged,
  CalculateNormalsSmoothShaded,
  CalculateNormalsFlatShaded
}

public struct MeshExportOptions {
  public MeshNormalExportMode NormalExportMode;
  public bool InvertNormals;
  public bool ReverseFaceWinding;
  public Matrix4x4 Matrix;

  public MeshExportOptions(Matrix4x4 matrix,
                           MeshNormalExportMode normalExportMode = MeshNormalExportMode.Unchanged,
                           bool invertNormals = false,
                           bool reverseFaceWinding = false) {
      Matrix = matrix;
      NormalExportMode = normalExportMode;
      InvertNormals = invertNormals;
      ReverseFaceWinding = reverseFaceWinding;
    }

  public static MeshExportOptions Default =
      new MeshExportOptions(Matrix4x4.Identity, MeshNormalExportMode.Unchanged, false, false);
}

public class GenericMesh {
  public GenericMesh() : this(MeshExportOptions.Default) { }

  public GenericMesh(MeshExportOptions options) {
      ExportOptions = options;
      Vertices = [];
      Normals = [];
      TexCoords = [];
      Faces = [];
    }

  private bool exportOptionsApplied;
  public MeshExportOptions ExportOptions;
  public List<Vector3> Vertices;
  public List<Vector3> Normals;
  public List<Vector3> TexCoords;
  public List<GenericFace> Faces;

  public void ApplyExportOptions(MeshExportOptions options) {
      if (!exportOptionsApplied || !options.Equals(ExportOptions)) {
        // If these settings haven't already been applied

        if (options.NormalExportMode == MeshNormalExportMode.CalculateNormalsFlatShaded) {
          RecalculateNormalsFlatShaded();
        } else if (options.NormalExportMode == MeshNormalExportMode.CalculateNormalsSmoothShaded) {
          RecalculateNormalsSmoothShaded();
        } else {
          // Use already set up normals
        }

        Vertices = Vertices.Select(vertex => Vector3.Transform(vertex, options.Matrix)).ToList();
        Normals = Normals
                  .Select(normal => Vector3.Normalize(Vector3.TransformNormal(normal, options.Matrix) *
                                                      (options.InvertNormals ? -1.0f : 1.0f)))
                  .ToList();
        if (options.ReverseFaceWinding) {
          Faces = Faces.Select(face => GenericFace.ReverseWinding(face)).ToList();
        }

        exportOptionsApplied = true;
      }
    }

  public void AddMesh(GenericMesh other, Matrix4x4 transform) {
      int offset = Vertices.Count;

      var newVerts = other.Vertices.Select(vertex => Vector3.Transform(vertex, transform));
      var newNormals = other.Normals.Select(normal => Vector3.TransformNormal(normal, transform));
      var newFaces = other.Faces.Select(face => GenericFace.OffsetIndices(face, offset));

      Vertices.AddRange(newVerts);
      Normals.AddRange(newNormals);
      TexCoords.AddRange(other.TexCoords);
      Faces.AddRange(newFaces);
    }

  public bool Export(string filename, GenericMeshExportFormat format) {
      ApplyExportOptions(ExportOptions);

      return format switch {
          GenericMeshExportFormat.Obj => ExportObj(filename),
          _                           => throw new NotImplementedException()
      };
    }

  public GenericMeshSection[] GetMeshSections() {
      return Faces.GroupBy(face => face.Section).Select(group => group.First().Section).ToArray();
    }

  private bool ExportObj(string filename) {
      int bufferSize = 1024 * 1024; // 1 mb
      Directory.CreateDirectory(Path.GetDirectoryName(filename));
      using StreamWriter meshWriter =
          new StreamWriter(File.Open(Path.ChangeExtension(filename, ".obj"), FileMode.Create), Encoding.ASCII,
                           bufferSize);
      using StreamWriter materialWriter =
          new StreamWriter(File.Open(Path.ChangeExtension(filename, ".mtl"), FileMode.Create), Encoding.ASCII,
                           bufferSize);

      meshWriter.WriteLine($"mtllib {Path.GetFileName(Path.ChangeExtension(filename, ".mtl"))}");

      foreach (var vertex in Vertices) {
        meshWriter.WriteLine($"v {GetObjVectorString(vertex)}");
      }

      foreach (var normal in Normals) {
        meshWriter.WriteLine($"vn {GetObjVectorString(normal)}");
      }

      foreach (var texCoord in TexCoords) {
        meshWriter.WriteLine($"vt {GetObjVectorString(texCoord)}");
      }

      List<GenericMaterial> materials = [];
      GenericMeshSection[] sections = GetMeshSections();

      foreach (var section in sections) {
        meshWriter.WriteLine($"o {section.Name}");
        var faceGroups = Faces.Where(face => face.Section == section).GroupBy(face => face.Material);

        foreach (var group in faceGroups) {
          var material = group.First().Material;
          meshWriter.WriteLine($"usemtl {material.Name}");
          if (!materials.Contains(material)) {
            materials.Add(material);
          }

          foreach (var face in group) {
            meshWriter.WriteLine($"f {GetObjFaceString(face.A)} {GetObjFaceString(face.B)} {GetObjFaceString(face.C)}");
          }
        }
      }

      foreach (var material in materials) {
        // Define the material
        materialWriter.WriteLine($"newmtl {material.Name}");

        // Some reasonable defaults from Blender
        materialWriter.WriteLine($"Ns 225.000000");
        materialWriter.WriteLine($"Ka 1.000000 1.000000 1.000000");
        materialWriter.WriteLine($"Kd 1.000000 1.000000 1.000000");
        materialWriter.WriteLine($"Ks 0.500000 0.500000 0.500000");
        materialWriter.WriteLine($"Ke 0.000000 0.000000 0.000000");
        materialWriter.WriteLine($"Ni 1.450000");
        materialWriter.WriteLine($"d 1.000000");
        materialWriter.WriteLine($"illum 2");

        string textureFilename;
        if (material.Textures.TryGetValue(GenericMaterialTextureType.Albedo, out textureFilename)) {
          materialWriter.WriteLine($"map_Kd {textureFilename}");
        }
      }

      return true;
    }

  public void RecalculateNormalsFlatShaded() {
      var verticesCopy = new List<Vector3>(Vertices);
      var uvsCopy = new List<Vector3>(TexCoords);
      Vertices.Clear();
      Normals.Clear();
      TexCoords.Clear();

      for (int i = 0; i < Faces.Count; i++) {
        int index = Vertices.Count;
        Vertices.Add(verticesCopy[Faces[i].A]);
        Vertices.Add(verticesCopy[Faces[i].B]);
        Vertices.Add(verticesCopy[Faces[i].C]);

        TexCoords.Add(uvsCopy[Faces[i].A]);
        TexCoords.Add(uvsCopy[Faces[i].B]);
        TexCoords.Add(uvsCopy[Faces[i].C]);

        var face = new GenericFace(index, index + 1, index + 2, Faces[i].Material, Faces[i].Section);
        Faces[i] = face;

        var normal = face.CalculateNormal(Vertices);
        Normals.Add(normal);
        Normals.Add(normal);
        Normals.Add(normal);
      }
    }

  public void RecalculateNormalsSmoothShaded() {
      var vertexMap = CalculateVertexIndexToFaceIndexMap();
      Normals.Clear();

      for (int vertexIndex = 0; vertexIndex < Vertices.Count; vertexIndex++) {
        Vector3 sum = Vector3.Zero;

        if (vertexMap.ContainsKey(vertexIndex)) {
          List<int> faces = vertexMap[vertexIndex];

          foreach (var faceIndex in faces) {
            sum += Faces[faceIndex].CalculateNormal(Vertices);
          }

          if (faces.Count > 0) {
            sum = Vector3.Normalize(sum / faces.Count);
          }
        }

        Normals.Add(sum);
      }
    }

  private string GetObjVectorString(Vector3 vector) {
      return $"{GetObjFloatString(vector.X)} {GetObjFloatString(vector.Y)} {GetObjFloatString(vector.Z)}";
    }

  private string GetObjFloatString(float value) {
      return value.ToString("0.######");
    }

  private string GetObjFaceString(int index) {
      bool hasNormal = index < Normals.Count;
      bool hasTexCoord = index < TexCoords.Count;
      string indexStr = (index + 1).ToString();

      if (hasTexCoord && !hasNormal) {
        return $"{indexStr}/{indexStr}";
      } else if (!hasTexCoord && hasNormal) {
        return $"{indexStr}//{indexStr}";
      } else if (hasTexCoord && hasNormal) {
        return $"{indexStr}/{indexStr}/{indexStr}";
      }

      return indexStr;
    }

  public Dictionary<int, List<int>> CalculateVertexIndexToFaceIndexMap() {
      var map = new Dictionary<int, List<int>>();
      for (int i = 0; i < Faces.Count; i++) {
        AssociateVertexWithFace(Faces[i].A, i, map);
        AssociateVertexWithFace(Faces[i].B, i, map);
        AssociateVertexWithFace(Faces[i].C, i, map);
      }

      return map;
    }

  private void AssociateVertexWithFace(int vertexIndex, int faceIndex, Dictionary<int, List<int>> map) {
      if (!map.ContainsKey(vertexIndex)) {
        map.Add(vertexIndex, []);
      }

      if (!map[vertexIndex].Contains(faceIndex)) {
        map[vertexIndex].Add(faceIndex);
      }
    }
}

public class GenericMeshSection {
  public GenericMeshSection(string name) {
      Name = name;
    }

  public string Name;
}

public enum GenericMeshExportFormat {
  Obj
}

public struct GenericFace {
  public GenericFace(int a, int b, int c, GenericMaterial material, GenericMeshSection section) {
      A = a;
      B = b;
      C = c;
      Material = material;
      Section = section;
    }

  public Vector3 CalculateNormal(List<Vector3> vertices) {
      return CalculateNormal(vertices[A], vertices[B], vertices[C]);
    }

  public static Vector3 CalculateNormal(Vector3 a, Vector3 b, Vector3 c) {
      var u = b - a;
      var v = c - a;
      return Vector3.Normalize(Vector3.Cross(u, v));
    }

  public static GenericFace OffsetIndices(GenericFace initialValue, int offset) {
      return new GenericFace(initialValue.A + offset, initialValue.B + offset, initialValue.C + offset,
                             initialValue.Material, initialValue.Section);
    }

  public static GenericFace ReverseWinding(GenericFace initialValue) {
      return new GenericFace(initialValue.A, initialValue.C, initialValue.B, initialValue.Material,
                             initialValue.Section);
    }

  public int A;
  public int B;
  public int C;
  public GenericMaterial Material;
  public GenericMeshSection Section;
}

public class GenericMaterial {
  public GenericMaterial(string name) {
      Name = name;
      Textures = new Dictionary<GenericMaterialTextureType, string>();
    }

  public string Name;
  public GenericMaterialTextureType Type;
  public Dictionary<GenericMaterialTextureType, string> Textures;
}

public enum GenericMaterialTextureType {
  Albedo,
  Opacity,
  AmbientOcclusion
}