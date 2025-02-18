using System.Drawing;
using System.Runtime.CompilerServices;

using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

using QuickFont;

namespace fin.ui.rendering.gl.texture;

public class GlTextTexture : IGlTexture {
  private readonly GlFbo impl_;

  public GlTextTexture(string text,
                       QFont font,
                       QFontAlignment alignment = QFontAlignment.Left) {
    var size = font.Measure(text);
    var width = (int) MathF.Ceiling(size.Width);
    var height = (int) MathF.Ceiling(size.Height);

    this.impl_ = new GlFbo(width, height);

    this.impl_.TargetFbo();
    GL.Viewport(0, 0, width, height);
    GL.ClearColor(0, 0, 0, 0);
    GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
    {
      var x = alignment switch {
          QFontAlignment.Left    => 0,
          QFontAlignment.Centre  => width / 2,
          QFontAlignment.Right   => width,
          _                      => throw new ArgumentOutOfRangeException(nameof(alignment), alignment, null)
      };

      using var drawing = new QFontDrawing();
      drawing.Print(font,
                    text,
                    new Vector3(x, height, 0),
                    alignment,
                    new QFontRenderOptions {
                        Colour = Color.Black,
                    });
      drawing.ProjectionMatrix = Matrix4.CreateOrthographicOffCenter(
          0,
          width,
          0,
          height,
          -1.0f,
          1.0f);
      drawing.RefreshBuffers();
      drawing.Draw();
    }
    GL.Flush();
    this.impl_.UntargetFbo();
  }

  ~GlTextTexture() => this.ReleaseUnmanagedResources_();

  public bool IsDisposed { get; private set; }

  public void Dispose() {
    this.ReleaseUnmanagedResources_();
    GC.SuppressFinalize(this);
  }

  private void ReleaseUnmanagedResources_() {
    this.IsDisposed = true;
    this.impl_.Dispose();
  }

  public int Width => this.impl_.Width;
  public int Height => this.impl_.Height;
  public int Id => this.impl_.ColorTextureId;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void Bind(int textureIndex = 0) => this.impl_.Bind();
}