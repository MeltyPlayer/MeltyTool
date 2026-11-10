using fin.ui.rendering.gl;

namespace fin.shaders.glsl;

public static class GlslConstants {
  public static string SHADER_VERSION
    => $"{OpenGlVersionService.MajorVersion}{OpenGlVersionService.MinorVersion}0{(
        OpenGlVersionService.Es ? " es" : "")}";

  public static string VERTEX_SHADER_VERSION => SHADER_VERSION;
  public static string FRAGMENT_SHADER_VERSION => SHADER_VERSION;

  public const string FLOAT_PRECISION = "precision mediump float;";

  public const int UBO_GLOBAL_MATRICES_BINDING_INDEX = 1;
  public const string UBO_GLOBAL_MATRICES_NAME = "GlobalMatrices";

  public const string UNIFORM_PROJECTION_VIEW_MATRIX_NAME = "projectionViewMatrix";

  public const int UBO_CURRENT_MATRICES_BINDING_INDEX = 2;
  public const string UBO_CURRENT_MATRICES_NAME = "CurrentMatrices";

  public const string UNIFORM_MODEL_MATRIX_NAME = "modelMatrix";

  public const int UBO_LIGHTS_BINDING_INDEX = 3;
  public const string UBO_LIGHTS_NAME = "Lights";

  public const int UBO_PRIMITIVE_METADATA_BINDING_INDEX = 4;
  public const string UBO_PRIMITIVE_METADATA_NAME = "PrimitiveMetadataSsbo";

  public const string UNIFORM_CAMERA_POSITION_NAME = "cameraPosition";

  public const string UNIFORM_BONE_MATRICES_NAME = "boneMatrices";
  public const string UNIFORM_USE_LIGHTING_NAME = "useLighting";

  public const string UNIFORM_HAS_SPECULAR_NAME = "hasSpecular";
  public const string UNIFORM_SHININESS_NAME = "shininess";

  public const string UNIFORM_FRAG_COLOR_NAME = "fragColor";

  public const string IN_UV_NAME = "uv";
  public const string IN_VERTEX_COLOR_NAME = "vertexColor";
  public const string IN_SPHERICAL_REFLECTION_UV_NAME = "sphericalReflectionUv";
  public const string IN_LINEAR_REFLECTION_UV_NAME = "linearReflectionUv";

  public const float MIN_ALPHA_BEFORE_DISCARD_MASK = .95f;
  public const string MIN_ALPHA_BEFORE_DISCARD_MASK_TEXT = ".95";
  public const float MIN_ALPHA_BEFORE_DISCARD_TRANSPARENT = .01f;
  public const string MIN_ALPHA_BEFORE_DISCARD_TRANSPARENT_TEXT = ".01";

  public const string TEXTURE_3_POINT_NAME = "texture_3point";
}