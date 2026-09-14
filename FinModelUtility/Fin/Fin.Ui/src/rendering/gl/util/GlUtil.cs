using System.Runtime.InteropServices;

using fin.model;
using fin.ui.rendering.gl.material;

using OpenTK.Graphics.OpenGL4;

namespace fin.ui.rendering.gl;

public static partial class GlUtil {
  public static bool IsInitialized { get; private set; }

  public static void InitDll() {
    if (IsInitialized) {
      return;
    }

    // (Set up DLL here, if ever needed again.)

    IsInitialized = true;
  }

  private static readonly Lock GL_LOCK_ = new();

  public static void RunLockedGl(Action handler) {
    lock (GL_LOCK_) {
      handler();
    }
  }

  public static void InitGl() {
    GL.Enable(EnableCap.DebugOutput);
    GL.DebugMessageCallback(
        (source,
         type,
         id,
         severity,
         length,
         messagePtr,
         userParam) => {
          var message = Marshal.PtrToStringAnsi(messagePtr);

          if (type == DebugType.DebugTypeError) {
            throw new Exception(message);
          }

          Console.WriteLine(message);
        },
        0);

    GlMaterialConstants.Initialize();

    ResetGl();
  }

  public static void ResetGl() {
    GL.ClearDepth(5.0F);
    GL.Enable(EnableCap.PrimitiveRestartFixedIndex);

    if (!OpenGlVersionService.Es) {
      GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Fill);
    }

    ResetBlending();
    ResetClearColor();
    ResetCulling();
    ResetDepth();
    ResetFlipFaces();
    ResetUboData();
    ResetUboBufferBase();
    ResetVao();
    ResetEbo();

    for (var i = 0; i < MaterialConstants.MAX_TEXTURES; ++i) {
      UnbindTexture(i);
    }

    // This index is used for normals
    UnbindTexture(MaterialConstants.MAX_TEXTURES);
  }
}