using Dxt;

using fin.image;

namespace HaloWarsTools {
  public class HWXttResource : HWBinaryResource {
    public IImage AlbedoTexture { get; private set; }

    public static new HWXttResource
        FromFile(HWContext context, string filename)
      => GetOrCreateFromFile(context, filename, HWResourceType.Xtt) as
             HWXttResource;

    protected override void Load(byte[] bytes) {
      base.Load(bytes);

      this.AlbedoTexture = ExtractEmbeddedDXT1(
          bytes,
          GetFirstChunkOfType(
              HWBinaryResourceChunkType
                  .XTT_AtlasChunkAlbedo));
    }

    private IImage ExtractEmbeddedDXT1(byte[] bytes,
                                       HWBinaryResourceChunk chunk) {
      // Decompress DXT1 texture and turn it into a Bitmap
      var width =
          BinaryUtils.ReadInt32BigEndian(bytes, (int) chunk.Offset + 4);
      var height =
          BinaryUtils.ReadInt32BigEndian(bytes, (int) chunk.Offset + 8);
      return DxtDecoder.DecompressDXT1(bytes,
                                       (int) chunk.Offset + 16,
                                       width,
                                       height);
    }
  }
}