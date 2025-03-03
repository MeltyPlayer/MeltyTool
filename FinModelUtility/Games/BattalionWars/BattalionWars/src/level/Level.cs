using fin.data.dictionaries;

namespace modl.level;

public class Level {
  private readonly SubTypeDictionary<string, BLevelObject?> objectById_
      = new();

  public Level() {
    this.objectById_.Set<BLevelObject?>("0", null);
  }

  public TObject Get<TObject>(string id) where TObject : BLevelObject?
    => this.objectById_.Get<TObject>(id);

  public void Set<TObject>(string id, TObject obj)
      where TObject : BLevelObject
    => this.objectById_.Set(id, obj);
}