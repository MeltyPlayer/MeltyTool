using IronPython.Runtime;

// ReSharper disable InconsistentNaming

namespace ModelPluginWrappers.noesis;

public class Noesis {
  public static Dictionary<string, Handle> HandlesByExtension { get; } = new();

  public static void logPopup() { }

  public static Handle register(string formatName, string extension)
    => HandlesByExtension[extension] = new Handle(formatName, extension);

  public static void setHandlerTypeCheck(Handle handle,
                                         Func<byte[], bool> checkType)
    => handle.checkType = checkType;

  public static void setHandlerLoadModel(Handle handle,
                                         Func<byte[], PythonList, bool>
                                             loadModel)
    => handle.loadModel = loadModel;

  public static void vec3Validate(dynamic _) { }
  public static void vec4Validate(dynamic _) { }

  public record Handle(string FormatName, string Extension) {
    public Func<byte[], bool> checkType;
    public Func<byte[], PythonList, bool> loadModel;
  }

  public enum PixelType {
    NOESISTEX_RGBA32
  }

  public enum InterpolationType {
    NOEKF_INTERPOLATE_LINEAR,
  }

  public enum KeyframeType {
    NOEKF_ROTATION_QUATERNION_4,
    NOEKF_TRANSLATION_VECTOR_3,
    NOEKF_SCALE_SCALAR_1,
  }

  public enum NoeFormat {
    RPGEODATA_FLOAT,
    RPGEODATA_USHORT,
  }

  public enum NoePrimitiveType {
    RPGEO_POINTS,
  }
}