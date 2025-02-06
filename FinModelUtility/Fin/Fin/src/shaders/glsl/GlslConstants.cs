namespace fin.shaders.glsl;

public static class GlslConstants {
  public const string VERTEX_SHADER_VERSION = "430";
  public const string FRAGMENT_SHADER_VERSION = "310 es";

  public const string FLOAT_PRECISION = "precision highp float;";

  public const int UBO_MATRICES_BINDING_INDEX = 1;
  public const string UBO_MATRICES_NAME = "Matrices";

  public const int UBO_LIGHTS_BINDING_INDEX = 2;
  public const string UBO_LIGHTS_NAME = "Lights";

  public const string UNIFORM_MODEL_MATRIX_NAME = "modelMatrix";
  public const string UNIFORM_VIEW_MATRIX_NAME = "viewMatrix";
  public const string UNIFORM_PROJECTION_MATRIX_NAME = "projectionMatrix";
  public const string UNIFORM_CAMERA_POSITION_NAME = "cameraPosition";

  public const string UNIFORM_BONE_MATRICES_NAME = "boneMatrices";
  public const string UNIFORM_USE_LIGHTING_NAME = "useLighting";
  public const string UNIFORM_SHININESS_NAME = "shininess";

  public const string IN_UV_NAME = "uv";
  public const string IN_VERTEX_COLOR_NAME = "vertexColor";
  public const string IN_SPHERICAL_REFLECTION_UV_NAME = "sphericalReflectionUv";

  public const string MIN_ALPHA_BEFORE_DISCARD_MASK_TEXT = ".95";
  public const string MIN_ALPHA_BEFORE_DISCARD_TRANSPARENT_TEXT = ".01";
}