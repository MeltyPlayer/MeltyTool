using System.Numerics;

using schema.text.reader;

using vrml.schema;


namespace vrml.api;

public partial class VrmlParser {
  private static AudioClipNode ReadAudioClipNode_(ITextReader tr) {
    bool loop = false;
    float pitch = 1;
    float startTime = 0;
    string url = "";

    ReadFields_(
        tr,
        fieldName => {
          switch (fieldName) {
            case "loop": {
              loop = ReadBool_(tr);
              break;
            }
            case "pitch": {
              pitch = tr.ReadSingle();
              break;
            }
            case "startTime": {
              startTime = tr.ReadSingle();
              break;
            }
            case "url": {
              url = ReadString_(tr);
              break;
            }
            default: throw new NotImplementedException();
          }
        });

    return new AudioClipNode {
        Loop = loop,
        Pitch = pitch,
        StartTime = startTime,
        Url = url,
    };
  }

  private static SoundNode ReadSoundNode_(
      ITextReader tr,
      IDictionary<string, INode> definitions) {
    float intensity = 1;
    Vector3 location = Vector3.Zero;
    float minBack = 1;
    float minFront = 1;
    float maxBack = 10;
    float maxFront = 10;
    AudioClipNode source = default;

    ReadFields_(
        tr,
        fieldName => {
          switch (fieldName) {
            case "intensity": {
              intensity = tr.ReadSingle();
              break;
            }
            case "location": {
              location = ReadVector3_(tr);
              break;
            }
            case "minBack": {
              minBack = tr.ReadSingle();
              break;
            }
            case "minFront": {
              minFront = tr.ReadSingle();
              break;
            }
            case "maxBack": {
              maxBack = tr.ReadSingle();
              break;
            }
            case "maxFront": {
              maxFront = tr.ReadSingle();
              break;
            }
            case "source": {
              source = ParseNodeOfType_<AudioClipNode>(tr, definitions);
              break;
            }
            default: throw new NotImplementedException();
          }
        });

    return new SoundNode {
        Intensity = intensity,
        Location = location,
        MinBack = minBack,
        MinFront = minFront,
        MaxBack = maxBack,
        MaxFront = maxFront,
        Source = source,
    };
  }
}