using fin.util.asserts;

using OpenTK.Graphics.OpenGL4;


namespace fin.ui.rendering.gl;

public partial class GlShaderProgram {
  public ISamplerShaderUniform GetUniformSampler2d(string name) {
    if (!this.cachedUniformsByName_.TryGetValue(name, out var uniform)) {
      this.cachedUniformsByName_[name] = uniform =
          new Sampler2dShaderUniform(this.GetUniformLocation_(name));
    }

    return Asserts.AsA<ISamplerShaderUniform>(uniform);
  }

  private class Sampler2dShaderUniform(int location)
      : BShaderUniform(location), ISamplerShaderUniform {
    private enum SamplerMode {
      UNDEFINED,
      HANDLE,
      TEXTURE_INDEX,
    }

    private SamplerMode mode_;
    private long handle_;
    private int textureIndex_;

    // TODO: Can we switch to maybe mark dirty here?
    public void SetHandle(long handle) {
      this.mode_ = SamplerMode.HANDLE;
      this.handle_ = handle;
      this.MarkDirty();
    }

    public void SetTextureIndex(int textureIndex) {
      this.mode_ = SamplerMode.TEXTURE_INDEX;

      if (this.textureIndex_ == textureIndex) {
        return;
      }

      this.textureIndex_ = textureIndex;
      this.MarkDirty();
    }

    protected override void PassValueToProgram() {
      switch (this.mode_) {
        case SamplerMode.HANDLE: {
          GL.Arb.UniformHandle(location, this.handle_);
          break;
        }
        case SamplerMode.TEXTURE_INDEX: {
          GL.Uniform1(location, this.textureIndex_);
          break;
        }
      }
    }
  }
}