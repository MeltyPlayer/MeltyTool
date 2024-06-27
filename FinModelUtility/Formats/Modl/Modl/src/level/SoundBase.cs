namespace modl.level;

public class SoundBase : BLevelObject {
  public float Priority { get; set; }
  public float Volume { get; set; }
  public float VolumeVariation { get; set; }
  public float PitchMultiplier { get; set; }
  public float PitchMultiplierVariation { get; set; }
  public float FalloffMin { get; set; }
  public float FalloffMax { get; set; }
  public uint Flags { get; set; }
  public SampleResource?[] Sample { get; set; }
  public float[] Likelyhood { get; set; }
  public uint CacheSize { get; set; }
}