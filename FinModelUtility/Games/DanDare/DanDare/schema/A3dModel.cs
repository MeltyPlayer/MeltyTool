using System.Numerics;

namespace DanDare.schema;

public enum A3dFieldType {
  BOOL = 0,
  INT = 2,
  INT_ARRAY = 3,
  FLOAT = 4,
  FLOAT_ARRAY = 5,
  DOUBLE = 6,
  STRING = 8,
  NODE = 10,
  NODE_ARRAY = 11,
  FLOAT3 = 12,
  FLOAT4 = 13
}

public interface IA3dField {
  string Name { get; }
  A3dFieldType Type { get; }
  List<IA3dField> Links { get; }
}

public sealed class A3dField<T> : IA3dField {
  public string Name { get; }
  public A3dFieldType Type { get; }
  public T Value { get; set; }
  public List<IA3dField> Links { get; } = new();

  public A3dField(string name, A3dFieldType type, T initialValue = default!) {
    this.Name = name;
    this.Type = type;
    this.Value = initialValue;
  }
}

public abstract class BA3dNode {
  public string Type { get; }
  public string Name { get; }
  public Dictionary<string, IA3dField> Fields { get; } = new();

  protected BA3dNode(string type, string name) {
    this.Type = type;
    this.Name = name;
  }

  protected A3dField<T> RegisterField<T>(string name,
                                         A3dFieldType type,
                                         T defaultValue = default!) {
    var field = new A3dField<T>(name, type, defaultValue);
    this.Fields[name] = field;
    return field;
  }

  public IA3dField GetOrCreateField(string name, A3dFieldType type) {
    if (this.Fields.TryGetValue(name, out var existing))
      return existing;

    IA3dField created = type switch {
        A3dFieldType.BOOL => new A3dField<bool>(name, type),
        A3dFieldType.INT  => new A3dField<int>(name, type),
        A3dFieldType.INT_ARRAY => new A3dField<int[]>(
            name,
            type,
            Array.Empty<int>()),
        A3dFieldType.FLOAT => new A3dField<float>(name, type),
        A3dFieldType.FLOAT_ARRAY => new A3dField<float[]>(
            name,
            type,
            Array.Empty<float>()),
        A3dFieldType.DOUBLE => new A3dField<double>(name, type),
        A3dFieldType.STRING => new A3dField<string>(name, type, string.Empty),
        A3dFieldType.NODE   => new A3dField<BA3dNode?>(name, type),
        A3dFieldType.NODE_ARRAY => new A3dField<List<BA3dNode>>(
            name,
            type,
            new List<BA3dNode>()),
        A3dFieldType.FLOAT3 => new A3dField<Vector3>(name, type),
        A3dFieldType.FLOAT4 => new A3dField<Vector4>(name, type),
        _ => throw new NotSupportedException(
            $"Unsupported field type {(int) type}")
    };

    this.Fields[name] = created;
    return created;
  }

  public static BA3dNode Create(string type, string name) => type switch {
      "aTransform"        => new A3dTransformNode(name),
      "aMesh"             => new A3dMeshNode(name),
      "aMaterial"         => new A3dMaterialNode(name),
      "aTexture"          => new A3dTextureNode(name),
      "aCamera"           => new A3dCameraNode(name),
      "aLight"            => new A3dLightNode(type, name),
      "aLightDirectional" => new A3dLightNode(type, name),
      "aLightOmni"        => new A3dLightNode(type, name),
      "aParameter"        => new A3dParameterNode(name),
      "aExtern"           => new A3dExternNode(name),
      "aTimer"            => new A3dTimerNode(name),
      "aTouch"            => new A3dTouchNode(name),
      "aFloatLinear"      => new A3dFloatLinearNode(name),
      "aPositionLinear"   => new A3dVector3LinearNode(type, name),
      "aColorLinear"      => new A3dVector3LinearNode(type, name),
      "aRotationLinear"   => new A3dRotationLinearNode(name),
      "aVertexLinear"     => new A3dVertexLinearNode(name),
      _                   => new A3dUnknownNode(type, name)
  };
}

