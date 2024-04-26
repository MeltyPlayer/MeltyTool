using fin.language.equations.fixedFunction;
using fin.model;

namespace fin.ui.rendering.gl.model {
  public class UseLightingDetector {
    public bool ShouldUseLightingFor(IReadOnlyModel model) {
      var useLighting = false;
      foreach (var vertex in model.Skin.Vertices) {
        if (vertex is IReadOnlyNormalVertex { LocalNormal: { } }) {
          useLighting = true;
          break;
        }
      }

      if (!useLighting) {
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
              useLighting = true;
              break;
            }
          }
        }
      }

      return useLighting;
    }
  }
}