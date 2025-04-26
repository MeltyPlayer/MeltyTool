using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Numerics;

using fin.color;
using fin.image;
using fin.io;
using fin.util.hash;
using fin.image.util;

namespace fin.model.impl;

public partial class ModelImpl<TVertex> {
  private partial class MaterialManagerImpl {
    public IReadOnlyList<ITexture> Textures { get; }
  }

  private abstract class BTextureImpl(int index, IReadOnlyImage[] mipmapImages)
      : ITexture {
    private TransparencyType? transparencyType_;
    private Bitmap? imageData_;

    public string Name { get; set; }
    public override string ToString() => this.Name;

    public int Index => index;


    public LocalImageFormat BestImageFormat => LocalImageFormat.PNG;

    public string ValidFileName
      => this.Name.ReplaceInvalidFilenameCharacters() +
         this.BestImageFormat.GetExtension();

    public int UvIndex { get; set; }
    public UvType UvType { get; set; }

    public ColorType ColorType { get; set; }

    public IReadOnlyImage[] MipmapImages => mipmapImages;
    public IReadOnlyImage Image => this.MipmapImages[0];

    public Bitmap ImageData => this.imageData_ ??= this.Image.AsBitmap();

    public void WriteToStream(Stream stream)
      => this.Image.ExportToStream(stream, this.BestImageFormat);

    public void SaveInDirectory(ISystemDirectory directory) {
      var name = this.Name.ReplaceInvalidFilenameCharacters();
      var extension = this.BestImageFormat.GetExtension();

      if (this.MipmapImages.Length == 1) {
        ISystemFile outFile =
            new FinFile(Path.Combine(directory.FullPath, name + extension));
        using var writer = outFile.OpenWrite();
        this.WriteToStream(writer);
      } else {
        for (var i = 0; i < this.MipmapImages.Length; ++i) {
          var mipmapImage = this.MipmapImages[i];
          ISystemFile outFile =
              new FinFile(Path.Combine(directory.FullPath,
                                       $"{name}_level{i}{extension}"));
          using var writer = outFile.OpenWrite();
          mipmapImage.ExportToStream(writer, this.BestImageFormat);
        }
      }
    }

    public TransparencyType TransparencyType
      => this.transparencyType_
          ??= TransparencyTypeUtil.GetTransparencyType(this.Image);

    public WrapMode WrapModeU { get; set; }
    public WrapMode WrapModeV { get; set; }

    public IColor? BorderColor { get; set; }

    public TextureMagFilter MagFilter { get; set; } = TextureMagFilter.LINEAR;

    public TextureMinFilter MinFilter { get; set; } =
      TextureMinFilter.LINEAR_MIPMAP_LINEAR;

    public float MinLod { get; set; } = -1000;
    public float MaxLod { get; set; } = 1000;
    public float LodBias { get; set; } = 0;

    public Vector2? ClampS { get; set; }
    public Vector2? ClampT { get; set; }

    public ITextureTransform TextureTransform { get; }
      = new TextureTransformImpl();

    public override int GetHashCode()
      => new FluentHash()
         .With(this.Name)
         .With(this.Image)
         .With(this.WrapModeU)
         .With(this.WrapModeV)
         .With(this.UvType)
         .With(this.UvIndex)
         .With(this.TextureTransform);

    public override bool Equals(object? other) {
      if (ReferenceEquals(null, other)) {
        return false;
      }

      if (ReferenceEquals(this, other)) {
        return true;
      }

      if (other is ITexture otherTexture) {
        return this.Name == otherTexture.Name &&
               this.Image == otherTexture.Image &&
               this.WrapModeU == otherTexture.WrapModeU &&
               this.WrapModeV == otherTexture.WrapModeV &&
               this.UvType == otherTexture.UvType &&
               this.UvIndex == otherTexture.UvIndex &&
               this.TextureTransform == otherTexture.TextureTransform;
      }

      return false;
    }
  }
}