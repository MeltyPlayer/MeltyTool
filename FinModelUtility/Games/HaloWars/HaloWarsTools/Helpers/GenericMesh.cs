using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;

namespace HaloWarsTools {
  public enum MeshNormalExportMode {
    Unchanged,
    CalculateNormalsSmoothShaded,
    CalculateNormalsFlatShaded
  }

  public struct MeshExportOptions(
      Matrix4x4 matrix,
      MeshNormalExportMode normalExportMode = MeshNormalExportMode.Unchanged,
      bool invertNormals = false,
      bool reverseFaceWinding = false) {
    public MeshNormalExportMode NormalExportMode = normalExportMode;
    public bool InvertNormals = invertNormals;
    public bool ReverseFaceWinding = reverseFaceWinding;
    public Matrix4x4 Matrix = matrix;

    public static MeshExportOptions Default =
        new MeshExportOptions(Matrix4x4.Identity, MeshNormalExportMode.Unchanged, false, false);
  }

  public class GenericMesh(MeshExportOptions options) {
    public GenericMesh() : this(MeshExportOptions.Default) { }

    private bool exportOptionsApplied;
    public MeshExportOptions ExportOptions = options;
    public List<Vector3> Vertices = [];
    public List<Vector3> Normals = [];
    public List<Vector3> TexCoords = [];
    public List<GenericFace> Faces = [];

    public void ApplyExportOptions(MeshExportOptions options) {
      if (!this.exportOptionsApplied || !options.Equals(this.ExportOptions)) {
        // If these settings haven't already been applied

        if (options.NormalExportMode == MeshNormalExportMode.CalculateNormalsFlatShaded) {
          this.RecalculateNormalsFlatShaded();
        } else if (options.NormalExportMode == MeshNormalExportMode.CalculateNormalsSmoothShaded) {
          this.RecalculateNormalsSmoothShaded();
        } else {
          // Use already set up normals
        }

        this.Vertices = this.Vertices.Select(vertex => Vector3.Transform(vertex, options.Matrix)).ToList();
        this.Normals = this.Normals
                           .Select(normal => Vector3.Normalize(Vector3.TransformNormal(normal, options.Matrix) *
                                     (options.InvertNormals ? -1.0f : 1.0f)))
                           .ToList();
        if (options.ReverseFaceWinding) {
          this.Faces = this.Faces.Select(face => GenericFace.ReverseWinding(face)).ToList();
        }

        this.exportOptionsApplied = true;
      }
    }

    public void AddMesh(GenericMesh other, Matrix4x4 transform) {
      int offset = this.Vertices.Count;

      var newVerts = other.Vertices.Select(vertex => Vector3.Transform(vertex, transform));
      var newNormals = other.Normals.Select(normal => Vector3.TransformNormal(normal, transform));
      var newFaces = other.Faces.Select(face => GenericFace.OffsetIndices(face, offset));

      this.Vertices.AddRange(newVerts);
      this.Normals.AddRange(newNormals);
      this.TexCoords.AddRange(other.TexCoords);
      this.Faces.AddRange(newFaces);
    }

    public bool Export(string filename, GenericMeshExportFormat format) {
      this.ApplyExportOptions(this.ExportOptions);

      return format switch {
          GenericMeshExportFormat.Obj => this.ExportObj(filename),
          _                           => throw new NotImplementedException()
      };
    }

