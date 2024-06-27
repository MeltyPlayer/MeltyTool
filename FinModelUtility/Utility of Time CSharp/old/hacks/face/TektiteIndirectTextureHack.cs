using System.Collections.Generic;

using UoT.hacks.fields;

namespace UoT {
  /// <summary>
  ///   Indirect textures for Tektites. This is needed for this object because
  ///   it chooses between two sets of textures depending on the color.
  ///
  ///   These addresses were found by searching the ROM for specific color
  ///   values.
  /// </summary>
  public sealed class TektiteIndirectTextureHack : BIndirectTextureHack {

    public TektiteIndirectTextureHack() {
      var fields = new List<IField>();
      fields.Add(this.color_);

      this.Fields = fields.AsReadOnly();
    }

    public override IReadOnlyList<IField> Fields { get; }

    private readonly IDiscreteField<ushort> color_ =
        new DiscreteField<ushort>.Builder("Color")
            .AddPossibleValue("Red", 0x1B00)
            .AddPossibleValue("Blue", 0x1300)
            .Build();

    public override uint MapTextureAddress(uint originalAddress) {
      var baseOffset = this.color_.Value;

      var topOffset = baseOffset;
      var eyeOffset = baseOffset + (0x1F20 - 0x1B00);
      var bottomOffset = baseOffset + (0x2100 - 0x1B00);

      // Top
      if (originalAddress == 0x08000000) {
        return 0x06000000 + (uint) topOffset;
      }

      // Eye
      if (originalAddress == 0x09000000) {
        return 0x06000000 + (uint) eyeOffset;
      }

      // Bottom
      if (originalAddress == 0x0A000000) {
        return 0x06000000 + (uint) bottomOffset;
      }

      return originalAddress;
    }
  }
}