public sealed class A3dUnknownNode : BA3dNode {
  public A3dUnknownNode(string type, string name) : base(type, name) { }
}

public sealed class A3dTransformNode : BA3dNode {
  private readonly A3dField<List<BA3dNode>> children_;
  private readonly A3dField<Vector3> position_;
  private readonly A3dField<Vector4> rotation_;
  private readonly A3dField<Vector3> scale_;
  private readonly A3dField<Vector4> scaleRotation_;
  private readonly A3dField<Vector3> center_;
  private readonly A3dField<int> choice_;

  public List<BA3dNode> Children {
    get => this.children_.Value;
    set => this.children_.Value = value;
  }

  public Vector3 Position {
    get => this.position_.Value;
    set => this.position_.Value = value;
  }

  public Quaternion Rotation {
    get => this.rotation_.Value.AsQuaternion();
    set => this.rotation_.Value = value.AsVector4();
  }

  public Vector3 Scale {
    get => this.scale_.Value;
    set => this.scale_.Value = value;
  }

  public Quaternion ScaleRotation {
    get => this.scaleRotation_.Value.AsQuaternion();
    set => this.scaleRotation_.Value = value.AsVector4();
  }

  public Vector3 Center {
    get => this.center_.Value;
    set => this.center_.Value = value;
  }

  public int Choice {
    get => this.choice_.Value;
    set => this.choice_.Value = value;
  }

  public A3dTransformNode(string name) : base("aTransform", name) {
    this.children_ = this.RegisterField("children",
                                        A3dFieldType.NODE_ARRAY,
                                        new List<BA3dNode>());
    this.position_
        = this.RegisterField("position", A3dFieldType.FLOAT3, Vector3.Zero);
    this.rotation_ = this.RegisterField("rotation",
                                        A3dFieldType.FLOAT4,
                                        Quaternion.Identity.AsVector4());
    this.scale_ = this.RegisterField("scale", A3dFieldType.FLOAT3, Vector3.One);
    this.scaleRotation_ = this.RegisterField("scaleRotation",
                                             A3dFieldType.FLOAT4,
                                             new Vector4(0, 0, 1, 0));
    this.center_
        = this.RegisterField("center", A3dFieldType.FLOAT3, Vector3.Zero);
    this.choice_ = this.RegisterField("choice", A3dFieldType.INT, -1);
  }
}

public sealed class A3dMeshNode : BA3dNode {
  private readonly A3dField<bool> ccw_;
  private readonly A3dField<float[]> vertex_;
  private readonly A3dField<int[]> vertexIndex_;
  private readonly A3dField<float[]> normal_;
  private readonly A3dField<int[]> normalIndex_;
  private readonly A3dField<float[]> map_;
  private readonly A3dField<int[]> mapIndex_;
  private readonly A3dField<float[]> color_;
  private readonly A3dField<int[]> colorIndex_;
  private readonly A3dField<BA3dNode?> material_;
  private readonly A3dField<int[]> materialIndex_;
  private readonly A3dField<BA3dNode?> texture_;
  private readonly A3dField<float> crease_;

  public bool Ccw {
    get => this.ccw_.Value;
    set => this.ccw_.Value = value;
  }

  public float[] Positions {
    get => this.vertex_.Value;
    set => this.vertex_.Value = value;
  }

  public int[] PositionIndices {
    get => this.vertexIndex_.Value;
    set => this.vertexIndex_.Value = value;
  }

  public float[] Normals {
    get => this.normal_.Value;
    set => this.normal_.Value = value;
  }

  public int[] NormalIndices {
    get => this.normalIndex_.Value;
    set => this.normalIndex_.Value = value;
  }

  public float[] Uvs {
    get => this.map_.Value;
    set => this.map_.Value = value;
  }

  public int[] UvIndices {
    get => this.mapIndex_.Value;
    set => this.mapIndex_.Value = value;
  }

