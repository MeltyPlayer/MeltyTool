using fin.data.queues;

using Sledge.Formats.Map.Formats;

using SledgeEntity = Sledge.Formats.Map.Objects.Entity;
using SledgeMapObject = Sledge.Formats.Map.Objects.MapObject;
using SledgeSolid = Sledge.Formats.Map.Objects.Solid;

namespace Celeste64.map;

public class Map {
  public IReadOnlyList<SledgeEntity> Entities { get; }
  public IReadOnlyList<SledgeSolid> Solids { get; }

  public Map(Stream stream) {
    var sledgeMap = new QuakeMapFormat().Read(stream);

    var entities = new LinkedList<SledgeEntity>();
    var solids = new LinkedList<SledgeSolid>();

    var mapObjectQueue = new FinQueue<SledgeMapObject>(sledgeMap.Worldspawn);
    while (mapObjectQueue.TryDequeue(out var mapObj)) {
      switch (mapObj) {
        case SledgeEntity entity: {
          entities.AddLast(entity);
          break;
        }
        case SledgeSolid solid: {
          solids.AddLast(solid);
          break;
        }
      }

      mapObjectQueue.Enqueue(mapObj.Children);
    }

    this.Entities = entities.ToArray();
    this.Solids = solids.ToArray();
  }
}