using System.Runtime.CompilerServices;

using fin.data.disposables;

using OpenTK.Graphics.ES30;

using PixelFormat = OpenTK.Graphics.ES30.PixelFormat;
using TextureMagFilter = OpenTK.Graphics.ES30.TextureMagFilter;
using TextureMinFilter = OpenTK.Graphics.ES30.TextureMinFilter;

namespace fin.ui.rendering.gl.texture;

public class GlFbo : IFinDisposable {
  private int fboId_;
  private int colorTextureId_;
  private int depthTextureId_;

  public GlFbo(int width, int height) {
    this.Width = width;
    this.Height = height;

    // Create Color Tex
    GL.GenTextures(1, out this.colorTextureId_);
    GL.BindTexture(TextureTarget.Texture2D, this.colorTextureId_);
    GL.TexImage2D(TextureTarget2d.Texture2D,
                  0,
                  TextureComponentCount.Rgba8,
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

    // Create Depth Tex
    GL.GenTextures(1, out this.depthTextureId_);
    GL.BindTexture(TextureTarget.Texture2D, this.depthTextureId_);
    GL.TexImage2D(TextureTarget2d.Texture2D,
                  0,
                  TextureComponentCount.DepthComponent32f,
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

    // Create a FBO and attach the textures
    GL.GenFramebuffers(1, out this.fboId_);
    GL.BindFramebuffer(FramebufferTarget.Framebuffer, this.fboId_);
    GL.FramebufferTexture2D(FramebufferTarget.Framebuffer,
                            FramebufferAttachment.ColorAttachment0Ext,
                            TextureTarget2d.Texture2D,
                            this.colorTextureId_,
                            0);
    GL.FramebufferTexture2D(FramebufferTarget.Framebuffer,
                            FramebufferAttachment.DepthAttachmentExt,
                            TextureTarget2d.Texture2D,
                            this.depthTextureId_,
                            0);
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
    GL.DeleteTextures(1, ref this.colorTextureId_);
    GL.DeleteTextures(1, ref this.depthTextureId_);
  }

  public int Width { get; }
  public int Height { get; }
  public int ColorTextureId => this.colorTextureId_;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void Bind(int textureIndex = 0)
    => GlUtil.BindTexture(textureIndex, this.colorTextureId_);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void BindDepth(int textureIndex = 0)
    => GlUtil.BindTexture(textureIndex, this.depthTextureId_);

  public void TargetFbo()
    => GL.BindFramebuffer(FramebufferTarget.Framebuffer, this.fboId_);

  public void UntargetFbo()
    => GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
}