  public float[] Color {
    get => this.color_.Value;
    set => this.color_.Value = value;
  }

  public int[] ColorIndex {
    get => this.colorIndex_.Value;
    set => this.colorIndex_.Value = value;
  }

  public A3dMaterialNode? Material {
    get => this.material_.Value as A3dMaterialNode;
    set => this.material_.Value = value;
  }

  public int[] MaterialIndex {
    get => this.materialIndex_.Value;
    set => this.materialIndex_.Value = value;
  }

  public A3dTextureNode? Texture {
    get => this.texture_.Value as A3dTextureNode;
    set => this.texture_.Value = value;
  }

  public float Crease {
    get => this.crease_.Value;
    set => this.crease_.Value = value;
  }

  public A3dMeshNode(string name) : base("aMesh", name) {
    this.ccw_ = this.RegisterField("ccw", A3dFieldType.BOOL, true);
    this.vertex_ = this.RegisterField("vertex",
                                      A3dFieldType.FLOAT_ARRAY,
                                      Array.Empty<float>());
    this.vertexIndex_ = this.RegisterField("vertexIndex",
                                           A3dFieldType.INT_ARRAY,
                                           Array.Empty<int>());
    this.normal_ = this.RegisterField("normal",
                                      A3dFieldType.FLOAT_ARRAY,
                                      Array.Empty<float>());
    this.normalIndex_ = this.RegisterField("normalIndex",
                                           A3dFieldType.INT_ARRAY,
                                           Array.Empty<int>());
    this.map_ = this.RegisterField("map",
                                   A3dFieldType.FLOAT_ARRAY,
                                   Array.Empty<float>());
    this.mapIndex_ = this.RegisterField("mapIndex",
                                        A3dFieldType.INT_ARRAY,
                                        Array.Empty<int>());
    this.color_ = this.RegisterField("color",
                                     A3dFieldType.FLOAT_ARRAY,
                                     Array.Empty<float>());
    this.colorIndex_ = this.RegisterField("colorIndex",
                                          A3dFieldType.INT_ARRAY,
                                          Array.Empty<int>());
    this.material_ = this.RegisterField<BA3dNode?>("material",
      A3dFieldType.NODE,
      null);
    this.materialIndex_ = this.RegisterField("materialIndex",
                                             A3dFieldType.INT_ARRAY,
                                             Array.Empty<int>());
    this.texture_ = this.RegisterField<BA3dNode?>("texture",
                                                  A3dFieldType.NODE,
                                                  null);
    this.crease_ = this.RegisterField("crease", A3dFieldType.FLOAT, 0.5f);
  }
}

public sealed class A3dMaterialNode : BA3dNode {
  private readonly A3dField<Vector3> diffuse_;
  private readonly A3dField<Vector3> emissive_;
  private readonly A3dField<BA3dNode?> texture_;
  private readonly A3dField<float> alpha_;
  private readonly A3dField<float> transparency_;
  private readonly A3dField<int> shading_;
  private readonly A3dField<bool> bilinear_;

  public Vector3 Diffuse {
    get => this.diffuse_.Value;
    set => this.diffuse_.Value = value;
  }

  public Vector3 Emissive {
    get => this.emissive_.Value;
    set => this.emissive_.Value = value;
  }

  public A3dTextureNode? Texture {
    get => this.texture_.Value as A3dTextureNode;
    set => this.texture_.Value = value;
  }

  public float Alpha {
    get => this.alpha_.Value;
    set => this.alpha_.Value = value;
  }

  public float Transparency {
    get => this.transparency_.Value;
    set => this.transparency_.Value = value;
  }

  public int Shading {
    get => this.shading_.Value;
    set => this.shading_.Value = value;
  }

  public bool Bilinear {
    get => this.bilinear_.Value;
    set => this.bilinear_.Value = value;
  }

