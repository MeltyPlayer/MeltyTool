using f3dzex2.io;

namespace UoT.memory {
  public enum ZFileType {
    OBJECT,
    CODE,
    SCENE,
    MAP,

    /// <summary>
    ///   A set of objects in a given map. These seem to be used to switch
    ///   between different versions of rooms.
    /// </summary>
    OBJECT_SET,
    OTHER,
  }

  public interface IZFile {
    ZFileType Type { get; }

    string FileName { get; }
    Segment Segment { get; }
  }


  public abstract class BZFile(Segment segment) : IZFile {
    public abstract ZFileType Type { get; }
    public string FileName { get; set; }
    public Segment Segment { get; } = segment;
    public override string ToString() => this.FileName;
  }


  public class ZObject(Segment segment) : BZFile(segment) {
    public override ZFileType Type => ZFileType.OBJECT;
  }


  public class ZCodeFiles(Segment segment) : BZFile(segment) {
    public override ZFileType Type => ZFileType.CODE;
  }


  public class ZScene(Segment segment) : BZFile(segment) {
    public override ZFileType Type => ZFileType.SCENE;

    // TODO: Make nonnull via init, C#9.
    public ZMap[]? Maps;
  }

  public class ZMap(Segment segment) : BZFile(segment) {
    public override ZFileType Type => ZFileType.MAP;

    // TODO: Make nonnull via init, C#9.
    public ZScene? Scene { get; set; }
  }

  public class ZObjectSet(Segment segment) : BZFile(segment) {
    public override ZFileType Type => ZFileType.OBJECT_SET;
  }

  public class ZOtherData(Segment segment) : BZFile(segment) {
    public override ZFileType Type => ZFileType.OTHER;
  }
}