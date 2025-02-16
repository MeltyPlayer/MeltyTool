using System.Numerics;


namespace vrml.schema;

public record AudioClipNode : BNode {
  public bool Loop { get; init; }
  public float Pitch { get; init; }
  public float StartTime { get; init; }
  public string Url { get; init; }
}

public record SoundNode : BNode {
  public float Intensity { get; init; }
  public Vector3 Location { get; init; }
  public float MaxBack { get; init; }
  public float MaxFront { get; init; }
  public float MinBack { get; init; }
  public float MinFront { get; init; }
  public AudioClipNode Source { get; init; }
}