  public A3dMaterialNode(string name) : base("aMaterial", name) {
    this.diffuse_
        = this.RegisterField("diffuse", A3dFieldType.FLOAT3, new Vector3(0.8f));
    this.emissive_
        = this.RegisterField("emissive", A3dFieldType.FLOAT3, Vector3.Zero);
    this.texture_
        = this.RegisterField<BA3dNode?>("texture", A3dFieldType.NODE, null);
    this.alpha_ = this.RegisterField("alpha", A3dFieldType.FLOAT, 1.0f);
    this.transparency_
        = this.RegisterField("transparency", A3dFieldType.FLOAT, 0.0f);
    this.shading_ = this.RegisterField("shading", A3dFieldType.INT, 0);
    this.bilinear_ = this.RegisterField("bilinear", A3dFieldType.BOOL, true);
  }
}

public sealed class A3dTextureNode : BA3dNode {
  private readonly A3dField<string> image_;
  private readonly A3dField<bool> bilinear_;

  public string Image {
    get => this.image_.Value;
    set => this.image_.Value = value;
  }

  public bool Bilinear {
    get => this.bilinear_.Value;
    set => this.bilinear_.Value = value;
  }

  public A3dTextureNode(string name) : base("aTexture", name) {
    this.image_
        = this.RegisterField("image", A3dFieldType.STRING, string.Empty);
    this.bilinear_ = this.RegisterField("bilinear", A3dFieldType.BOOL, true);
  }
}

public sealed class A3dCameraNode : BA3dNode {
  private readonly A3dField<float> fov_;
  private readonly A3dField<Vector3> position_;
  private readonly A3dField<Vector4> orientation_;
  private readonly A3dField<string> description_;
  private readonly A3dField<int> cameraType_;
  private readonly A3dField<float> near_;
  private readonly A3dField<float> far_;

  public float Fov {
    get => this.fov_.Value;
    set => this.fov_.Value = value;
  }

  public Vector3 Position {
    get => this.position_.Value;
    set => this.position_.Value = value;
  }

  public Quaternion Orientation {
    get => this.orientation_.Value.AsQuaternion();
    set => this.orientation_.Value = value.AsVector4();
  }

  public string Description {
    get => this.description_.Value;
    set => this.description_.Value = value;
  }

  public int CameraType {
    get => this.cameraType_.Value;
    set => this.cameraType_.Value = value;
  }

  public float Near {
    get => this.near_.Value;
    set => this.near_.Value = value;
  }

  public float Far {
    get => this.far_.Value;
    set => this.far_.Value = value;
  }

  public A3dCameraNode(string name) : base("aCamera", name) {
    this.fov_ = this.RegisterField("fov", A3dFieldType.FLOAT, 0.785398f);
    this.position_ = this.RegisterField("position",
                                        A3dFieldType.FLOAT3,
                                        new Vector3(0, 0, 10));
    this.orientation_ = this.RegisterField("orientation",
                                           A3dFieldType.FLOAT4,
                                           Quaternion.Identity.AsVector4());
    this.description_
        = this.RegisterField("description", A3dFieldType.STRING, string.Empty);
    this.cameraType_ = this.RegisterField("type", A3dFieldType.INT, 0);
    this.near_ = this.RegisterField("near", A3dFieldType.FLOAT, 0.1f);
    this.far_ = this.RegisterField("far", A3dFieldType.FLOAT, 1000.0f);
  }
}

public sealed class A3dLightNode : BA3dNode {
  private readonly A3dField<bool> on_;
  private readonly A3dField<Vector3> color_;
  private readonly A3dField<float> intensity_;
  private readonly A3dField<Vector3> position_;
  private readonly A3dField<Vector3> direction_;
  private readonly A3dField<Vector3> attenuation_;
  private readonly A3dField<float> radius_;

  public bool On {
    get => this.on_.Value;
    set => this.on_.Value = value;
  }

  public Vector3 Color {
    get => this.color_.Value;
    set => this.color_.Value = value;
  }

  public float Intensity {
    get => this.intensity_.Value;
    set => this.intensity_.Value = value;
  }

