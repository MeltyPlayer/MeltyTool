using fin.model;

using schema.text.reader;

using vrml.schema;


namespace vrml.api;

public partial class VrmlParser {
  private static ShapeHintsNode ReadShapeHintsNode_(ITextReader tr) {
    VertexOrder vertexOrdering = VertexOrder.COUNTER_CLOCKWISE;
    string shapeType = "";

    ReadFields_(
        tr,
        fieldName => {
          switch (fieldName) {
            case "vertexOrdering": {
              vertexOrdering = ReadWord_(tr) switch {
                  "CLOCKWISE"         => VertexOrder.CLOCKWISE,
                  "COUNTER_CLOCKWISE" => VertexOrder.COUNTER_CLOCKWISE,
                  _                   => throw new ArgumentOutOfRangeException()
              };
              break;
            }
            case "shapeType": {
              shapeType = ReadWord_(tr);
              break;
            }
            default: throw new NotImplementedException();
          }
        });

    return new ShapeHintsNode {
        ShapeType = shapeType,
        VertexOrdering = vertexOrdering,
    };
  }
}