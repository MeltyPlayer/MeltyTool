using CommunityToolkit.HighPerformance;

using fin.model;
using fin.shaders.glsl;

using Vector4 = System.Numerics.Vector4;

namespace fin.ui.rendering.gl.ubo;

public class LightsUbo : IDisposable {
  private const int SIZE_OF_LIGHT = (UboUtil.SIZE_OF_VECTOR3 + 4) +
                                    (UboUtil.SIZE_OF_VECTOR3 + 4) +
                                    UboUtil.SIZE_OF_VECTOR4 +
                                    (UboUtil.SIZE_OF_VECTOR3 + 4) +
                                    (UboUtil.SIZE_OF_VECTOR3 + 4);

  private const int SIZE_OF_BUFFER
      = SIZE_OF_LIGHT * MaterialConstants.MAX_LIGHTS +
        UboUtil.SIZE_OF_VECTOR4 +
        4;

  private readonly GlUbo impl_
      = new(SIZE_OF_BUFFER, GlslConstants.UBO_LIGHTS_BINDING_INDEX);

  ~LightsUbo() => this.ReleaseUnmanagedResources_();

  public void Dispose() {
    this.ReleaseUnmanagedResources_();
    GC.SuppressFinalize(this);
  }

  private void ReleaseUnmanagedResources_() => this.impl_.Dispose();

  public void UpdateData(bool useLighting, IReadOnlyLighting? lighting) {
    var offset = 0;
    Span<byte> buffer = stackalloc byte[SIZE_OF_BUFFER];

    if (!useLighting || lighting == null) {
      offset += SIZE_OF_LIGHT * MaterialConstants.MAX_LIGHTS +
                UboUtil.SIZE_OF_VECTOR4;
      UboUtil.AppendBool(buffer, ref offset, false);
    } else {
      var lights = lighting.Lights;
      for (var i = 0; i < MaterialConstants.MAX_LIGHTS; ++i) {
        AddLightToBuffer_(buffer,
                          ref offset,
                          i < lights?.Count ? lights[i] : null);
      }

      var ambientLightColor = new Vector4(lighting.AmbientLightColor.Rf,
                                          lighting.AmbientLightColor.Gf,
                                          lighting.AmbientLightColor.Bf,
                                          lighting.AmbientLightColor.Af) *
                              lighting.AmbientLightStrength;
      UboUtil.AppendVector4(buffer, ref offset, ambientLightColor);
      UboUtil.AppendBool(buffer, ref offset, true);
    }

    this.impl_.UpdateDataIfChanged(buffer);
  }

  public void Bind() => this.impl_.Bind();

  private static void AddLightToBuffer_(Span<byte> buffer,
                                        ref int offset,
                                        IReadOnlyLight? light) {
    if (light == null) {
      offset += 3;
      buffer.Cast<byte, int>()[0] = 0;
      offset += SIZE_OF_LIGHT - 3;
      return;
    }

    UboUtil.AppendVector3(buffer, ref offset, light.Position);
    UboUtil.AppendBool(buffer, ref offset, light.Enabled);

    UboUtil.AppendVector3(buffer, ref offset, light.Normal);
    UboUtil.AppendInt(buffer, ref offset, (int) light.SourceType);

    var lightColor = new Vector4(light.Color.Rf,
                                 light.Color.Gf,
                                 light.Color.Bf,
                                 light.Color.Af) *
                     light.Strength;
    UboUtil.AppendVector4(buffer, ref offset, lightColor);

    UboUtil.AppendVector3(buffer, ref offset, light.CosineAttenuation);
    UboUtil.AppendInt(buffer, ref offset, (int) light.DiffuseFunction);

    UboUtil.AppendVector3(buffer, ref offset, light.DistanceAttenuation);
    UboUtil.AppendInt(buffer, ref offset, (int) light.AttenuationFunction);
  }
}