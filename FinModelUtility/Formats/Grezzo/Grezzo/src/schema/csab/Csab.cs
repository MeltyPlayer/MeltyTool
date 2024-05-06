using System.Collections.Generic;

using fin.schema;
using fin.util.asserts;

using schema.binary;
using schema.binary.attributes;

namespace grezzo.schema.csab {
  [Endianness(Endianness.LittleEndian)]
  public class Csab : IBinaryDeserializable {
    public uint Version { get; set; }
    public bool IsPastVersion4 => this.Version > 4;

    public uint Duration { get; set; }

    public Dictionary<int, AnimationNode>
        BoneIndexToAnimationNode { get; set; } = new();

    [Unknown]
    public void Read(IBinaryReader br) {
      var basePosition = br.Position;

      br.AssertString("csab");
      var size = br.ReadUInt32();

      // Subversion?
      this.Version = br.ReadUInt32();

      br.AssertUInt32(0x00);

      float[] unk;
      if (IsPastVersion4) {
        // TODO: Might have to do with translations?
        unk = br.ReadSingles(3);
      }

      // Num animations?
      br.AssertUInt32(0x01);
      // Location?
      var animationOffset = br.ReadUInt32();

      br.AssertUInt32(0x00);
      br.AssertUInt32(0x00);
      br.AssertUInt32(0x00);
      br.AssertUInt32(0x00);

      this.Duration = br.ReadUInt32();

      // Jasper and M-1 believe this is loop mode, where 0 is a non-looping and
      // 1 is looping. But this doesn't seem to actually correlate with the
      // animations you'd expect to be looping vs. non-looping?
      var loopMode = br.ReadUInt32();

      var anodCount = br.ReadUInt32();
      var boneCount = br.ReadUInt32();
      Asserts.True(anodCount <= boneCount);

      // Jasper: This appears to be an inverse of the bone index in each array,
      // probably for fast binding?
      var boneToAnimationTable = br.ReadInt16s(boneCount);

      // TODO(jstpierre): This doesn't seem like a Grezzo thing to do.
      br.Align(0x04);

      var animationNodes = new AnimationNode[anodCount];
      for (var i = 0; i < anodCount; ++i) {
        var anod = new AnimationNode(this);

        var offset = br.ReadUInt32();
        br.SubreadAt(basePosition + animationOffset + offset, anod.Read);

        animationNodes[i] = anod;
      }

      for (var b = 0; b < boneCount; ++b) {
        var anodIndex = boneToAnimationTable[b];
        if (anodIndex != -1) {
          var anod = animationNodes[anodIndex];
          this.BoneIndexToAnimationNode[b] = anod;
        }
      }
    }
  }
}