  public Vector3 Position {
    get => this.position_.Value;
    set => this.position_.Value = value;
  }

  public Vector3 Direction {
    get => this.direction_.Value;
    set => this.direction_.Value = value;
  }

  public Vector3 Attenuation {
    get => this.attenuation_.Value;
    set => this.attenuation_.Value = value;
  }

  public float Radius {
    get => this.radius_.Value;
    set => this.radius_.Value = value;
  }

  public A3dLightNode(string type, string name) : base(type, name) {
    this.on_ = this.RegisterField("on", A3dFieldType.BOOL, true);
    this.color_ = this.RegisterField("color", A3dFieldType.FLOAT3, Vector3.One);
    this.intensity_ = this.RegisterField("intensity", A3dFieldType.FLOAT, 1.0f);
    this.position_
        = this.RegisterField("position", A3dFieldType.FLOAT3, Vector3.Zero);
    this.direction_ = this.RegisterField("direction",
                                         A3dFieldType.FLOAT3,
                                         new Vector3(0, 0, -1));
    this.attenuation_ = this.RegisterField("attenuation",
                                           A3dFieldType.FLOAT3,
                                           new Vector3(1, 0, 0));
    this.radius_ = this.RegisterField("radius", A3dFieldType.FLOAT, 100.0f);
  }
}

public sealed class A3dParameterNode : BA3dNode {
  private readonly A3dField<string> copyright_;
  private readonly A3dField<bool> showCopyright_;
  private readonly A3dField<bool> sceneBilinear_;
  private readonly A3dField<Vector3> backgroundColor_;
  private readonly A3dField<string> backgroundImage_;
  private readonly A3dField<bool> sceneAntialias_;
  private readonly A3dField<Vector3> fogColor_;
  private readonly A3dField<float> fogNear_;
  private readonly A3dField<float> fogFar_;
  private readonly A3dField<bool> fogActive_;

  public string Copyright {
    get => this.copyright_.Value;
    set => this.copyright_.Value = value;
  }

  public bool ShowCopyright {
    get => this.showCopyright_.Value;
    set => this.showCopyright_.Value = value;
  }

  public bool SceneBilinear {
    get => this.sceneBilinear_.Value;
    set => this.sceneBilinear_.Value = value;
  }

  public Vector3 BackgroundColor {
    get => this.backgroundColor_.Value;
    set => this.backgroundColor_.Value = value;
  }

  public string BackgroundImage {
    get => this.backgroundImage_.Value;
    set => this.backgroundImage_.Value = value;
  }

  public bool SceneAntialias {
    get => this.sceneAntialias_.Value;
    set => this.sceneAntialias_.Value = value;
  }

  public Vector3 FogColor {
    get => this.fogColor_.Value;
    set => this.fogColor_.Value = value;
  }

  public float FogNear {
    get => this.fogNear_.Value;
    set => this.fogNear_.Value = value;
  }

  public float FogFar {
    get => this.fogFar_.Value;
    set => this.fogFar_.Value = value;
  }

  public bool FogActive {
    get => this.fogActive_.Value;
    set => this.fogActive_.Value = value;
  }

  public A3dParameterNode(string name) : base("aParameter", name) {
    this.copyright_
        = this.RegisterField("copyright", A3dFieldType.STRING, string.Empty);
    this.showCopyright_
        = this.RegisterField("showCopyright", A3dFieldType.BOOL, true);
    this.sceneBilinear_
        = this.RegisterField("sceneBilinear", A3dFieldType.BOOL, true);
    this.backgroundColor_
        = this.RegisterField("backgroundColor",
                             A3dFieldType.FLOAT3,
                             Vector3.Zero);
    this.backgroundImage_
        = this.RegisterField("backgroundImage",
                             A3dFieldType.STRING,
                             string.Empty);
    this.sceneAntialias_
        = this.RegisterField("sceneAntialias", A3dFieldType.BOOL, false);
    this.fogColor_
        = this.RegisterField("fogColor", A3dFieldType.FLOAT3, Vector3.One);
    this.fogNear_ = this.RegisterField("fogNear", A3dFieldType.FLOAT, 0.0f);
    this.fogFar_ = this.RegisterField("fogFar", A3dFieldType.FLOAT, 100.0f);
    this.fogActive_ = this.RegisterField("fogActive", A3dFieldType.BOOL, false);
  }
}

