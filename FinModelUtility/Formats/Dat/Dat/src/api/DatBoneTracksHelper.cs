using dat.schema.animation;

using fin.model;

namespace dat.api;

public static class DatBoneTracksHelper {
  public static void AddDatKeyframesToBoneTracks(
      IEnumerable<IDatKeyframes> allDatKeyframes,
      IBoneTracks boneTracks) {
    var positionTrack = boneTracks.UseSeparatePositionAxesTrack();
    var rotationTrack = boneTracks.UseEulerRadiansRotationTrack();
    var scaleTrack = boneTracks.UseScaleTrack();

    foreach (var datKeyframes in allDatKeyframes) {
      var jointTrackType = datKeyframes.JointTrackType;
      switch (jointTrackType) {
        case JointTrackType.HSD_A_J_TRAX:
        case JointTrackType.HSD_A_J_TRAY:
        case JointTrackType.HSD_A_J_TRAZ: {
          var axis = jointTrackType - JointTrackType.HSD_A_J_TRAX;
          foreach (var keyframe in datKeyframes.Keyframes) {
            var frame = keyframe.Frame;
            var incomingValue = keyframe.IncomingValue;
            var outgoingValue = keyframe.OutgoingValue;
            var incomingTangent = keyframe.IncomingTangent;
            var outgoingTangent = keyframe.OutgoingTangent;

            positionTrack.Set(frame,
                              axis,
                              incomingValue,
                              outgoingValue,
                              incomingTangent,
                              outgoingTangent);
          }

          break;
        }
        case JointTrackType.HSD_A_J_ROTX:
        case JointTrackType.HSD_A_J_ROTY:
        case JointTrackType.HSD_A_J_ROTZ: {
          var axis = jointTrackType - JointTrackType.HSD_A_J_ROTX;
          foreach (var keyframe in datKeyframes.Keyframes) {
            var frame = keyframe.Frame;
            var incomingValue = keyframe.IncomingValue;
            var outgoingValue = keyframe.OutgoingValue;
            var incomingTangent = keyframe.IncomingTangent;
            var outgoingTangent = keyframe.OutgoingTangent;

            rotationTrack.Set(frame,
                              axis,
                              incomingValue,
                              outgoingValue,
                              incomingTangent,
                              outgoingTangent);
          }

          break;
        }
        case JointTrackType.HSD_A_J_SCAX:
        case JointTrackType.HSD_A_J_SCAY:
        case JointTrackType.HSD_A_J_SCAZ: {
          var axis = jointTrackType - JointTrackType.HSD_A_J_SCAX;
          foreach (var keyframe in datKeyframes.Keyframes) {
            var frame = keyframe.Frame;
            var incomingValue = keyframe.IncomingValue;
            var outgoingValue = keyframe.OutgoingValue;
            var incomingTangent = keyframe.IncomingTangent;
            var outgoingTangent = keyframe.OutgoingTangent;

            scaleTrack.Set(frame,
                           axis,
                           incomingValue,
                           outgoingValue,
                           incomingTangent,
                           outgoingTangent);
          }

          break;
        }
      }
    }
  }
}