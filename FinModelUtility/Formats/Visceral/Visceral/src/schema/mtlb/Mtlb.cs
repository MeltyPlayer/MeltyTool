using fin.schema.vector;

using schema.binary;

namespace visceral.schema.mtlb {
  public class Mtlb : IBinaryDeserializable {
    public string Name { get; private set; }

    public IReadOnlyList<MtlbChannel> HighLodMaterialChannels {
      get;
      private set;
    }

    public IReadOnlyList<MtlbChannel> LowLodMaterialChannels {
      get;
      private set;
    }

    public void Read(IBinaryReader br) {
      br.Position = 0x10;
      var stringsLength = br.ReadUInt32();

      br.Position = 0x40;

      // TODO: These might be wrong
      var highLodChannelCount = br.ReadUInt16();
      var lowLodChannelCount = br.ReadUInt16();

      var valuesOffset = br.ReadUInt32();
      var stringsOffset = br.ReadUInt32();

      this.Name = br.SubreadAt(stringsOffset + stringsLength,
                               sbr => sbr.ReadStringNT());

      br.Position = 0x50;

      var readNewChannel =
          () => {
            var id = br.ReadUInt32();
            var category = (MtlbChannelCategory) br.ReadUInt32();

            var typeString = br.SubreadAt(stringsOffset + br.ReadUInt32(),
                                          sbr => sbr.ReadStringNT());
            var type = typeString.ToMtlbChannelType();

            var unk0 = br.ReadUInt32();
            var unk1 = br.ReadUInt32();

            var valueOffset = valuesOffset + br.ReadUInt32();
            Vector4f? colorValues = null;
            Vector2i? idValues = null;
            if (type.IsSampler()) {
              idValues
                  = br.SubreadAt(valueOffset, sbr => sbr.ReadNew<Vector2i>());
            } else {
              colorValues
                  = br.SubreadAt(valueOffset, sbr => sbr.ReadNew<Vector4f>());
            }

            var path = br.SubreadAt(stringsOffset + br.ReadUInt32(),
                                    sbr => sbr.ReadStringNT());

            return new MtlbChannel {
                MtlbChannelCategory = category,
                Type = type,
                ColorValues = colorValues,
                IdValues = idValues,
                Path = path,
            };
          };

      var highLodMaterialChannels
          = new MtlbChannel[highLodChannelCount];
      this.HighLodMaterialChannels = highLodMaterialChannels;
      for (var i = 0; i < highLodChannelCount; i++) {
        highLodMaterialChannels[i] = readNewChannel();
      }

      var lowLodMaterialChannels = new MtlbChannel[lowLodChannelCount];
      this.LowLodMaterialChannels = lowLodMaterialChannels;
      for (var i = 0; i < lowLodChannelCount; i++) {
        lowLodMaterialChannels[i] = readNewChannel();
      }
    }
  }

  public enum MtlbChannelCategory {
    Sampler = 5,
  }

  public enum MtlbChannelType {
    NotSupported,

    // Samplers
    OcclusionTexSampler,
    colorTexSampler,
    normalSampler,
    SelfIllumTexSampler,
    SpecEnvMapSampler,
    SpecularTexSampler,

    // Values
    AmbLightAmbOcclIntensityFacingRatio,
    bumpDiffLgtSpecModShinnyness,
    g_blinkParams,
    g_materialNormalMapScale,
    g_skinPSParams,
    Shinnyness,
  }

  internal static class MtlbChannelTypeExtensions {
    public static MtlbChannelType ToMtlbChannelType(this string typeText)
      => typeText switch {
          "AmbLightAmbOcclIntensityFacingRatio" => MtlbChannelType
              .AmbLightAmbOcclIntensityFacingRatio,
          "bumpDiffLgtSpecModShinnyness" => MtlbChannelType
              .bumpDiffLgtSpecModShinnyness,
          "colorTexSampler" => MtlbChannelType.colorTexSampler,
          "g_blinkParams"   => MtlbChannelType.g_blinkParams,
          "g_materialNormalMapScale" => MtlbChannelType
              .g_materialNormalMapScale,
          "g_skinPSParams"      => MtlbChannelType.g_skinPSParams,
          "normalSampler"       => MtlbChannelType.normalSampler,
          "OcclusionTexSampler" => MtlbChannelType.OcclusionTexSampler,
          "SelfIllumTexSampler" => MtlbChannelType.SelfIllumTexSampler,
          "Shinnyness"          => MtlbChannelType.Shinnyness,
          "SpecEnvMapSampler"   => MtlbChannelType.SpecEnvMapSampler,
          "SpecularTexSampler"  => MtlbChannelType.SpecularTexSampler,
          _ => MtlbChannelType.NotSupported
      };

    public static bool IsSampler(this MtlbChannelType type)
      => type is MtlbChannelType.colorTexSampler
                 or MtlbChannelType.normalSampler
                 or MtlbChannelType.OcclusionTexSampler
                 or MtlbChannelType.SelfIllumTexSampler
                 or MtlbChannelType.SpecularTexSampler;
  }

  public class MtlbChannel {
    public MtlbChannelCategory MtlbChannelCategory { get; set; }
    public MtlbChannelType Type { get; set; }
    public Vector4f? ColorValues { get; set; }
    public Vector2i? IdValues { get; set; }
    public string Path { get; set; }
  }
}