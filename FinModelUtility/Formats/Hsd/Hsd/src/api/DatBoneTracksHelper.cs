using fin.animation.keyframes;
using fin.model;

using sysdolphin.schema.animation;

namespace sysdolphin.api;

public static class DatBoneTracksHelper {
  public static void AddDatKeyframesToBoneTracks(
      IEnumerable<IDatKeyframes> allDatKeyframes,
      IBoneTracks boneTracks) {
    IDatKeyframes? translationsX = null;
    IDatKeyframes? translationsY = null;
    IDatKeyframes? translationsZ = null;

    IDatKeyframes? rotationsX = null;
    IDatKeyframes? rotationsY = null;
    IDatKeyframes? rotationsZ = null;

    IDatKeyframes? scalesX = null;
    IDatKeyframes? scalesY = null;
    IDatKeyframes? scalesZ = null;

    foreach (var datKeyframes in allDatKeyframes) {
      var jointTrackType = datKeyframes.JointTrackType;
      switch (jointTrackType) {
        case JointTrackType.HSD_A_J_TRAX: {
          translationsX = datKeyframes;
          break;
        }
        case JointTrackType.HSD_A_J_TRAY: {
          translationsY = datKeyframes;
          break;
        }
        case JointTrackType.HSD_A_J_TRAZ: {
          translationsZ = datKeyframes;
          break;
        }
        case JointTrackType.HSD_A_J_ROTX: {
          rotationsX = datKeyframes;
          break;
        }
        case JointTrackType.HSD_A_J_ROTY: {
          rotationsY = datKeyframes;
          break;
        }
        case JointTrackType.HSD_A_J_ROTZ: {
          rotationsZ = datKeyframes;
          break;
        }
        case JointTrackType.HSD_A_J_SCAX: {
          scalesX = datKeyframes;
          break;
        }
        case JointTrackType.HSD_A_J_SCAY: {
          scalesY = datKeyframes;
          break;
        }
        case JointTrackType.HSD_A_J_SCAZ: {
          scalesZ = datKeyframes;
          break;
        }
      }
    }

    var translationTrack
        = boneTracks.UseSeparateTranslationKeyframesWithTangents(
            translationsX?.Keyframes.Count ?? 0,
            translationsY?.Keyframes.Count ?? 0,
            translationsZ?.Keyframes.Count ?? 0);
    Span<IDatKeyframes?> translationAxes =
        [translationsX, translationsY, translationsZ];
    for (var i = 0; i < translationAxes.Length; ++i) {
      var translationAxis = translationAxes[i];
      if (translationAxis == null) {
        continue;
      }

      foreach (var keyframe in translationAxis.Keyframes) {
        var frame = keyframe.Frame;
        var incomingValue = keyframe.IncomingValue;
        var outgoingValue = keyframe.OutgoingValue;
        var incomingTangent = keyframe.IncomingTangent;
        var outgoingTangent = keyframe.OutgoingTangent;

        translationTrack.Axes[i]
                        .Add(new KeyframeWithTangents<float>(frame,
                               incomingValue,
                               outgoingValue,
                               incomingTangent,
                               outgoingTangent));
      }
    }

    var rotationTrack
        = boneTracks.UseSeparateEulerRadiansKeyframesWithTangents(
            rotationsX?.Keyframes.Count ?? 0,
            rotationsY?.Keyframes.Count ?? 0,
            rotationsZ?.Keyframes.Count ?? 0);
    Span<IDatKeyframes?> rotationAxes = [rotationsX, rotationsY, rotationsZ];
    for (var i = 0; i < rotationAxes.Length; ++i) {
      var rotationAxis = rotationAxes[i];
      if (rotationAxis == null) {
        continue;
      }

      foreach (var keyframe in rotationAxis.Keyframes) {
        var frame = keyframe.Frame;
        var incomingValue = keyframe.IncomingValue;
        var outgoingValue = keyframe.OutgoingValue;
        var incomingTangent = keyframe.IncomingTangent;
        var outgoingTangent = keyframe.OutgoingTangent;

        rotationTrack.Axes[i]
                     .Add(new KeyframeWithTangents<float>(frame,
                            incomingValue,
                            outgoingValue,
                            incomingTangent,
                            outgoingTangent));
      }
    }

    var scaleTrack = boneTracks.UseSeparateScaleKeyframesWithTangents(
        scalesX?.Keyframes.Count ?? 0,
        scalesY?.Keyframes.Count ?? 0,
        scalesZ?.Keyframes.Count ?? 0);
    Span<IDatKeyframes?> scaleAxes = [scalesX, scalesX, scalesZ];
    for (var i = 0; i < scaleAxes.Length; ++i) {
      var scaleAxis = scaleAxes[i];
      if (scaleAxis == null) {
        continue;
      }

      foreach (var keyframe in scaleAxis.Keyframes) {
        var frame = keyframe.Frame;
        var incomingValue = keyframe.IncomingValue;
        var outgoingValue = keyframe.OutgoingValue;
        var incomingTangent = keyframe.IncomingTangent;
        var outgoingTangent = keyframe.OutgoingTangent;

        scaleTrack.Axes[i]
                  .Add(new KeyframeWithTangents<float>(frame,
                         incomingValue,
                         outgoingValue,
                         incomingTangent,
                         outgoingTangent));
      }
    }
  }
}