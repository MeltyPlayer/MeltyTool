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
    ISegmentChunk SegmentChunk { get; }
  }


  public abstract class BZFile(ISegmentChunk segmentChunk) : IZFile {
    public abstract ZFileType Type { get; }
    public string FileName { get; set; }
    public ISegmentChunk SegmentChunk { get; } = segmentChunk;
    public override string ToString() => this.FileName;
  }


  public sealed class ZObject(ISegmentChunk segmentChunk) : BZFile(segmentChunk) {
    public override ZFileType Type => ZFileType.OBJECT;
  }


  public sealed class ZCodeFiles(ISegmentChunk segmentChunk) : BZFile(segmentChunk) {
    public override ZFileType Type => ZFileType.CODE;
  }


  public sealed class ZScene(ISegmentChunk segmentChunk) : BZFile(segmentChunk) {
    public override ZFileType Type => ZFileType.SCENE;

    // TODO: Make nonnull via init, C#9.
    public ZMap[]? Maps;
  }

  public sealed class ZMap(ISegmentChunk segmentChunk) : BZFile(segmentChunk) {
    public override ZFileType Type => ZFileType.MAP;

    // TODO: Make nonnull via init, C#9.
    public ZScene? Scene { get; set; }
  }

  public sealed class ZObjectSet(ISegmentChunk segmentChunk) : BZFile(segmentChunk) {
    public override ZFileType Type => ZFileType.OBJECT_SET;
  }

  public sealed class ZOtherData(ISegmentChunk segmentChunk) : BZFile(segmentChunk) {
    public override ZFileType Type => ZFileType.OTHER;
  }
}