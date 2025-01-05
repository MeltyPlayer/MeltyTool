namespace vrml.util;

internal enum PropertyConstants {
  Marked, FaceListIndex, Median, IncidentEdges, HeVertexIndex
}

internal class DynamicProperties {
  private Dictionary<PropertyConstants, object> _properties
      = new Dictionary<PropertyConstants, object>();

  public int Count {
    get { return this._properties.Count; }
  }

  internal void AddProperty(PropertyConstants key, object value) {
    this._properties.Add(key, value);
  }

  internal bool ExistsKey(PropertyConstants key) {
    if (this._properties.ContainsKey(key))
      return true;
    return false;
  }

  internal object GetValue(PropertyConstants key) {
    return this._properties[key];
  }

  internal void ChangeValue(PropertyConstants key, object value) {
    if (!this.ExistsKey(key))
      throw new Exception("Key " + key + " was not found.");
    this._properties[key] = value;
  }

  internal void Clear() {
    this._properties.Clear();
  }

  internal void RemoveKey(PropertyConstants key) {
    this._properties.Remove(key);
  }
}