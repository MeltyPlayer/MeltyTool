using fin.schema;
using fin.schema.vector;

using schema.binary;
using schema.binary.attributes;

using sm64.scripts.geo;

namespace sm64.scripts {
  public enum GeoCommandId : byte {
    BRANCH_AND_STORE = 0x00,
    TERMINATE = 0x01,
    BRANCH = 0x02,
    RETURN_FROM_BRANCH = 0x03,
    OPEN_NODE = 0x04,
    CLOSE_NODE = 0x05,
    VIEWPORT = 0x08,
    ORTHO_MATRIX = 0x09,
    CAMERA_FRUSTUM = 0x0A,
    START_LAYOUT = 0x0B,
    TOGGLE_DEPTH_BUFFER = 0x0C,
    SET_RENDER_RANGE = 0x0D,
    SWITCH = 0x0E,
    CAMERA_LOOK_AT = 0x0F,
    TRANSLATE_AND_ROTATE = 0x10,
    TRANSLATE = 0x11,
    ROTATE = 0x12,
    ANIMATED_PART = 0x13,
    BILLBOARD = 0x14,
    DISPLAY_LIST = 0x15,
    SHADOW = 0x16,
    OBJECT_LIST = 0x17,
    DISPLAY_LIST_FROM_ASM = 0x18,
    BACKGROUND = 0x19,
    NOOP_1A = 0x1A,
    HELD_OBJECT = 0x1C,
    SCALE = 0x1D,
    CULLING_RADIUS = 0x20,
  }

  public enum GeoDrawingLayer : byte {
    OPAQUE_NO_AA,
    OPAQUE_WITH_AA,
    DECALS,
    INTERSECTING_POLYGONS,
    TRANSPARENT_PIXELS,
    BLENDING1,
    BLENDING2,
    BLENDING3
  }

  public enum GeoTranslateAndRotateFormat : byte {
    TRANSLATION_AND_ROTATION,
    TRANSLATION,
    ROTATION,
    YAW
  }

  public interface IGeoCommandList {
    IReadOnlyList<IGeoCommand> Commands { get; }
  }

  public interface IGeoCommand {
    GeoCommandId Id { get; }
  }

  [BinarySchema]
  public partial class GeoBranchAndStoreCommand : IGeoCommand, IBinaryDeserializable {
    public GeoCommandId Id => GeoCommandId.BRANCH_AND_STORE;

    [IntegerFormat(SchemaIntegerType.UINT24)]
    private readonly uint padding_ = 0;

    public uint GeoCommandSegmentedAddress { get; set; }

    [Skip]
    public IGeoCommandList? GeoCommandList { get; set; }
  }

  [BinarySchema]
  public partial class GeoTerminateCommand : IGeoCommand, IBinaryDeserializable {
    public GeoCommandId Id => GeoCommandId.TERMINATE;

    [IntegerFormat(SchemaIntegerType.UINT24)]
    private readonly uint padding_ = 0;
  }

  [BinarySchema]
  public partial class GeoBranchCommand : IGeoCommand, IBinaryDeserializable {
    public GeoCommandId Id => GeoCommandId.BRANCH;

    [IntegerFormat(SchemaIntegerType.BYTE)]
    public bool StoreReturnAddress { get; set; }

    private readonly ushort padding_ = 0;

    public uint GeoCommandSegmentedAddress { get; set; }

    [Skip]
    public IGeoCommandList? GeoCommandList { get; set; }
  }

  [BinarySchema]
  public partial class GeoReturnFromBranchCommand
      : IGeoCommand, IBinaryDeserializable {
    public GeoCommandId Id => GeoCommandId.RETURN_FROM_BRANCH;

    [IntegerFormat(SchemaIntegerType.UINT24)]
    private readonly uint padding_ = 0;
  }

  [BinarySchema]
  public partial class GeoOpenNodeCommand : IGeoCommand, IBinaryDeserializable {
    public GeoCommandId Id => GeoCommandId.OPEN_NODE;

    [IntegerFormat(SchemaIntegerType.UINT24)]
    private readonly uint padding_ = 0;
  }

  [BinarySchema]
  public partial class GeoCloseNodeCommand : IGeoCommand, IBinaryDeserializable {
    public GeoCommandId Id => GeoCommandId.CLOSE_NODE;

    [IntegerFormat(SchemaIntegerType.UINT24)]
    private readonly uint padding_ = 0;
  }

  [BinarySchema]
  public partial class GeoViewportCommand : IGeoCommand, IBinaryDeserializable {
    public GeoCommandId Id => GeoCommandId.VIEWPORT;
    private readonly byte padding_ = 0;

    public short NumEntries { get; set; }
    public short X { get; set; }
    public short Y { get; set; }
    public short Width { get; set; }
    public short Height { get; set; }
  }

