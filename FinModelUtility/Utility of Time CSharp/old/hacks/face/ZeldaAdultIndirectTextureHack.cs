using System.Collections.Generic;

using UoT.hacks.fields;

namespace UoT {
  public class ZeldaAdultIndirectTextureHack : IIndirectTextureHack {
    // TODO: Support different eyes/mouths depending on animation frame.
    // TODO: Where are the rest of the expressions?

    public IReadOnlyList<IField> Fields { get; } =
      new List<IField>().AsReadOnly();

    public enum Eye {
      NORMAL = 0x30C8,
    }

    public EyeState EyeState { get => default; set { } }
    public MouthState MouthState { get => default; set { } }


    public uint MapTextureAddress(uint originalAddress) {
      // Left eye
      if (originalAddress == 0x09000000) {
        return 0x06000000 + (uint)Eye.NORMAL;
      }

      // Right eye
      if (originalAddress == 0x08000000) {
        return 0x06000000 + (uint)Eye.NORMAL;
      }

      // Mouth
      if (originalAddress == 0x0A000000) {
        return (uint)(0x06000000 + 0);
      }

      return originalAddress;
    }
  }
}
