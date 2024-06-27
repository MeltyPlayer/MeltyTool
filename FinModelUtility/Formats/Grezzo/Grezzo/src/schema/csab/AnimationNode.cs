using System.Collections.Generic;
using System.Linq;

using schema.binary;

namespace grezzo.schema.csab;

public class AnimationNode : IBinaryDeserializable {
  private readonly Csab parent_;

  public AnimationNode(Csab parent) {
      this.parent_ = parent;

      this.TranslationAxes =
          Enumerable.Range(0, 3)
                    .Select(_ => new CsabTrack(parent, TrackType.POSITION))
                    .ToArray();
      this.RotationAxes =
          Enumerable.Range(0, 3)
                    .Select(_ => new CsabTrack(parent, TrackType.ROTATION))
                    .ToArray();
      this.ScaleAxes =
          Enumerable.Range(0, 3)
                    .Select(_ => new CsabTrack(parent, TrackType.SCALE))
                    .ToArray();
    }

  public ushort BoneIndex { get; set; }

  public IReadOnlyList<CsabTrack> TranslationAxes { get; }
  public IReadOnlyList<CsabTrack> RotationAxes { get; }
  public IReadOnlyList<CsabTrack> ScaleAxes { get; }

  public bool IsPastVersion4 => this.parent_.IsPastVersion4;

  public void Read(IBinaryReader br) {
      var basePosition = br.Position;

      br.AssertString("anod");

      this.BoneIndex = br.ReadUInt16();

      var isRotationShort = br.ReadUInt16() != 0;

      foreach (var translationAxis in TranslationAxes) {
        var offset = br.ReadUInt16();
        if (offset != 0) {
          br.SubreadAt(basePosition + offset, translationAxis.Read);
        }
      }

      foreach (var rotationAxis in RotationAxes) {
        rotationAxis.AreRotationsShort = isRotationShort;

        var offset = br.ReadUInt16();
        if (offset != 0) {
          br.SubreadAt(basePosition + offset, rotationAxis.Read);
        }
      }

      foreach (var scaleAxis in ScaleAxes) {
        var offset = br.ReadUInt16();
        if (offset != 0) {
          br.SubreadAt(basePosition + offset, scaleAxis.Read);
        }
      }

      br.AssertUInt16(0x00);
    }
}