public sealed class A3dExternNode : BA3dNode {
  private readonly A3dField<string> url_;
  private readonly A3dField<string> command_;

  public string Url {
    get => this.url_.Value;
    set => this.url_.Value = value;
  }

  public string Command {
    get => this.command_.Value;
    set => this.command_.Value = value;
  }

  public A3dExternNode(string name) : base("aExtern", name) {
    this.url_ = this.RegisterField("url", A3dFieldType.STRING, string.Empty);
    this.command_
        = this.RegisterField("command", A3dFieldType.STRING, string.Empty);
  }
}

public sealed class A3dTimerNode : BA3dNode {
  private readonly A3dField<double> cycleInterval_;
  private readonly A3dField<bool> loop_;
  private readonly A3dField<bool> enabled_;
  private readonly A3dField<bool> isActive_;
  private readonly A3dField<double> startTime_;
  private readonly A3dField<double> stopTime_;
  private readonly A3dField<double> cycleTime_;
  private readonly A3dField<double> time_;
  private readonly A3dField<float> alpha_;

  public double CycleInterval {
    get => this.cycleInterval_.Value;
    set => this.cycleInterval_.Value = value;
  }

  public bool Loop {
    get => this.loop_.Value;
    set => this.loop_.Value = value;
  }

  public bool Enabled {
    get => this.enabled_.Value;
    set => this.enabled_.Value = value;
  }

  public bool IsActive {
    get => this.isActive_.Value;
    set => this.isActive_.Value = value;
  }

  public double StartTime {
    get => this.startTime_.Value;
    set => this.startTime_.Value = value;
  }

  public double StopTime {
    get => this.stopTime_.Value;
    set => this.stopTime_.Value = value;
  }

  public double CycleTime {
    get => this.cycleTime_.Value;
    set => this.cycleTime_.Value = value;
  }

  public double Time {
    get => this.time_.Value;
    set => this.time_.Value = value;
  }

  public float Alpha {
    get => this.alpha_.Value;
    set => this.alpha_.Value = value;
  }

  public A3dTimerNode(string name) : base("aTimer", name) {
    this.cycleInterval_
        = this.RegisterField("cycleInterval", A3dFieldType.DOUBLE, 1.0);
    this.loop_ = this.RegisterField("loop", A3dFieldType.BOOL, false);
    this.enabled_ = this.RegisterField("enabled", A3dFieldType.BOOL, true);
    this.isActive_ = this.RegisterField("isActive", A3dFieldType.BOOL, false);
    this.startTime_ = this.RegisterField("startTime", A3dFieldType.DOUBLE, 0.0);
    this.stopTime_ = this.RegisterField("stopTime", A3dFieldType.DOUBLE, 0.0);
    this.cycleTime_ = this.RegisterField("cycleTime", A3dFieldType.DOUBLE, 0.0);
    this.time_ = this.RegisterField("time", A3dFieldType.DOUBLE, 0.0);
    this.alpha_ = this.RegisterField("alpha", A3dFieldType.FLOAT, 0.0f);
  }
}

public sealed class A3dTouchNode : BA3dNode {
  private readonly A3dField<BA3dNode?> touch_;
  private readonly A3dField<bool> enabled_;
  private readonly A3dField<bool> pressed_;
  private readonly A3dField<bool> isOver_;
  private readonly A3dField<double> touchTime_;
  private readonly A3dField<bool> sendBool_;
  private readonly A3dField<double> sendTime_;

  public BA3dNode? Touch {
    get => this.touch_.Value;
    set => this.touch_.Value = value;
  }

