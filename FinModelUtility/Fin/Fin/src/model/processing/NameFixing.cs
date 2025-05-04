using System.Collections.Generic;

using fin.data.counters;

namespace fin.model.processing;

public static class NameFixing {
  public static void FixNames(IModel model) {
    FixNamedInstances_(model.AnimationManager.Animations, "animation");
    FixNamedInstances_(model.Skeleton.Bones, "bone");
    FixNamedInstances_(model.MaterialManager.All, "material");
    FixNamedInstances_(model.Skin.Meshes, "mesh");
    FixNamedInstances_(model.AnimationManager.MorphTargets, "morphTarget");
    // TODO: Split this out into textures and images, so duplicate images are
    // not exported.
    //FixNamedInstances_(model.MaterialManager.Textures, "texture");
  }

  private static void FixNamedInstances_<T>(IReadOnlyList<T> instances,
                                              string baseName)
      where T : INamed {
    var nameCounts = new CounterSet<string>();
    for (var i = 0; i < instances.Count; ++i) {
      var instance = instances[i];

      var name = instance.Name;
      if (name == null) {
        instance.Name = $"{baseName}{i}";
        continue;
      }

      var nameCount = nameCounts.Increment(name);
      if (nameCount > 1) {
        instance.Name = $"{name} ({nameCount})";
      }
    }
  }
}