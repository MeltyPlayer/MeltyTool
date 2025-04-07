using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace vhr.api;

/// <summary>
///   Shamelessly stolen from:
///   https://stackoverflow.com/a/18997172
/// </summary>
class SingleOrArrayConverter<T> : JsonConverter {
  public override bool CanConvert(Type objectType) {
    return (objectType == typeof(List<T>));
  }

  public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
    JToken token = JToken.Load(reader);
    if (token.Type == JTokenType.Array) {
      return token.ToObject<List<T>>()!;
    }
    if (token.Type == JTokenType.Null) {
      return null!;
    }
    return new List<T> { token.ToObject<T>()! };
  }

  public override bool CanWrite {
    get { return false; }
  }

  public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
    throw new NotImplementedException();
  }
} 