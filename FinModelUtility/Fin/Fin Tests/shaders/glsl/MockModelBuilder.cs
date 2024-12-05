using System;
using System.Drawing;

using fin.model;
using fin.model.impl;
using fin.model.util;
using fin.util.image;

namespace fin.shaders.glsl;

public struct MockMaterialOptions {
  public bool Masked { get; init; }
  public bool WithNormals { get; init; }
  public bool WithColors { get; init; }
  public bool WithUvs { get; init; }
}

public static class MockMaterial {
  public static IShaderSourceGlsl BuildAndGetSource(
      MockMaterialOptions options,
      Func<IMaterialManager, IMaterial> buildMaterial) {
    var model = ModelImpl.CreateForViewer();

    var material = buildMaterial(model.MaterialManager);
    material.TransparencyType = options.Masked
        ? TransparencyType.MASK
        : TransparencyType.TRANSPARENT;

    var skin = model.Skin;
    var v = skin.AddVertex(0, 0, 0);

    if (options.WithNormals) {
      v.SetLocalNormal(0, 0, 1);
    }

    if (options.WithColors) {
      v.SetColor(Color.Red);
    }

    if (options.WithUvs) {
      v.SetUv(0, 1);
    }

    skin.AddMesh().AddPoints(v).SetMaterial(material);

    return material.ToShaderSource(model);
  }
}