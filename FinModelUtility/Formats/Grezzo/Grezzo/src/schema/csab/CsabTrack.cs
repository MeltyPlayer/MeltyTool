using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using schema.binary;

namespace grezzo.schema.csab;

public enum TrackType {
  POSITION,
  SCALE,
  ROTATION
}

public class CsabTrack : IBinaryDeserializable {
  private readonly Csab parent_;
  private Func<IBinaryReader, int> readRawLinearFloat_;
  private Func<IBinaryReader, int> readRawLinearShort_;

  public CsabTrack(Csab parent,
                   TrackType valueType) {
      this.parent_ = parent;
      this.ValueType = valueType;

      // TODO: Is this right????
      this.readRawLinearFloat_ = this.ValueType switch {
          // Weird that this is different, but this really does seem to be right.
          TrackType.POSITION => br => br.ReadUInt16() / 2,
          TrackType.SCALE    => br => br.ReadInt16(),
          TrackType.ROTATION => br => br.ReadInt16(),
      };
      // TODO: Is this right????
      this.readRawLinearShort_ = this.ValueType switch {
          // Weird that this is different, but this really does seem to be right.
          TrackType.POSITION => br => br.ReadUInt16(),
          TrackType.SCALE    => br => br.ReadInt16(),
          TrackType.ROTATION => br => br.ReadInt16(),
      };
    }

  public TrackType ValueType { get; }

  public AnimationTrackType Type { get; set; }

  public IList<CsabKeyframe> Keyframes { get; set; } =
    new List<CsabKeyframe>();

  public uint Duration { get; set; }

  public bool AreRotationsShort { get; set; }

  public bool IsPastVersion4 => this.parent_.IsPastVersion4;

  public void Read(IBinaryReader br) {
      var startFrame = 0;
      if (this.IsPastVersion4) {
        var isConstant = br.ReadByte() != 0;
        this.Type = (AnimationTrackType) br.ReadByte();
        this.Keyframes = new CsabKeyframe[br.ReadUInt16()];

        if (isConstant) {
          for (var i = 0; i < this.Keyframes.Count; ++i) {
            var scale = br.ReadSingle();
            var bias = br.ReadSingle();
            var value = br.ReadUInt32();

            this.Keyframes[i] = new CsabKeyframe {
                Time = (uint) i, Value = this.ApplyScaleAndBias_(value, scale, bias),
            };
          }

          return;
        }
      } else {
        this.Type = (AnimationTrackType) br.ReadUInt32();
        this.Keyframes = new CsabKeyframe[br.ReadUInt32()];
        startFrame = br.ReadInt32();
        this.Duration = br.ReadUInt32();
      }

      float trackScale = -1;
      float trackBias = -1;
      if (this.IsPastVersion4 && this.Type == AnimationTrackType.LINEAR) {
        trackScale = br.ReadSingle();
        trackBias = br.ReadSingle();
      }

      for (var i = 0; i < this.Keyframes.Count; ++i) {
        this.Keyframes[i] = this.Type switch {
            AnimationTrackType.LINEAR when !this.AreRotationsShort
                => this.ReadKeyframeLinearFloat_(
                    br,
                    trackScale,
                    trackBias,
                    startFrame,
                    i),
            AnimationTrackType.LINEAR when this.AreRotationsShort
                => this.ReadKeyframeLinearShort_(
                    br,
                    trackScale,
                    trackBias,
                    startFrame,
                    i),
            AnimationTrackType.HERMITE when !this.AreRotationsShort
                => this.ReadKeyframeHermiteFloat_(br, startFrame),
            AnimationTrackType.HERMITE when this.AreRotationsShort
                => this.ReadKeyframeHermiteShort_(br, startFrame),
        };
      }

      br.Align(4);
    }

  private CsabKeyframe ReadKeyframeLinearFloat_(
      IBinaryReader br,
      float trackScale,
      float trackBias,
      int startFrame,
      int index) {
      if (this.IsPastVersion4) {
        var raw = this.readRawLinearFloat_(br);
        var value = this.ApplyScaleAndBias_(raw, trackScale, trackBias);
        return new CsabKeyframe {
            Time = (uint) (startFrame + index), Value = value,
        };
      }

      return new CsabKeyframe {
          Time = (uint) (startFrame + br.ReadUInt32()), Value = br.ReadSingle(),
      };
    }

  private CsabKeyframe ReadKeyframeLinearShort_(
      IBinaryReader br,
      float trackScale,
      float trackBias,
      int startFrame,
      int index) {
      if (this.IsPastVersion4) {
        var raw = this.readRawLinearShort_(br);
        var value = this.ApplyScaleAndBias_(raw, trackScale, trackBias);
        return new CsabKeyframe {
            Time = (uint) (startFrame + index), Value = value,
        };
      }

      return new CsabKeyframe {
          Time = (uint) (startFrame + br.ReadUInt16()),
          Value = br.ReadSn16() * MathF.PI,
      };
    }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private float ApplyScaleAndBias_(float value, float scale, float bias)
    => value * scale - bias;

  private CsabKeyframe ReadKeyframeHermiteFloat_(IBinaryReader br,
                                                 int startFrame)
    => new CsabKeyframe {
        Time = (uint) (startFrame + br.ReadUInt32()),
        Value = br.ReadSingle(),
        IncomingTangent = br.ReadSingle(),
        OutgoingTangent = br.ReadSingle(),
    };

  private CsabKeyframe ReadKeyframeHermiteShort_(IBinaryReader br,
                                                 int startFrame)
    => new CsabKeyframe {
        Time = (uint) (startFrame + br.ReadUInt16()),
        Value = br.ReadSn16() * MathF.PI,
        IncomingTangent = br.ReadSn16(),
        OutgoingTangent = br.ReadSn16(),
    };
}