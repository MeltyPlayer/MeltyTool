using System.Collections.Generic;


namespace HaloWarsTools {
  public class HWObjectDefinition {
    public string Name;
    //public HWVisResource Visual;

    public static HWObjectDefinition GetOrCreateFromId(HWContext context, string id) {
      return context.ObjectDefinitions.GetValueOrDefault(id);
    }
  }
}