namespace gx;

public enum GxColorSrc : byte {
  Register = 0, // Use Register Colors
  Vertex = 1    // Use Vertex Colors
}

public enum GxAttenuationFunction : byte {
  // No attenuation
  None = 2,

  // Specular Computation
  Spec = 0,

  // Spot Light Attenuation
  Spot = 1
}

[Flags]
public enum GxLightMask : byte {
  Light0 = 0x01,
  Light1 = 0x02,
  Light2 = 0x04,
  Light3 = 0x08,
  Light4 = 0x10,
  Light5 = 0x20,
  Light6 = 0x40,
  Light7 = 0x80,
  None = 0x00
}

public static class GxLightMaskExtensions {
  public static IEnumerable<int> GetActiveLights(this GxLightMask lightMask) {
      for (var i = 0; i < 8; ++i) {
        var bitMask = 1 << i;
        if (((int) lightMask & bitMask) != 0) {
          yield return i;
        }
      }
    }
}

public enum GxDiffuseFunction : byte {
  None = 0,
  Signed = 1,
  Clamp = 2
}


public interface IColorChannelControl {
  bool LightingEnabled { get; }

  GxColorSrc MaterialSrc { get; }

  /// <summary>
  ///   Which lights affect the vertex.
  /// </summary>
  GxLightMask LitMask { get; }

  /// <summary>
  ///   How to merge the lights together.
  /// </summary>
  GxDiffuseFunction DiffuseFunction { get; }

  /// <summary>
  ///   What type of attenuation function to use for the lights.
  /// </summary>
  GxAttenuationFunction AttenuationFunction { get; }

  GxColorSrc AmbientSrc { get; }
}

public class ColorChannelControlImpl : IColorChannelControl {
  public bool LightingEnabled { get; set; }
  public GxColorSrc MaterialSrc { get; set; }
  public GxLightMask LitMask { get; set; }
  public GxDiffuseFunction DiffuseFunction { get; set; }
  public GxAttenuationFunction AttenuationFunction { get; set; }
  public GxColorSrc AmbientSrc { get; set; }
}