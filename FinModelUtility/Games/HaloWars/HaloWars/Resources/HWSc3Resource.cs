namespace HaloWarsTools {
  public class HWSc3Resource : HWScnResource {
    public static new HWSc3Resource FromFile(HWContext context, string filename) {
      return GetOrCreateFromFile(context, filename, HWResourceType.Sc3) as HWSc3Resource;
    }
  }
}