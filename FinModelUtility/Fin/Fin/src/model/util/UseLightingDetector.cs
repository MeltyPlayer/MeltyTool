using System.Collections.Generic;
using System.Linq;

namespace fin.model.util;

public class UseLightingDetector {
  public bool ShouldUseLightingFor(IReadOnlyModel model) {
    foreach (var vertex in model.Skin.Vertices) {
      if (vertex is IReadOnlyNormalVertex { LocalNormal: { } }) {
        return true;
      }
    }

    var lightInputsList = new LinkedList<FixedFunctionSource>();
    lightInputsList.AddLast(FixedFunctionSource.LIGHT_AMBIENT_COLOR);
    lightInputsList.AddLast(FixedFunctionSource.LIGHT_AMBIENT_ALPHA);
    for (var i = 0; i < MaterialConstants.MAX_LIGHTS; ++i) {
      lightInputsList.AddLast(
          FixedFunctionSource.LIGHT_DIFFUSE_COLOR_0 + i);
      lightInputsList.AddLast(
          FixedFunctionSource.LIGHT_DIFFUSE_ALPHA_0 + i);
      lightInputsList.AddLast(
          FixedFunctionSource.LIGHT_SPECULAR_COLOR_0 + i);
      lightInputsList.AddLast(
          FixedFunctionSource.LIGHT_SPECULAR_ALPHA_0 + i);
    }

    var lightInputs = lightInputsList.ToArray();
    foreach (var material in model.MaterialManager.All) {
      if (material is IFixedFunctionMaterial fixedFunctionMaterial) {
        if (fixedFunctionMaterial.Equations
                                 .DoOutputsDependOn(lightInputs)) {
          return true;
        }
      }
    }

    return false;
  }
}