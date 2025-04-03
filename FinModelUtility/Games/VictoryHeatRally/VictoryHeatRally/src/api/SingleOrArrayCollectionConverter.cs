using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;


namespace vhr.api;

/// <summary>
///   Shamelessly stolen from:
///   https://stackoverflow.com/a/53771970
/// </summary>
public class SingleOrArrayCollectionConverter<TCollection, TItem>
    : JsonConverter where TCollection : ICollection<TItem> {
  // Adapted from this answer https://stackoverflow.com/a/18997172
  // to https://stackoverflow.com/questions/18994685/how-to-handle-both-a-single-item-and-an-array-for-the-same-property-using-json-n
  // by Brian Rogers https://stackoverflow.com/users/10263/brian-rogers
  readonly bool canWrite;

  public SingleOrArrayCollectionConverter() : this(false) { }

  public SingleOrArrayCollectionConverter(bool canWrite) {
    this.canWrite = canWrite;
  }

  public override bool CanConvert(Type objectType) {
    return typeof(TCollection).IsAssignableFrom(objectType);
  }

  static void ValidateItemContract_(IContractResolver resolver) {
    var itemContract = resolver.ResolveContract(typeof(TItem));
    if (itemContract is JsonArrayContract)
      throw new JsonSerializationException(
          string.Format("Item contract type {0} not supported.", itemContract));
  }

  public override object? ReadJson(JsonReader reader,
                                   Type objectType,
                                   object? existingValue,
                                   JsonSerializer serializer) {
    ValidateItemContract_(serializer.ContractResolver);
    if (reader.TokenType == JsonToken.Null)
      return null;
    var list = (ICollection<TItem?>) (existingValue ?? serializer
        .ContractResolver.ResolveContract(objectType)
        .DefaultCreator());
    if (reader.TokenType == JsonToken.StartArray)
      serializer.Populate(reader, list);
    else
      list.Add(serializer.Deserialize<TItem>(reader));
    return list;
  }

  public override bool CanWrite {
    get { return canWrite; }
  }

  public override void WriteJson(JsonWriter writer,
                                 object value,
                                 JsonSerializer serializer) {
    ValidateItemContract_(serializer.ContractResolver);
    var list = value as ICollection<TItem>;
    if (list == null)
      throw new JsonSerializationException(
          string.Format("Invalid type for {0}: {1}",
                        GetType(),
                        value.GetType()));
    if (list.Count == 1) {
      foreach (var item in list) {
        serializer.Serialize(writer, item);
        break;
      }
    } else {
      writer.WriteStartArray();
      foreach (var item in list)
        serializer.Serialize(writer, item);
      writer.WriteEndArray();
    }
  }
}