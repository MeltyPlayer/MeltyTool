using fin.image;
using fin.image.formats;

using OpenTK.Graphics.OpenGL;

using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;

namespace fin.ui.rendering.gl.texture;

public static class GlTextureExtensions {
  public static unsafe IImage ConvertToImage(this IGlTexture texture) {
    var width = texture.Width;
    var height = texture.Height;

    var image = new Rgba32Image(width, height);

    using var fastLock = image.UnsafeLock();
    GL.GetTextureImage(texture.Id,
                       0,
                       PixelFormat.Rgba,
                       PixelType.UnsignedByte,
                       4 * width * height,
                       new IntPtr(fastLock.byteScan0));

    return image;
  }
}