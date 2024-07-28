using System.Numerics;

using fin.math;

using OpenTK.Graphics.OpenGL;

namespace sm64.Scripts {
  public class CollisionTriangleList {
    public int id = 0;
    public List<uint> indicesList;
    public uint[] indices;
    public int ibo { get; set; }

    public CollisionTriangleList(int ID) {
      this.id = ID;
      this.indicesList = [];
    }

    public void AddTriangle(uint a, uint b, uint c) {
      this.indicesList.Add(a);
      this.indicesList.Add(b);
      this.indicesList.Add(c);
    }

    public uint getTriangleCount() {
      return (uint) this.indices.Length / 3;
    }

    public void buildList() {
      this.indices = this.indicesList.ToArray();
    }
  }

  public class CollisionMap {
    private int vbo;

    private List<Vector3> vertices = [];

    //private Vector3[] vertices = null;
    public List<CollisionTriangleList> triangles =
        [];

    public Vector3[] verts;

    public void AddVertex(Vector3 newVert) {
      this.vertices.Add(newVert);
    }

    public void AddTriangle(uint a, uint b, uint c) {
      if (this.triangles.Count > 0)
        this.triangles[this.triangles.Count - 1].AddTriangle(a, b, c);
    }

    public void NewTriangleList(int ID) {
      this.triangles.Add(new CollisionTriangleList(ID));
    }

    public uint getTriangleCount() {
      uint tri_count = 0;
      foreach (CollisionTriangleList tri in this.triangles)
        tri_count += tri.getTriangleCount();
      return tri_count;
    }


    public static float barryCentric(Vector3 p1,
                                     Vector3 p2,
                                     Vector3 p3,
                                     Vector3 pos) {
      float det = (p2.Z - p3.Z) * (p1.X - p3.X) + (p3.X - p2.X) * (p1.Z - p3.Z);
      float l1 =
          ((p2.Z - p3.Z) * (pos.X - p3.X) + (p3.X - p2.X) * (pos.Z - p3.Z)) /
          det;
      float l2 =
          ((p3.Z - p1.Z) * (pos.X - p3.X) + (p1.X - p3.X) * (pos.Z - p3.Z)) /
          det;
      float l3 = 1.0f - l1 - l2;
      return l1 * p1.Y + l2 * p2.Y + l3 * p3.Y;
    }

    public static bool PointInTriangle(Vector2 p,
                                       Vector2 p0,
                                       Vector2 p1,
                                       Vector2 p2) {
      var s = p0.Y * p2.X - p0.X * p2.Y + (p2.Y - p0.Y) * p.X +
              (p0.X - p2.X) * p.Y;
      var t = p0.X * p1.Y - p0.Y * p1.X + (p0.Y - p1.Y) * p.X +
              (p1.X - p0.X) * p.Y;

      if ((s < 0) != (t < 0))
        return false;

      var A = -p1.Y * p2.X + p0.Y * (p2.X - p1.X) + p0.X * (p1.Y - p2.Y) +
              p1.X * p2.Y;
      if (A < 0.0) {
        s = -s;
        t = -t;
        A = -A;
      }

      return s > 0 && t > 0 && (s + t) <= A;
    }

    private struct tempTriangle {
      public Vector3 a, b, c;
    }

    public short dropToGround(Vector3 pos) {
      List<float> found = [];
      for (int i = 0; i < this.triangles.Count; i++) {
        CollisionTriangleList list = this.triangles[i];
        for (int j = 0; j < list.indices.Length; j += 3) {
          tempTriangle temp;
          int index1 = (int) list.indices[j + 0];
          int index2 = (int) list.indices[j + 1];
          int index3 = (int) list.indices[j + 2];
          int numVertices = this.vertices.Count;
          if (index1 >= numVertices || index2 >= numVertices ||
              index3 >= numVertices)
            continue;
          temp.a = this.vertices[index1];
          temp.b = this.vertices[index2];
          temp.c = this.vertices[index3];
          if (PointInTriangle(pos.Xz(),
                              temp.a.Xz(),
                              temp.b.Xz(),
                              temp.c.Xz())) {
            found.Add(barryCentric(temp.a, temp.b, temp.c, pos));
          }
        }
      }

      if (found.Count == 0)
        return (short) pos.Y;

      int closest_index = 0;
      float closest_abs = 9999999.0f;
      // Console.WriteLine("Found " + found.Count + " triangles under position");
      for (int i = 0; i < found.Count; i++) {
        float abs = Math.Abs(pos.Y - found[i]);
        if (abs < closest_abs) {
          closest_abs = abs;
          closest_index = i;
        }
      }

      return (short) found[closest_index];
    }

    public void buildCollisionMap() {
      this.verts = this.vertices.ToArray();

      this.vbo = GL.GenBuffer();
      GL.BindBuffer(BufferTarget.ArrayBuffer, this.vbo);
      GL.BufferData(
          BufferTarget.ArrayBuffer,
          (IntPtr) (3 * sizeof(float) * this.verts.Length),
          this.verts,
          BufferUsageHint.StaticDraw
      );

      for (int i = 0; i < this.triangles.Count; i++) {
        this.triangles[i].buildList();
        this.triangles[i].ibo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, this.triangles[i].ibo);
        GL.BufferData(
            BufferTarget.ElementArrayBuffer,
            (IntPtr) (sizeof(uint) * this.triangles[i].indices.Length),
            this.triangles[i].indices,
            BufferUsageHint.StaticDraw
        );
      }
    }
  }
}