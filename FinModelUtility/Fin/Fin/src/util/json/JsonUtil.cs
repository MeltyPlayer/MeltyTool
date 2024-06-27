using System.IO;

using fin.util.asserts;

using Newtonsoft.Json;

namespace fin.util.json;

public static class JsonUtil {
  public static string Serialize(object obj) {
    var jsonSerializer = new JsonSerializer {
        Formatting = Formatting.Indented
    };
    using var jsonTextWriter = new StringWriter();
    jsonSerializer.Serialize(jsonTextWriter, obj);
    return jsonTextWriter.ToString();
  }

  public static T Deserialize<T>(string text) {
    var jsonSerializer = new JsonSerializer {
        Formatting = Formatting.Indented
    };
    using var jsonReader = new JsonTextReader(new StringReader(text));
    return Asserts.CastNonnull(jsonSerializer.Deserialize<T>(jsonReader));
  }
}