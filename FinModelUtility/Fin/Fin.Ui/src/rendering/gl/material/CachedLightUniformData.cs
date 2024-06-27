using System.Numerics;

using fin.model;

using OpenTK.Graphics.OpenGL;


namespace fin.ui.rendering.gl.material;

public class CachedLightUniformData {
  public IReadOnlyLight Light { get; }

  public IShaderUniform<bool> EnabledUniform { get; }

  public IShaderUniform<int> SourceTypeUniform { get; }
  public IShaderUniform<Vector3> PositionUniform { get; }
  public IShaderUniform<Vector3> NormalUniform { get; }

  public IShaderUniform<Vector4> ColorUniform { get; }

  public IShaderUniform<int> DiffuseFunctionUniform { get; }
  public IShaderUniform<int> AttenuationFunctionUniform { get; }
  public IShaderUniform<Vector3> CosineAttenuationUniform { get; }
  public IShaderUniform<Vector3> DistanceAttenuationUniform { get; }

  public CachedLightUniformData(
      int lightIndex,
      IReadOnlyLight light,
      GlShaderProgram shaderProgram) {
      this.Light = light;

      var lightAccessor = $"{MaterialConstants.LIGHTS_NAME}[{lightIndex}]";

      this.EnabledUniform =
          shaderProgram.GetUniformBool($"{lightAccessor}.enabled");

      this.SourceTypeUniform =
          shaderProgram.GetUniformInt($"{lightAccessor}.sourceType");
      this.PositionUniform =
          shaderProgram.GetUniformVec3($"{lightAccessor}.position");
      this.NormalUniform =
          shaderProgram.GetUniformVec3($"{lightAccessor}.normal");

      this.ColorUniform =
          shaderProgram.GetUniformVec4($"{lightAccessor}.color");

      this.DiffuseFunctionUniform =
          shaderProgram.GetUniformInt($"{lightAccessor}.diffuseFunction");
      this.AttenuationFunctionUniform =
          shaderProgram.GetUniformInt($"{lightAccessor}.attenuationFunction");
      this.CosineAttenuationUniform =
          shaderProgram.GetUniformVec3($"{lightAccessor}.cosineAttenuation");
      this.DistanceAttenuationUniform =
          shaderProgram.GetUniformVec3($"{lightAccessor}.distanceAttenuation");
    }

  public void PassInUniforms() {
      var light = this.Light;
      if (!light.Enabled) {
        return;
      }

      this.EnabledUniform.SetAndMaybeMarkDirty(true);

      this.SourceTypeUniform.SetAndMaybeMarkDirty((int) light.SourceType);

      var position = light.Position;
      if (position != null) {
        this.PositionUniform.SetAndMaybeMarkDirty(
            new Vector3(position.X, position.Y, position.Z));
      }

      var normal = light.Normal;
      if (normal != null) {
        this.NormalUniform.SetAndMaybeMarkDirty(
            new Vector3(normal.X, normal.Y, normal.Z));
      }

      var strength = light.Strength;
      var color = light.Color;
      this.ColorUniform.SetAndMaybeMarkDirty(
          new Vector4(color.Rf * strength,
                      color.Gf * strength,
                      color.Bf * strength,
                      color.Af * strength));

      this.DiffuseFunctionUniform.SetAndMaybeMarkDirty(
          (int) light.DiffuseFunction);
      this.AttenuationFunctionUniform.SetAndMaybeMarkDirty(
          (int) light.AttenuationFunction);

      var cosineAttenuation = light.CosineAttenuation;
      if (cosineAttenuation != null) {
        this.CosineAttenuationUniform.SetAndMaybeMarkDirty(new Vector3(
            cosineAttenuation.X,
            cosineAttenuation.Y,
            cosineAttenuation.Z));
      }

      var distanceAttenuation = light.DistanceAttenuation;
      if (distanceAttenuation != null) {
        this.DistanceAttenuationUniform.SetAndMaybeMarkDirty(new Vector3(
            distanceAttenuation.X,
            distanceAttenuation.Y,
            distanceAttenuation.Z));
      }
    }
}