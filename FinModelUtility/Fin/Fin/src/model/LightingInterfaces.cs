using System.Collections.Generic;

using fin.color;
using fin.math.xyz;

using schema.readOnly;

namespace fin.model;

public enum LightSourceType {
  UNDEFINED,
  POSITION,
  RAY,
  LINE,
}

public enum AttenuationFunction {
  NONE,
  SPECULAR,
  SPOT,
}

public enum DiffuseFunction {
  NONE,
  SIGNED,
  CLAMP,
}

[GenerateReadOnly]
public partial interface ILighting {
  IReadOnlyList<ILight> Lights { get; }

  ILight CreateLight();

  IColor AmbientLightColor { get; set; }
  float AmbientLightStrength { get; set; }
}

[GenerateReadOnly]
public partial interface ILight {
  string Name { get; }
  ILight SetName(string name);

  bool Enabled { get; set; }

  LightSourceType SourceType { get; }

  IReadOnlyXyz? Position { get; }
  ILight SetPosition(IReadOnlyXyz position);

  IReadOnlyXyz? Normal { get; }
  ILight SetNormal(IReadOnlyXyz normal);

  float Strength { get; set; }

  IColor Color { get; }
  ILight SetColor(IColor color);

  IReadOnlyXyz? CosineAttenuation { get; }
  ILight SetCosineAttenuation(IReadOnlyXyz cosineAttenuation);
  IReadOnlyXyz? DistanceAttenuation { get; }
  ILight SetDistanceAttenuation(IReadOnlyXyz distanceAttenuation);

  AttenuationFunction AttenuationFunction { get; }
  ILight SetAttenuationFunction(AttenuationFunction attenuationFunction);
  DiffuseFunction DiffuseFunction { get; }
  ILight SetDiffuseFunction(DiffuseFunction diffuseFunction);
}