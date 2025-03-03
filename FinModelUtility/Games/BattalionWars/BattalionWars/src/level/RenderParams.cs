using fin.color;

namespace modl.level;

public enum MaterialLightingMode {
  MODULATE
}

public class RenderParams : BLevelObject {
  public IColor FogColor { get; set; }
  public IColor SkyColor { get; set; }
  public MaterialLightingMode GlobalLightingMode { get; set; }
  public IColor SunAmbientColor { get; set; }
  public float GouraudMapAmbientScaleDark { get; set; }
  public float GouraudMapAmbientScaleBright { get; set; }
  public IColor SunDirectionalColor { get; set; }
  public IColor AntiSunDirectionalColor { get; set; }
  public float SunElevation { get; set; }
  public float SunRotation { get; set; }
  public float AntiSunElevation { get; set; }
  public float AntiSunRotation { get; set; }
  public float SunContrast { get; set; }
  public float TerrainLightScale { get; set; }
  public IColor ExplosionDirColor { get; set; }
  public IColor ExplosionAmbColor { get; set; }
  public IColor DestroyableBurntAmbColor { get; set; }
  public float ShadowElevation { get; set; }
  public float ShadowRotation { get; set; }
  public byte ShadowFadeValue { get; set; }
  public float ShadowCutOffDistance { get; set; }
  public IColor ShadowColor { get; set; }
  public float AlphaDistance { get; set; }
  public float FogDistance { get; set; }
  public float FarClipPlane { get; set; }
  public float AlphaDistanceFx { get; set; }
  public float FarClipPlaneFx { get; set; }
  public float TroopLowLodDrawDistanceBias { get; set; }
  public TextureResource? ReticuleTexture { get; set; }
  public TextureResource? ScorchTexture { get; set; }
  public bool TerrainParticlesEnabled { get; set; }
  public float TerrainParticlesViewDistance { get; set; }
  public float TerrainParticlesAlphaDistance { get; set; }
  public bool TerrainParticlesLit { get; set; }
  public TerrainParticleAnimationBase? TerrainParticleAnimationParams { get; set; }
  public NodeHierarchyResource? WorldSkydome { get; set; }
  public float SkydomeSize { get; set; }
  public bool SkydomeHeightAdjust { get; set; }
  public float SkydomeHeightHorizonPos { get; set; }
  public float SkydomeHeightFactorUp { get; set; }
  public float SkydomeHeightFactorDown { get; set; }
  public TextureResource? SphericalSkyMap { get; set; }
  public float WaterHeight { get; set; }
  public float WaterSwellFrequency { get; set; }
  public float WaterSwellAmplitude { get; set; }
  public float WaterOpacity { get; set; }
  public float WaterUvScale { get; set; }
  public float WaterBumpHeight { get; set; }
}