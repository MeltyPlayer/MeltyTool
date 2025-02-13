using System.Collections.Generic;
using System.Numerics;

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
  new IReadOnlyList<ILight> Lights { get; }

  ILight CreateLight();

  new IColor AmbientLightColor { get; set; }
  new float AmbientLightStrength { get; set; }
}

[GenerateReadOnly]
public partial interface ILight {
  new string Name { get; }
  ILight SetName(string name);

  new bool Enabled { get; set; }

  new LightSourceType SourceType { get; }

  new IReadOnlyXyz? Position { get; }
  ILight SetPosition(Vector3 position);
  ILight SetPosition(IReadOnlyXyz position);

  new IReadOnlyXyz? Normal { get; }
  ILight SetNormal(Vector3 normal);
  ILight SetNormal(IReadOnlyXyz normal);

  new float Strength { get; set; }

  new IColor Color { get; }
  ILight SetColor(IColor color);

  new IReadOnlyXyz? CosineAttenuation { get; }
  ILight SetCosineAttenuation(IReadOnlyXyz cosineAttenuation);
  new IReadOnlyXyz? DistanceAttenuation { get; }
  ILight SetDistanceAttenuation(IReadOnlyXyz distanceAttenuation);

  new AttenuationFunction AttenuationFunction { get; }
  ILight SetAttenuationFunction(AttenuationFunction attenuationFunction);
  new DiffuseFunction DiffuseFunction { get; }
  ILight SetDiffuseFunction(DiffuseFunction diffuseFunction);
}