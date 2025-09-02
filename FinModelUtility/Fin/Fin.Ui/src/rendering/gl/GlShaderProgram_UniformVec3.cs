﻿using System.Numerics;

using fin.util.asserts;

using OpenTK.Graphics.ES30;


namespace fin.ui.rendering.gl;

public partial class GlShaderProgram {
  public IShaderUniform<Vector3> GetUniformVec3(string name) {
      if (!this.cachedUniforms_.TryGetValue(name, out var uniform)) {
        this.cachedUniforms_[name] = uniform =
            new Vec3ShaderUniform(this.GetUniformLocation_(name));
      }

      return Asserts.AsA<IShaderUniform<Vector3>>(uniform);
    }

  private class Vec3ShaderUniform(int location)
      : BShaderUniform, IShaderUniform<Vector3> {
    private Vector3 value_;

    public void SetAndMarkDirty(in Vector3 value) {
        this.value_ = value;
        this.MarkDirty();
      }

    public void SetAndMaybeMarkDirty(in Vector3 value) {
        if (this.value_.Equals(value)) {
          return;
        }

        this.value_ = value;
        this.MarkDirty();
      }

    protected override unsafe void PassValueToProgram() {
        fixed (float* ptr = &this.value_.X) {
          GL.Uniform3(location, 1, ptr);
        }
      }
  }
}