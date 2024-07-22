namespace fin.shaders.glsl;

public static class GlslConstants {
  public const int UBO_MATRIX_BINDING_INDEX = 1;
  public const string UBO_MATRIX_NAME = "Matrix";

  public const string UNIFORM_MODEL_MATRIX_NAME = "modelMatrix";
  public const string UNIFORM_MODEL_VIEW_MATRIX_NAME = "modelViewMatrix";
  public const string UNIFORM_PROJECTION_MATRIX_NAME = "projectionMatrix";
  public const string UNIFORM_CAMERA_POSITION_NAME = "cameraPosition";

  public const string UNIFORM_BONE_MATRICES_NAME = "boneMatrices";
  public const string UNIFORM_USE_LIGHTING_NAME = "useLighting";
  public const string UNIFORM_SHININESS_NAME = "shininess";

  public const float MIN_ALPHA_BEFORE_DISCARD = .95f;
  public const string MIN_ALPHA_BEFORE_DISCARD_TEXT = ".95";
}