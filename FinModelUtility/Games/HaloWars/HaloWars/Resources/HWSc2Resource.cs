namespace HaloWarsTools {
  public class HWSc2Resource : HWScnResource {
    public static new HWSc2Resource FromFile(HWContext context, string filename) {
      return GetOrCreateFromFile(context, filename, HWResourceType.Sc2) as HWSc2Resource;
    }
  }
}