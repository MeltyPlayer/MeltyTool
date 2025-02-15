using System.Numerics;

using schema.text.reader;

using vrml.schema;


namespace vrml.api;

public partial class VrmlParser {
  private static OrientationInterpolatorNode ReadOrientationInterpolatorNode_(
      ITextReader tr) {
    IReadOnlyList<float> key = null!;
    IReadOnlyList<Quaternion> keyValue = null!;

    ReadFields_(
        tr,
        fieldName => {
          switch (fieldName) {
            case "key": {
              key = ReadSingleArray_(tr);
              break;
            }
            case "keyValue": {
              keyValue = ReadQuaternionArray_(tr);
              break;
            }
            default: throw new NotImplementedException();
          }
        });

    return new OrientationInterpolatorNode {
        Keyframes = key.Zip(keyValue).ToArray(),
    };
  }

  private static PositionInterpolatorNode ReadPositionInterpolatorNode_(ITextReader tr) {
    IReadOnlyList<float> key = null!;
    IReadOnlyList<Vector3> keyValue = null!;

    ReadFields_(
        tr,
        fieldName => {
          switch (fieldName) {
            case "key": {
              key = ReadSingleArray_(tr);
              break;
            }
            case "keyValue": {
              keyValue = ReadVector3Array_(tr);
              break;
            }
            default: throw new NotImplementedException();
          }
        });

    return new PositionInterpolatorNode {
        Keyframes = key.Zip(keyValue).ToArray(),
    };
  }

  private static TimeSensorNode ReadTimeSensorNode_(ITextReader tr) {
    float cycleInterval = default;
    bool loop = false;
    float startTime = default;

    ReadFields_(
        tr,
        fieldName => {
          switch (fieldName) {
            case "cycleInterval": {
              cycleInterval = tr.ReadSingle();
              break;
            }
            case "loop": {
              loop = ReadBool_(tr);
              break;
            }
            case "startTime": {
              startTime = tr.ReadSingle();
              break;
            }
            default: throw new NotImplementedException();
          }
        });

    return new TimeSensorNode {
        CycleInterval = cycleInterval,
    };
  }
}