  [BinarySchema]
  public partial class GeoOrthoMatrixCommand : IGeoCommand, IBinaryDeserializable {
    public GeoCommandId Id => GeoCommandId.ORTHO_MATRIX;
    private readonly byte padding_ = 0;

    public short NearPlane { get; set; }
  }

  [BinarySchema]
  public partial class GeoCameraFrustumCommand : IGeoCommand, IBinaryDeserializable {
    public GeoCommandId Id => GeoCommandId.CAMERA_FRUSTUM;

    [IntegerFormat(SchemaIntegerType.BYTE)]
    public bool EnableFrustumFunc { get; private set; }

    public short FieldOfView { get; set; }
    public short Near { get; set; }
    public short Far { get; set; }

    [RIfBoolean(nameof(EnableFrustumFunc))]
    public uint? FrustumFuncSegmentedAddress { get; set; }
  }

  [BinarySchema]
  public partial class GeoStartLayoutCommand : IGeoCommand, IBinaryDeserializable {
    public GeoCommandId Id => GeoCommandId.START_LAYOUT;

    [IntegerFormat(SchemaIntegerType.UINT24)]
    private readonly uint padding_ = 0;
  }

  [BinarySchema]
  public partial class GeoToggleDepthBufferCommand
      : IGeoCommand, IBinaryDeserializable {
    public GeoCommandId Id => GeoCommandId.TOGGLE_DEPTH_BUFFER;

    [IntegerFormat(SchemaIntegerType.BYTE)]
    public bool Enable { get; set; }

    private readonly ushort padding_ = 0;
  }


  [BinarySchema]
  public partial class GeoSetRenderRangeCommand : IGeoCommand, IBinaryDeserializable {
    public GeoCommandId Id => GeoCommandId.SET_RENDER_RANGE;

    [IntegerFormat(SchemaIntegerType.UINT24)]
    private readonly uint padding_ = 0;

    public short MinimumDistance { get; set; }
    public short MaximumDistance { get; set; }
  }

  [BinarySchema]
  public partial class GeoSwitchCommand : IGeoCommand, IBinaryDeserializable {
    public GeoCommandId Id => GeoCommandId.SWITCH;
    private readonly byte padding_ = 0;

    public short InitialSelectedCase { get; set; }

    public uint CaseSelectorSegmentedAddress { get; set; }
  }

  [BinarySchema]
  public partial class GeoCameraLookAtCommand : IGeoCommand, IBinaryDeserializable {
    public GeoCommandId Id => GeoCommandId.CAMERA_LOOK_AT;
    private readonly byte padding_ = 0;

    public short CameraType { get; set; }
    public Vector3s EyePosition { get; } = new();
    public Vector3s FocusPosition { get; } = new();
    public uint Function { get; set; }
  }

  public class GeoTranslateAndRotateCommand : IGeoCommand {
    public GeoCommandId Id => GeoCommandId.TRANSLATE_AND_ROTATE;

    /// <summary>
    ///   Determines whether display list is enabled.
    ///
    ///   Determines how the translation/rotation params are stored.
    /// </summary>
    public byte Params { get; set; }

    public GeoDrawingLayer DrawingLayer
      => GeoUtils.GetDrawingLayerFromParams(Params);

    public bool HasDisplayList
      => GeoUtils.IsDisplayListAndDrawingLayerEnabled(Params);

    public GeoTranslateAndRotateFormat Format
      => GeoUtils.GetTranslateAndRotateFormat(Params);

    public Vector3s Translation => new();
    public Vector3s Rotation => new();

    public uint? DisplayListSegmentedAddress { get; set; }
  }

  [BinarySchema]
  public partial class GeoTranslationCommand : IGeoCommand, IBinaryDeserializable {
    public GeoCommandId Id => GeoCommandId.TRANSLATE;

    /// <summary>
    ///   Determines whether display list is enabled.
    /// </summary>
    public byte Params { get; set; }

    [Skip]
    public GeoDrawingLayer DrawingLayer
      => GeoUtils.GetDrawingLayerFromParams(Params);

    public Vector3s Translation => new();

    [Skip]
    public bool HasDisplayList
      => GeoUtils.IsDisplayListAndDrawingLayerEnabled(Params);

    [RIfBoolean(nameof(HasDisplayList))]
    public uint? DisplayListSegmentedAddress { get; set; }
  }

  [BinarySchema]
  public partial class GeoRotationCommand : IGeoCommand, IBinaryDeserializable {
    public GeoCommandId Id => GeoCommandId.ROTATE;

    /// <summary>
    ///   Determines whether display list is enabled.
    /// </summary>
    public byte Params { get; set; }

    [Skip]
    public GeoDrawingLayer DrawingLayer
      => GeoUtils.GetDrawingLayerFromParams(Params);

    public Vector3s Rotation => new();

