using System.Collections.Generic;
using System.Drawing;
using System.IO;

using fin.color;
using fin.image;
using fin.io;
using fin.math.xyz;
using fin.schema.vector;
using fin.util.hash;
using fin.util.image;

namespace fin.model.impl {
  public partial class ModelImpl<TVertex> {
    private partial class MaterialManagerImpl {
      public IReadOnlyList<ITexture> Textures { get; }

      public ITexture CreateTexture(IReadOnlyImage imageData) {
        var texture = new TextureImpl(imageData);
        this.textures_.Add(texture);
        return texture;
      }
    }

    private class TextureImpl : ITexture {
      private ImageTransparencyType? transparencyType_;
      private Bitmap? imageData_;

      public TextureImpl(IReadOnlyImage image) {
        this.Image = image;
      }

      public string Name { get; set; }


      public LocalImageFormat BestImageFormat => LocalImageFormat.PNG;

      public string ValidFileName
        => this.Name.ReplaceInvalidFilenameCharacters() +
           BestImageFormat.GetExtension();

      public int UvIndex { get; set; }
      public UvType UvType { get; set; }

      public ColorType ColorType { get; set; }

      public IReadOnlyImage Image { get; }
      public Bitmap ImageData => this.imageData_ ??= Image.AsBitmap();

      public ISystemFile SaveInDirectory(ISystemDirectory directory) {
        ISystemFile outFile =
            new FinFile(Path.Combine(directory.FullPath, this.ValidFileName));
        using var writer = outFile.OpenWrite();
        this.Image.ExportToStream(writer, BestImageFormat);
        return outFile;
      }

      public ImageTransparencyType TransparencyType
        => this.transparencyType_ ??= ImageUtil.GetTransparencyType(this.Image);

      public WrapMode WrapModeU { get; set; }
      public WrapMode WrapModeV { get; set; }

      public IColor? BorderColor { get; set; }

      public TextureMagFilter MagFilter { get; set; } = TextureMagFilter.LINEAR;

      public TextureMinFilter MinFilter { get; set; } =
        TextureMinFilter.LINEAR_MIPMAP_LINEAR;

      public float MinLod { get; set; } = -1000;
      public float LodBias { get; set; } = 0;

      public IReadOnlyVector2? ClampS { get; set; }
      public IReadOnlyVector2? ClampT { get; set; }


      public bool IsTransform3d { get; private set; }


      public IReadOnlyXyz? Offset { get; private set; }

      public ITexture SetOffset2d(float x, float y) {
        this.Offset = new Vector3f { X = x, Y = y };
        return this;
      }

      public ITexture SetOffset3d(float x, float y, float z) {
        this.Offset = new Vector3f { X = x, Y = y, Z = z };
        this.IsTransform3d = true;
        return this;
      }


      public IReadOnlyXyz? Scale { get; private set; }

      public ITexture SetScale2d(float x, float y) {
        this.Scale = new Vector3f { X = x, Y = y };
        return this;
      }

      public ITexture SetScale3d(float x, float y, float z) {
        this.Scale = new Vector3f { X = x, Y = y, Z = z };
        this.IsTransform3d = true;
        return this;
      }


      public IReadOnlyXyz? RotationRadians { get; private set; }

      public ITexture SetRotationRadians2d(float rotationRadians) {
        this.RotationRadians = new Vector3f { Z = rotationRadians };
        return this;
      }

      public ITexture SetRotationRadians3d(float xRadians,
                                           float yRadians,
                                           float zRadians) {
        this.RotationRadians = new Vector3f
            { X = xRadians, Y = yRadians, Z = zRadians };
        this.IsTransform3d = true;
        return this;
      }

      public override int GetHashCode()
        => new FluentHash()
           .With(Image)
           .With(WrapModeU)
           .With(WrapModeV);

      public override bool Equals(object? other) {
        if (ReferenceEquals(null, other)) {
          return false;
        }

        if (ReferenceEquals(this, other)) {
          return true;
        }

        if (other is ITexture otherTexture) {
          return this.Image == otherTexture.Image &&
                 this.WrapModeU == otherTexture.WrapModeU &&
                 this.WrapModeV == otherTexture.WrapModeV;
        }

        return false;
      }
    }
  }
}