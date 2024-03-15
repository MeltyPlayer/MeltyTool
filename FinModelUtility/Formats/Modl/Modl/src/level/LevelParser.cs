using fin.io;

using modl.api;
using modl.schema.xml;
using modl.xml.level;

namespace modl.level {
  public class LevelParser {
    public IReadOnlyList<IBwObject> Parse(
        IReadOnlyGenericFile levelXmlFile,
        GameVersion gameVersion)
      => new XmlLevelParser()
         .Parse(levelXmlFile, gameVersion)
         .Instances
         .Select(this.ParseObject_)
         .ToArray();

    private IBwObject ParseObject_(XmlLevelObject xmlObject)
      => xmlObject.Type switch {
          /*"cAdvancedWeaponBase" => new AdvancedWeaponBase(xmlObject),
          "cAirVehicle"         => new AirVehicle(xmlObject),
          "cAirVehicleBase"     => new AirVehicleBase(xmlObject),
          "cAirVehicleEngineSoundBase"
              => new AirVehicleEngineSoundBase(xmlObject),
          "cAirVehiclePhysicsBase" => new AirVehiclePhysicsBase(xmlObject),
          "cAirVehicleSoundBase"   => new AirVehicleSoundBase(xmlObject),
          "cAnimationResource"     => new AnimationResource(xmlObject),
          "cAnimationTriggeredEffect"
              => new AnimationTriggeredEffect(xmlObject),
          "cAnimationTriggeredEffectChainItemGroundImpact"
              => new AnimationTriggeredEffectChainItemGroundImpact(xmlObject),
          "cAnimationTriggeredEffectChainItemSound"
              => new AnimationTriggeredEffectChainItemSound(xmlObject),
          "cAttackSustainReleaseEnvelope"
              => new AttackSustainReleaseEnvelope(xmlObject),
          "cBuilding"           => new Building(xmlObject),
          "cBuildingImpBase"    => new BuildingImpBase(xmlObject),
          "sDestroyBase"        => new DestroyBase(xmlObject),
          "cDestroyableObject"  => new DestroyableObject(xmlObject),
          "sExplodeBase"        => new ExplodeBase(xmlObject),
          "cGameScriptResource" => new GameScriptResource(xmlObject),
          "cGlobalScriptEntity" => new GlobalScriptEntity(xmlObject),
          "cGroundVehicle"      => new GroundVehicle(xmlObject),
          "cGroundVehicleBase"  => new GroundVehicleBase(xmlObject),
          "cGroundVehicleEngineSoundBase"
              => new GroundVehicleEngineSoundBase(xmlObject),
          "cGroundVehiclePhysicsBase"
              => new GroundVehiclePhysicsBase(xmlObject),
          "cGroundVehicleSoundBase"
              => new GroundVehicleSoundBase(xmlObject),
          "cHUD"             => new Hud(xmlObject),
          "cHUDSoundBlock"   => new HudSoundBlock(xmlObject),
          "cImpactBase"      => new ImpactBase(xmlObject),
          "cImpactTableBase" => new ImpactTableBase(xmlObject),
          "cImpactTableTaggedEffectBase"
              => new ImpactTableTaggedEffectBase(xmlObject),
          "cInitialisationScriptEntity"
              => new InitialisationScriptEntity(xmlObject),
          "cMapZone"                => new MapZone(xmlObject),
          "cNodeHierarchyResource"  => new NodeHierarchyResource(xmlObject),
          "cObjective"              => new Objective(xmlObject),
          "cObjectiveMarker"        => new ObjectiveMarker(xmlObject),
          "cObjectiveMarkerBase"    => new ObjectiveMarkerBase(xmlObject),
          "cPhase"                  => new Phase(xmlObject),
          "cPickupReflected"        => new PickupReflected(xmlObject),
          "sProjectileBase"         => new ProjectileBase(xmlObject),
          "cProjectileSoundBase"    => new ProjectileSoundBase(xmlObject),
          "cReflectedPhysicsParams" => new ReflectedPhysicsParams(xmlObject),
          "cReflectedUnitGroup"     => new ReflectedUnitGroup(xmlObject),
          "cRenderParams"           => new RenderParams(xmlObject),
          "sSampleResource"         => new SampleResource(xmlObject),
          "cSceneryCluster"         => new SceneryCluster(xmlObject),
          "sSceneryClusterBase"     => new SceneryClusterBase(xmlObject),
          "cScriptSprite"           => new ScriptSprite(xmlObject),
          "cSeatBase"               => new SeatBase(xmlObject),
          "cSimpleTequilaTaggedEffectBase"
              => new SimpleTequilaTaggedEffectBase(xmlObject),
          "cSoundBase"             => new SoundBase(xmlObject),
          "cSoundCurve"            => new SoundCurve(xmlObject),
          "cSprite"                => new Sprite(xmlObject),
          "sSpriteBasetype"        => new SpriteBaseType(xmlObject),
          "cTequilaEffectResource" => new TequilaEffectResource(xmlObject),
          "cTerrainParticleGeneratorBase"
              => new TerrainParticleGeneratorBase(xmlObject),
          "cTextureResource"       => new TextureResource(xmlObject),
          "cTroop"                 => new Troop(xmlObject),
          "cTroopAnimationSet"     => new TroopAnimationSet(xmlObject),
          "sTroopBase"             => new TroopBase(xmlObject),
          "sTroopBase"             => new TroopBase(xmlObject),
          "cTroopVoiceMessageBase" => new TroopVoiceMessageBase(xmlObject),
          "cWaypoint"              => new Waypoint(xmlObject),
          "sWeaponBase"            => new WeaponBase(xmlObject),
          "cWeaponSoundBase"       => new WeaponSoundBase(xmlObject),*/
      };
  }
}