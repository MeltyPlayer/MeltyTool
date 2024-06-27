namespace modl.level;

public class ImpactBase : BLevelObject {
  public SoundBase SoundBase { get; set; }
  public ExplodeBase? ExplosionType { get; set; }
  public TequilaEffectResource? TequilaEffect { get; set; }
}