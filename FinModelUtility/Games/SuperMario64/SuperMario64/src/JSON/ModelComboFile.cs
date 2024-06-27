using System.Globalization;

using Newtonsoft.Json.Linq;

namespace sm64.JSON;

public class ModelComboFile {
  private static byte parseByte(string str) {
      bool isHex = false;
      if (str.StartsWith("0x")) {
        str = str.Substring(2);
        isHex = true;
      } else if (str.StartsWith("$")) {
        str = str.Substring(1);
        isHex = true;
      }
      if (!isHex)
        return byte.Parse(str);
      else
        return byte.Parse(str, NumberStyles.HexNumber);
    }

  private static uint parseUInt(string str) {
      bool isHex = false;
      if (str.StartsWith("0x")) {
        str = str.Substring(2);
        isHex = true;
      } else if (str.StartsWith("$")) {
        str = str.Substring(1);
        isHex = true;
      }
      if (!isHex)
        return uint.Parse(str);
      else
        return uint.Parse(str, NumberStyles.HexNumber);
    }

  public static void writeObjectCombosFile(string filename) {
      Globals.objectComboEntries.Sort((x, y) => string.Compare(x.Name, y.Name));

      JArray array = [];
      foreach (ObjectComboEntry oce in Globals.objectComboEntries) {
        JObject entry = new JObject();
        entry["Name"] = oce.Name;
        entry["ModelID"] = "0x" + oce.ModelID.ToString("X2");
        entry["ModelAddress"] = "0x" + oce.ModelSegmentAddress.ToString("X8");
        entry["Behavior"] = "0x" + oce.Behavior.ToString("X8");
        if (oce.BP1_NAME != null)
          entry["BP1_NAME"] = oce.BP1_NAME;
        if (oce.BP2_NAME != null)
          entry["BP2_NAME"] = oce.BP2_NAME;
        if (oce.BP3_NAME != null)
          entry["BP3_NAME"] = oce.BP3_NAME;
        if (oce.BP4_NAME != null)
          entry["BP4_NAME"] = oce.BP4_NAME;
        if (oce.BP1_DESCRIPTION != null)
          entry["BP1_DESCRIPTION"] = oce.BP1_DESCRIPTION;
        if (oce.BP2_DESCRIPTION != null)
          entry["BP2_DESCRIPTION"] = oce.BP2_DESCRIPTION;
        if (oce.BP3_DESCRIPTION != null)
          entry["BP3_DESCRIPTION"] = oce.BP3_DESCRIPTION;
        if (oce.BP4_DESCRIPTION != null)
          entry["BP4_DESCRIPTION"] = oce.BP4_DESCRIPTION;
        array.Add(entry);
      }

      JObject o = new JObject();
      o["ObjectCombos"] = array;

      File.WriteAllText(filename, o.ToString());
    }

  private static bool checkValidEntry(JObject entry) {
      return (entry["Name"] != null && entry["ModelID"] != null &&
              entry["ModelAddress"] != null && entry["Behavior"] != null);
    }

  public static void parseObjectCombos(string filename) {
      if (File.Exists(filename)) {
        string json = File.ReadAllText(filename);
        JObject o = JObject.Parse(json);
        if (o["ObjectCombos"] != null) {
          JArray array = (JArray) o["ObjectCombos"];
          foreach (JToken token in array.Children()) {
            JObject entry = (JObject) token;
            if (checkValidEntry(entry)) {
              string name = entry["Name"].ToString();
              string modelID_s = entry["ModelID"].ToString();
              string ModelAddress_s = entry["ModelAddress"].ToString();
              string Behavior_s = entry["Behavior"].ToString();
              byte modelID = parseByte(modelID_s);
              uint ModelAddress = parseUInt(ModelAddress_s);
              uint Behavior = parseUInt(Behavior_s);
              ObjectComboEntry oce =
                  new ObjectComboEntry(name, modelID, ModelAddress, Behavior);
              if (entry["BP1_NAME"] != null)
                oce.BP1_NAME = entry["BP1_NAME"].ToString();
              if (entry["BP2_NAME"] != null)
                oce.BP2_NAME = entry["BP2_NAME"].ToString();
              if (entry["BP3_NAME"] != null)
                oce.BP3_NAME = entry["BP3_NAME"].ToString();
              if (entry["BP4_NAME"] != null)
                oce.BP4_NAME = entry["BP4_NAME"].ToString();
              if (entry["BP1_DESCRIPTION"] != null)
                oce.BP1_DESCRIPTION = entry["BP1_DESCRIPTION"].ToString();
              if (entry["BP2_DESCRIPTION"] != null)
                oce.BP2_DESCRIPTION = entry["BP2_DESCRIPTION"].ToString();
              if (entry["BP3_DESCRIPTION"] != null)
                oce.BP3_DESCRIPTION = entry["BP3_DESCRIPTION"].ToString();
              if (entry["BP4_DESCRIPTION"] != null)
                oce.BP4_DESCRIPTION = entry["BP4_DESCRIPTION"].ToString();
              Globals.objectComboEntries.Add(oce);
            }
          }
        }
      }
    }
}