using System.Drawing;
using System.Runtime.CompilerServices;

using fin.data.disposables;
using fin.util.asserts;

using OpenTK.Graphics.OpenGL4;

namespace fin.ui.rendering.gl.texture;

public sealed class GlFbo : IFinDisposable {
  private int fboId_;

  private GlWrapperTexture colorTexture_;
  private GlWrapperTexture depthTexture_;

  public GlFbo(int width, int height) {
    this.Width = width;
    this.Height = height;

    // Create Color Tex
    GL.GenTextures(1, out int colorTextureId);
    GL.BindTexture(TextureTarget.Texture2D, colorTextureId);
    GL.TexImage2D(TextureTarget.Texture2D,
                  0,
                  PixelInternalFormat.Rgba8,
                  width,
                  height,
                  0,
                  PixelFormat.Rgba,
                  PixelType.UnsignedByte,
                  IntPtr.Zero);
    GL.TexParameter(TextureTarget.Texture2D,
                    TextureParameterName.TextureMinFilter,
                    (int) TextureMinFilter.Linear);
    GL.TexParameter(TextureTarget.Texture2D,
                    TextureParameterName.TextureMagFilter,
                    (int) TextureMagFilter.Linear);
    GL.TexParameter(TextureTarget.Texture2D,
                    TextureParameterName.TextureWrapS,
                    (int) TextureWrapMode.ClampToBorder);
    GL.TexParameter(TextureTarget.Texture2D,
                    TextureParameterName.TextureWrapT,
                    (int) TextureWrapMode.ClampToBorder);

    this.colorTexture_ = new GlWrapperTexture(colorTextureId);

    // Create Depth Tex
    GL.GenTextures(1, out int depthTextureId);
    GL.BindTexture(TextureTarget.Texture2D, depthTextureId);
    GL.TexImage2D(TextureTarget.Texture2D,
                  0,
                  PixelInternalFormat.DepthComponent,
                  width,
                  height,
                  0,
                  PixelFormat.DepthComponent,
                  PixelType.UnsignedInt,
                  IntPtr.Zero);
    // things go horribly wrong if DepthComponent's Bitcount does not match the main Framebuffer's Depth
    GL.TexParameter(TextureTarget.Texture2D,
                    TextureParameterName.TextureMinFilter,
                    (int) TextureMinFilter.Linear);
    GL.TexParameter(TextureTarget.Texture2D,
                    TextureParameterName.TextureMagFilter,
                    (int) TextureMagFilter.Linear);
    GL.TexParameter(TextureTarget.Texture2D,
                    TextureParameterName.TextureWrapS,
                    (int) TextureWrapMode.ClampToBorder);
    GL.TexParameter(TextureTarget.Texture2D,
                    TextureParameterName.TextureWrapT,
                    (int) TextureWrapMode.ClampToBorder);

    this.depthTexture_ = new GlWrapperTexture(depthTextureId);

    // Create a FBO and attach the textures
    GL.GenFramebuffers(1, out this.fboId_);
    GL.BindFramebuffer(FramebufferTarget.Framebuffer, this.fboId_);
    GL.FramebufferTexture2D(FramebufferTarget.Framebuffer,
                            FramebufferAttachment.ColorAttachment0,
                            TextureTarget.Texture2D,
                            colorTextureId,
                            0);
    GL.FramebufferTexture2D(FramebufferTarget.Framebuffer,
                            FramebufferAttachment.DepthAttachment,
                            TextureTarget.Texture2D,
                            depthTextureId,
                            0);

    Asserts.Equal(FramebufferErrorCode.FramebufferComplete,
                  GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer));
  }

  ~GlFbo() => this.ReleaseUnmanagedResources_();

  public bool IsDisposed { get; private set; }

  public void Dispose() {
    this.ReleaseUnmanagedResources_();
    GC.SuppressFinalize(this);
  }

  private void ReleaseUnmanagedResources_() {
    this.IsDisposed = true;

    GL.DeleteFramebuffers(1, ref this.fboId_);
    this.colorTexture_.Dispose();
    this.depthTexture_.Dispose();
  }

  public int Width { get; }
  public int Height { get; }

  public IGlTexture ColorTexture => this.colorTexture_;
  public IGlTexture DepthTexture => this.depthTexture_;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void Bind(int textureIndex = 0)
    => this.colorTexture_.Bind(textureIndex);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void BindDepth(int textureIndex = 0)
    => this.depthTexture_.Bind(textureIndex);

  public void InvokeAsDrawTarget(Action handler) {
    var originalFramebuffer = GL.GetInteger(GetPName.DrawFramebufferBinding);
    var originalViewport = GlUtil.GetViewport();
    GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, this.fboId_);
    GlUtil.SetViewport(new Rectangle(0, 0, this.Width, this.Height));

    handler();

    GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, originalFramebuffer);
    GlUtil.SetViewport(originalViewport);
  }

  public void InvokeAsReadTarget(Action handler) {
    var originalFramebuffer = GL.GetInteger(GetPName.ReadFramebufferBinding);
    GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, this.fboId_);

    handler();

    GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, originalFramebuffer);
  }


  private class GlWrapperTexture(int id) : IGlTexture {
    ~GlWrapperTexture() => this.ReleaseUnmanagedResources_();

    public bool IsDisposed { get; private set; }

    public void Dispose() {
      this.ReleaseUnmanagedResources_();
      GC.SuppressFinalize(this);
    }

    private void ReleaseUnmanagedResources_() {
      if (this.IsDisposed) {
        return;
      }

      this.IsDisposed = true;
      GL.DeleteTextures(1, ref id);
    }

    public int Id => id;

    public void Bind(int textureIndex = 0)
      => GlUtil.BindTexture(textureIndex, id);
  }
}