using System.Collections;

namespace gx.displayList;

public abstract class BVertexDescriptor(uint value) : IVertexDescriptor {
  private IEnumerable<(GxVertexAttribute, GxAttributeType?, GxColorComponentType?)>?
      cachedEnumerable_;

  public uint Value {
    get;
    set {
      field = value;
      this.cachedEnumerable_ = null;
    }
  } = value;

  IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

  public IEnumerator<(GxVertexAttribute, GxAttributeType?, GxColorComponentType?)> GetEnumerator() {
    this.cachedEnumerable_ ??= this.GetEnumerator(this.Value);
    return this.cachedEnumerable_.GetEnumerator();
  }

  protected abstract IEnumerable<(GxVertexAttribute, GxAttributeType?, GxColorComponentType?)>
      GetEnumerator(uint value);
}