    [Skip]
    public bool HasDisplayList
      => GeoUtils.IsDisplayListAndDrawingLayerEnabled(Params);

    [RIfBoolean(nameof(HasDisplayList))]
    public uint? DisplayListSegmentedAddress { get; set; }
  }

  [BinarySchema]
  public partial class GeoAnimatedPartCommand
      : IGeoCommand, IBinaryDeserializable {
    public GeoCommandId Id => GeoCommandId.ANIMATED_PART;

    public GeoDrawingLayer DrawingLayer { get; set; }

    public Vector3s Offset => new();

    public uint DisplayListSegmentedAddress { get; set; }
  }

  [BinarySchema]
  public partial class GeoBillboardCommand : IGeoCommand, IBinaryDeserializable {
    public GeoCommandId Id => GeoCommandId.BILLBOARD;

    /// <summary>
    ///   Determines whether display list is enabled.
    /// </summary>
    public byte Params { get; set; }

    [Skip]
    public GeoDrawingLayer DrawingLayer
      => GeoUtils.GetDrawingLayerFromParams(Params);

    public Vector3s Translation => new();

    [Skip]
    public bool HasDisplayList
      => GeoUtils.IsDisplayListAndDrawingLayerEnabled(Params);

    [RIfBoolean(nameof(HasDisplayList))]
    public uint? DisplayListSegmentedAddress { get; set; }
  }

  [BinarySchema]
  public partial class GeoDisplayListCommand : IGeoCommand, IBinaryDeserializable {
    public GeoCommandId Id => GeoCommandId.DISPLAY_LIST;
    public GeoDrawingLayer DrawingLayer { get; set; }
    private readonly ushort padding_ = 0;

    public uint DisplayListSegmentedAddress { get; set; }
  }

  [BinarySchema]
  public partial class GeoShadowCommand : IGeoCommand, IBinaryDeserializable {
    public GeoCommandId Id => GeoCommandId.SHADOW;
    private readonly byte padding_ = 0;

    public short ShadowType { get; set; }
    public short ShadowSolidity { get; set; }
    public short ShadowScale { get; set; }
  }

  [BinarySchema]
  public partial class GeoObjectListCommand : IGeoCommand, IBinaryDeserializable {
    public GeoCommandId Id => GeoCommandId.OBJECT_LIST;

    [IntegerFormat(SchemaIntegerType.UINT24)]
    private readonly uint padding_ = 0;
  }

  [BinarySchema]
  public partial class GeoDisplayListFromAsm : IGeoCommand, IBinaryDeserializable {
    public GeoCommandId Id => GeoCommandId.DISPLAY_LIST_FROM_ASM;

    private readonly byte padding_ = 0;

    public ushort Params { get; set; }
    public uint AsmSegmentedAddress { get; set; }
  }

  [BinarySchema]
  public partial class GeoBackgroundCommand : IGeoCommand, IBinaryDeserializable {
    public GeoCommandId Id => GeoCommandId.BACKGROUND;

    private readonly byte padding_ = 0;

    public short BackgroundIdOrColor { get; set; }

    public uint BackgroundFunc { get; set; }
  }

  [BinarySchema]
  public partial class GeoNoopCommand : IGeoCommand, IBinaryDeserializable {
    public GeoCommandId Id { get; set; }

    [IntegerFormat(SchemaIntegerType.UINT24)]
    private readonly uint padding1_ = 0;

    private readonly uint padding2_ = 0;
  }

  [BinarySchema]
  public partial class GeoHeldObjectCommand : IGeoCommand, IBinaryDeserializable {
    public GeoCommandId Id => GeoCommandId.HELD_OBJECT;

    [Unknown]
    public byte Unk { get; set; }

    public Vector3s Offset { get; } = new();

    public uint Func { get; set; }
  }

  [BinarySchema]
  public partial class GeoScaleCommand : IGeoCommand, IBinaryDeserializable {
    public GeoCommandId Id => GeoCommandId.SCALE;

    /// <summary>
    ///   Determines whether display list is enabled.
    /// </summary>
    public byte Params { get; set; }

    [Skip]
    public GeoDrawingLayer DrawingLayer
      => GeoUtils.GetDrawingLayerFromParams(Params);

    private readonly ushort padding_ = 0;

    public uint Scale { get; set; }

    [Skip]
    public bool HasDisplayList
      => GeoUtils.IsDisplayListAndDrawingLayerEnabled(Params);

    [RIfBoolean(nameof(HasDisplayList))]
    public uint? DisplayListSegmentedAddress { get; set; }
  }

  [BinarySchema]
  public partial class GeoCullingRadiusCommand : IGeoCommand, IBinaryDeserializable {
    public GeoCommandId Id => GeoCommandId.CULLING_RADIUS;

    private readonly byte padding_ = 0;

    public short CullingRadius { get; set; }
  }
}