    public GenericMeshSection[] GetMeshSections() {
      return this.Faces.GroupBy(face => face.Section).Select(group => group.First().Section).ToArray();
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

      foreach (var vertex in this.Vertices) {
        meshWriter.WriteLine($"v {this.GetObjVectorString(vertex)}");
      }

      foreach (var normal in this.Normals) {
        meshWriter.WriteLine($"vn {this.GetObjVectorString(normal)}");
      }

      foreach (var texCoord in this.TexCoords) {
        meshWriter.WriteLine($"vt {this.GetObjVectorString(texCoord)}");
      }

      List<GenericMaterial> materials = [];
      GenericMeshSection[] sections = this.GetMeshSections();

      foreach (var section in sections) {
        meshWriter.WriteLine($"o {section.Name}");
        var faceGroups = this.Faces.Where(face => face.Section == section).GroupBy(face => face.Material);

        foreach (var group in faceGroups) {
          var material = group.First().Material;
          meshWriter.WriteLine($"usemtl {material.Name}");
          if (!materials.Contains(material)) {
            materials.Add(material);
          }

          foreach (var face in group) {
            meshWriter.WriteLine($"f {this.GetObjFaceString(face.A)} {this.GetObjFaceString(face.B)} {this.GetObjFaceString(face.C)}");
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
      var verticesCopy = new List<Vector3>(this.Vertices);
      var uvsCopy = new List<Vector3>(this.TexCoords);
      this.Vertices.Clear();
      this.Normals.Clear();
      this.TexCoords.Clear();

      for (int i = 0; i < this.Faces.Count; i++) {
        int index = this.Vertices.Count;
        this.Vertices.Add(verticesCopy[this.Faces[i].A]);
        this.Vertices.Add(verticesCopy[this.Faces[i].B]);
        this.Vertices.Add(verticesCopy[this.Faces[i].C]);

        this.TexCoords.Add(uvsCopy[this.Faces[i].A]);
        this.TexCoords.Add(uvsCopy[this.Faces[i].B]);
        this.TexCoords.Add(uvsCopy[this.Faces[i].C]);

        var face = new GenericFace(index, index + 1, index + 2, this.Faces[i].Material, this.Faces[i].Section);
        this.Faces[i] = face;

        var normal = face.CalculateNormal(this.Vertices);
        this.Normals.Add(normal);
        this.Normals.Add(normal);
        this.Normals.Add(normal);
      }
    }

    public void RecalculateNormalsSmoothShaded() {
      var vertexMap = this.CalculateVertexIndexToFaceIndexMap();
      this.Normals.Clear();

      for (int vertexIndex = 0; vertexIndex < this.Vertices.Count; vertexIndex++) {
        Vector3 sum = Vector3.Zero;

        if (vertexMap.ContainsKey(vertexIndex)) {
          List<int> faces = vertexMap[vertexIndex];

          foreach (var faceIndex in faces) {
            sum += this.Faces[faceIndex].CalculateNormal(this.Vertices);
          }

          if (faces.Count > 0) {
            sum = Vector3.Normalize(sum / faces.Count);
          }
        }

        this.Normals.Add(sum);
      }
    }

    private string GetObjVectorString(Vector3 vector) {
      return $"{this.GetObjFloatString(vector.X)} {this.GetObjFloatString(vector.Y)} {this.GetObjFloatString(vector.Z)}";
    }

    private string GetObjFloatString(float value) {
      return value.ToString("0.######");
    }

    private string GetObjFaceString(int index) {
      bool hasNormal = index < this.Normals.Count;
      bool hasTexCoord = index < this.TexCoords.Count;
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
      for (int i = 0; i < this.Faces.Count; i++) {
        this.AssociateVertexWithFace(this.Faces[i].A, i, map);
        this.AssociateVertexWithFace(this.Faces[i].B, i, map);
        this.AssociateVertexWithFace(this.Faces[i].C, i, map);
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

  public class GenericMeshSection(string name) {
    public string Name = name;
  }

  public enum GenericMeshExportFormat {
    Obj
  }

  public struct GenericFace(
      int a,
      int b,
      int c,
      GenericMaterial material,
      GenericMeshSection section) {
    public Vector3 CalculateNormal(List<Vector3> vertices) {
      return CalculateNormal(vertices[this.A], vertices[this.B], vertices[this.C]);
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

    public int A = a;
    public int B = b;
    public int C = c;
    public GenericMaterial Material = material;
    public GenericMeshSection Section = section;
  }

  public class GenericMaterial(string name) {
    public string Name = name;
    public GenericMaterialTextureType Type;
    public Dictionary<GenericMaterialTextureType, string> Textures = new();
  }

  public enum GenericMaterialTextureType {
    Albedo,
    Opacity,
    AmbientOcclusion
  }
}