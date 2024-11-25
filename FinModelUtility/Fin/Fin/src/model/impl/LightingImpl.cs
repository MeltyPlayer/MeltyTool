using System.Collections.Generic;
using System.Drawing;
using System.Numerics;

using fin.color;
using fin.math.xyz;
using fin.schema.vector;

namespace fin.model.impl;

public partial class ModelImpl<TVertex> {
  public IReadOnlyLighting? Lighting { get; }
}

public class LightingImpl : ILighting {
  private readonly List<ILight> lights_ = [];

  public IReadOnlyList<ILight> Lights => this.lights_;

  public ILight CreateLight() {
    var light = new LightImpl();
    this.lights_.Add(light);
    return light;
  }

  public float AmbientLightStrength { get; set; } = .1f;

  public IColor AmbientLightColor { get; set; } =
    FinColor.FromSystemColor(Color.White);

  private class LightImpl : ILight {
    public string Name { get; private set; }

    public ILight SetName(string name) {
      this.Name = name;
      return this;
    }

    public bool Enabled { get; set; } = true;

    public LightSourceType SourceType { get; private set; }

    private void UpdateSourceType_() {
      if (this.Position != null && this.Normal != null) {
        this.SourceType = LightSourceType.RAY;
      } else if (this.Position != null) {
        this.SourceType = LightSourceType.POSITION;
      } else if (this.Normal != null) {
        this.SourceType = LightSourceType.LINE;
      } else {
        this.SourceType = LightSourceType.UNDEFINED;
      }
    }

    public IReadOnlyXyz? Position { get; private set; }

    public ILight SetPosition(Vector3 position)
      => this.SetPosition(new Vector3f {
          X = position.X, Y = position.Y, Z = position.Z
      });

    public ILight SetPosition(IReadOnlyXyz position) {
      this.Position = position;
      this.UpdateSourceType_();
      return this;
    }

    public IReadOnlyXyz? Normal { get; private set; }

    public ILight SetNormal(Vector3 normal)
      => this.SetNormal(new Vector3f {
          X = normal.X, Y = normal.Y, Z = normal.Z
      });

    public ILight SetNormal(IReadOnlyXyz normal) {
      this.Normal = normal;
      this.UpdateSourceType_();
      return this;
    }

    public float Strength { get; set; } = 1;

    public IColor Color { get; private set; } =
      FinColor.FromRgbaFloats(1, 1, 1, 1);

    public ILight SetColor(IColor color) {
      this.Color = color;
      return this;
    }


    public IReadOnlyXyz? CosineAttenuation { get; private set; }

    public ILight SetCosineAttenuation(IReadOnlyXyz cosineAttenuation) {
      this.CosineAttenuation = cosineAttenuation;
      return this;
    }

    public IReadOnlyXyz? DistanceAttenuation { get; private set; }

    public ILight SetDistanceAttenuation(
        IReadOnlyXyz distanceAttenuation) {
      this.DistanceAttenuation = distanceAttenuation;
      return this;
    }

    public AttenuationFunction AttenuationFunction { get; private set; } =
      AttenuationFunction.NONE;

    public ILight SetAttenuationFunction(
        AttenuationFunction attenuationFunction) {
      this.AttenuationFunction = attenuationFunction;
      return this;
    }

    public DiffuseFunction DiffuseFunction { get; private set; } =
      DiffuseFunction.CLAMP;

    public ILight SetDiffuseFunction(DiffuseFunction diffuseFunction) {
      this.DiffuseFunction = diffuseFunction;
      return this;
    }
  }
}