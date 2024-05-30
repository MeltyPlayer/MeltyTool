using schema.binary;

namespace ttyd.schema.tpl {
  public class Tpl : IBinaryDeserializable {
    public IReadOnlyList<TplTexture> Textures { get; private set; }

    public void Read(IBinaryReader br) {
      var header = br.ReadNew<TplHeader>();

      br.Position = header.HeaderSize;
      var textureOffsets = br.ReadNews<TplTextureOffsets>(header.NumTextures);

      var textures = new List<TplTexture>();
      foreach (var textureOffset in textureOffsets) {
        br.Position = textureOffset.HeaderOffset;
        var textureHeader = br.ReadNew<TplTextureHeader>();

        br.Position = textureHeader.DataOffset;
        var image = new TplImageReader(
                textureHeader.Width,
                textureHeader.Height,
                textureHeader.Format)
            .ReadImage(br);

        textures.Add(new TplTexture {
            Header = textureHeader,
            Palette = null,
            Image = image,
        });
      }

      this.Textures = textures;
    }
  }
}