  public bool Enabled {
    get => this.enabled_.Value;
    set => this.enabled_.Value = value;
  }

  public bool Pressed {
    get => this.pressed_.Value;
    set => this.pressed_.Value = value;
  }

  public bool IsOver {
    get => this.isOver_.Value;
    set => this.isOver_.Value = value;
  }

  public double TouchTime {
    get => this.touchTime_.Value;
    set => this.touchTime_.Value = value;
  }

  public bool SendBool {
    get => this.sendBool_.Value;
    set => this.sendBool_.Value = value;
  }

  public double SendTime {
    get => this.sendTime_.Value;
    set => this.sendTime_.Value = value;
  }

  public A3dTouchNode(string name) : base("aTouch", name) {
    this.touch_
        = this.RegisterField<BA3dNode?>("touch", A3dFieldType.NODE, null);
    this.enabled_ = this.RegisterField("enabled", A3dFieldType.BOOL, true);
    this.pressed_ = this.RegisterField("pressed", A3dFieldType.BOOL, false);
    this.isOver_ = this.RegisterField("isOver", A3dFieldType.BOOL, false);
    this.touchTime_ = this.RegisterField("touchTime", A3dFieldType.DOUBLE, 0.0);
    this.sendBool_ = this.RegisterField("sendBool", A3dFieldType.BOOL, false);
    this.sendTime_ = this.RegisterField("sendTime", A3dFieldType.DOUBLE, 0.0);
  }
}

public abstract class A3dInterpolatorNode<TValue> : BA3dNode {
  private readonly A3dField<float> alpha_;
  private readonly A3dField<float[]> keyTime_;
  private readonly A3dField<float[]> keyValue_;
  private readonly A3dField<TValue> value_;

  public float Alpha {
    get => this.alpha_.Value;
    set => this.alpha_.Value = value;
  }

  public float[] KeyTime {
    get => this.keyTime_.Value;
    set => this.keyTime_.Value = value;
  }

  public float[] KeyValue {
    get => this.keyValue_.Value;
    set => this.keyValue_.Value = value;
  }

  public TValue Value {
    get => this.value_.Value;
    set => this.value_.Value = value;
  }

  protected A3dInterpolatorNode(string type,
                                string name,
                                A3dFieldType valueFieldType,
                                TValue defaultValue = default!)
      : base(type, name) {
    this.alpha_ = this.RegisterField("alpha", A3dFieldType.FLOAT, 0.0f);
    this.keyTime_
        = RegisterField("keyTime",
                        A3dFieldType.FLOAT_ARRAY,
                        Array.Empty<float>());
    this.keyValue_
        = RegisterField("keyValue",
                        A3dFieldType.FLOAT_ARRAY,
                        Array.Empty<float>());
    this.value_ = this.RegisterField("value", valueFieldType, defaultValue);
  }
}

public sealed class A3dFloatLinearNode : A3dInterpolatorNode<float> {
  public A3dFloatLinearNode(string name)
      : base("aFloatLinear", name, A3dFieldType.FLOAT, 0.0f) { }
}

public sealed class A3dVector3LinearNode : A3dInterpolatorNode<Vector3> {
  public A3dVector3LinearNode(string type, string name)
      : base(type, name, A3dFieldType.FLOAT3, Vector3.Zero) { }
}

public sealed class A3dRotationLinearNode : A3dInterpolatorNode<Quaternion> {
  public A3dRotationLinearNode(string name)
      : base("aRotationLinear",
             name,
             A3dFieldType.FLOAT4,
             Quaternion.Identity) { }
}

public sealed class A3dVertexLinearNode : A3dInterpolatorNode<float[]> {
  public A3dVertexLinearNode(string name)
      : base("aVertexLinear",
             name,
             A3dFieldType.FLOAT_ARRAY,
             []) { }
}

public sealed class A3dScene {
  public A3dTransformNode? Root { get; set; }
  public Dictionary<string, byte[]> EmbeddedFiles { get; } = new();
}