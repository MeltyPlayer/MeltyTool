using fin.util.types;

using OpenTK.Graphics.OpenGL;

namespace fin.ui.rendering.gl;

[IocCandiate]
public static class OpenGlFeatureServiceTester {
  public static void TestForFeatures() {
    var extensions = new HashSet<string>(GL.GetInteger(GetPName.NumExtensions));
    for (int i = 0; i < extensions.Count; i++) {
      var extension = GL.GetString(StringNameIndexed.Extensions, i);
      extensions.Add(extension);
    }

    OpenGlFeatureService.SupportsBindlessTextures
        = extensions.Contains("GL_ARB_bindless_texture");

    OpenGlFeatureService.SupportsNamedBufferStorage
        = (!OpenGlVersionService.Es &&
           OpenGlVersionService.MajorVersion >= 4 &&
           OpenGlVersionService.MinorVersion >= 5) ||
          extensions.Contains("GL_ARB_direct_state_access");

    OpenGlFeatureService.SupportsSsbos
        = (!OpenGlVersionService.Es &&
           OpenGlVersionService.MajorVersion >= 4 &&
           OpenGlVersionService.MinorVersion >= 3) ||
          extensions.Contains("GL_ARB_shader_storage_buffer_object");
  }
}