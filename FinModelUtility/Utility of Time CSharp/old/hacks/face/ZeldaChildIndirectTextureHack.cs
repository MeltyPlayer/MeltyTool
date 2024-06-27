using System.Collections.Generic;

using UoT.hacks;
using UoT.hacks.fields;

namespace UoT {
  public class ZeldaChildIndirectTextureHack : IIndirectTextureHack {
    // TODO: Support different eyes/mouths depending on animation frame.
    // TODO: Where are the rest of the expressions?
    // TODO: Where are happy eyes/mouth?

    // TODO: Map animation # to eye/mouth states

    public IReadOnlyList<IField> Fields { get; } =
      new List<IField>().AsReadOnly();

    public enum Eye {
      OPEN = 0x7208,
      LOOKING_OUT = 0x7208 + 0x1640,
      LOOKING_IN = 0x7208 + 0x1a40,
      MISCHIEVOUS = 0x7208 + 0x2240,
      HALF_OPEN = 0x7208 + 0x2640,
      CLOSED = 0x7208 + 0x2a40,
      SAD = 0x7208 + 0x2e40,
    }

    public enum Mouth {
      CLOSED = 0x7208 + 0x400,
      SAD = 0x7208 + 0x1e40,
      OPEN = 0x7208 + 0x3240,
    }

    public EyeState EyeState { get => default; set { } }
    public MouthState MouthState { get => default; set { } }


    public uint MapTextureAddress(uint originalAddress) {
       var eyeOffset = (uint) BlinkUtil.Get(Eye.OPEN, Eye.HALF_OPEN, Eye.CLOSED);
      
      // Left eye
      if (originalAddress == 0x09000000) {
        return 0x06000000 + (uint)eyeOffset;
      }
      
      // Right eye
      if (originalAddress == 0x08000000) {
        return 0x06000000 + (uint)eyeOffset;
      }

      // Mouth
      if (originalAddress == 0x0A000000) {
        return 0x06000000 + (uint) Mouth.CLOSED;
      }

      return originalAddress;
    }
  }
}
