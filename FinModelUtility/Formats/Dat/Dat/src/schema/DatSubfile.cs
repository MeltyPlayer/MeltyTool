using dat.schema.animation;
using dat.schema.melee;

using fin.data.queues;
using fin.util.linq;

using schema.binary;
using schema.binary.attributes;
using schema.util.enumerables;

namespace dat.schema;

/// <summary>
///   References:
///     - https://github.com/jam1garner/Smash-Forge/blob/c0075bca364366bbea2d3803f5aeae45a4168640/Smash%20Forge/Filetypes/Melee/DAT.cs
/// </summary>
public class DatSubfile : IBinaryDeserializable {
  private readonly List<RootNode> rootNodes_ = [];
  private readonly HashSet<uint> validOffsets_ = [];

  private uint dataBlockOffset_;
  private uint relocationTableOffset_;
  private uint rootNodeOffset_;
  private uint referenceNodeOffset_;
  private uint stringTableOffset_;

  public uint FileSize { get; set; }

  public LinkedList<(IDatNode, string)> RootNodesWithNames { get; } = [];

  public IEnumerable<IDatNode> RootNodes
    => this.RootNodesWithNames.Select(
        rootNodeAndName => rootNodeAndName.Item1);

  public IEnumerable<TNode> GetRootNodesOfType<TNode>()
      where TNode : IDatNode
    => this.RootNodes.WhereIs<IDatNode, TNode>();

  public IEnumerable<(TNode, string)> GetRootNodesWithNamesOfType<TNode>()
      where TNode : IDatNode
    => this.RootNodesWithNames
           .SelectWhere<(IDatNode, string), (TNode, string)>(IsOfType_);

  private static bool IsOfType_<TNode>(
      (IDatNode, string) nodeWithName,
      out (TNode, string) outNodeWithName)
      where TNode : IDatNode {
    var (node, name) = nodeWithName;
    if (node is TNode datNode) {
      outNodeWithName = (datNode, name);
      return true;
    }

    outNodeWithName = default;
    return false;
  }


  public IEnumerable<JObj> RootJObjs
    => this.GetRootNodesOfType<JObj>()
           .Concat(this.GetRootNodesOfType<SObj>()
                       .SelectMany(
                           sObj => sObj.JObjDescs?.Values.Select(
                                       jObjDesc => jObjDesc.RootJObj) ??
                                   [])
                       .WhereNonnull());

  private readonly Dictionary<uint, JObj> jObjByOffset_ = new();
  public IReadOnlyDictionary<uint, JObj> JObjByOffset => this.jObjByOffset_;

  public IEnumerable<JObj> JObjs => this.RootJObjs.SelectMany(
      DatNodeExtensions.GetSelfAndChildrenAndSiblings);


  public void Read(IBinaryReader br) {
    var fileHeader = br.ReadNew<FileHeader>();
    this.FileSize = fileHeader.FileSize;

    this.dataBlockOffset_ = 0x20;
    this.relocationTableOffset_ =
        this.dataBlockOffset_ + fileHeader.DataBlockSize;
    this.rootNodeOffset_ =
        this.relocationTableOffset_ + 4 * fileHeader.RelocationTableCount;
    this.referenceNodeOffset_ =
        this.rootNodeOffset_ + 8 * fileHeader.RootNodeCount;
    this.stringTableOffset_ =
        this.referenceNodeOffset_ + 8 * fileHeader.ReferenceNodeCount;

    // Reads relocation table
    this.validOffsets_.Clear();
    for (var i = 0; i < fileHeader.RelocationTableCount; ++i) {
      br.Position = this.relocationTableOffset_ + 4 * i;
      var relocationTableEntryOffset = br.ReadUInt32();

      br.Position = this.dataBlockOffset_ + relocationTableEntryOffset;
      var relocationTableValue = br.ReadUInt32();

      this.validOffsets_.Add(relocationTableValue);
    }

    // Reads root nodes
    this.rootNodes_.Clear();
    for (var i = 0; i < fileHeader.RootNodeCount; i++) {
      br.Position = this.rootNodeOffset_ + 8 * i;

      var rootNode = new RootNode();
      rootNode.Data.Read(br);

      br.SubreadAt(this.stringTableOffset_ + rootNode.Data.StringOffset,
                   sbr => { rootNode.Name = sbr.ReadStringNT(); });

      this.rootNodes_.Add(rootNode);
    }

    // TODO: Handle reference nodes

    // Reads root bone structures
    this.ReadRootNodeObjects_(br);
    this.ReadNames_(br);
  }

  private void ReadRootNodeObjects_(IBinaryReader br) {
    br.Position = this.dataBlockOffset_;
    br.PushLocalSpace();

    var jObjQueue = new FinTuple2Queue<uint, JObj>();

    this.RootNodesWithNames.Clear();
    foreach (var rootNode in this.rootNodes_) {
      var rootNodeOffset = rootNode.Data.DataOffset;
      br.Position = rootNodeOffset;

      IDatNode? node = null;
      switch (rootNode.Type) {
        case RootNodeType.JOBJ: {
          var jObj = br.ReadNew<JObj>();
          node = jObj;

          jObjQueue.Enqueue((rootNodeOffset, jObj));
          break;
        }
        case RootNodeType.MATANIM_JOINT: {
          node = br.ReadNew<MatAnimJoint>();
          break;
        }
        case RootNodeType.FIGATREE: {
          node = br.ReadNew<FigaTree>();
          break;
        }
        case RootNodeType.FIGHTER_DATA: {
          node = br.ReadNew<MeleeFighterData>();
          break;
        }
        case RootNodeType.SCENE_DATA: {
          var sObj = br.ReadNew<SObj>();
          node = sObj;

          var jObjDescs = sObj.JObjDescs?.Values;
          if (jObjDescs != null) {
            foreach (var jObjDesc in jObjDescs) {
              var rootJObj = jObjDesc.RootJObj;
              if (rootJObj != null) {
                jObjQueue.Enqueue((jObjDesc.RootJObjOffset, jObjDesc.RootJObj));
              }
            }
          }

          break;
        }
      }

      if (node != null) {
        this.RootNodesWithNames.AddLast((node, rootNode.Name));
      }
    }

    br.PopLocalSpace();

    while (jObjQueue.TryDequeue(out var jObjOffset, out var jObj)) {
      this.jObjByOffset_[jObjOffset] = jObj;

      if (jObj.FirstChild != null) {
        jObjQueue.Enqueue((jObj.FirstChildBoneOffset, jObj.FirstChild));
      }

      if (jObj.NextSibling != null) {
        jObjQueue.Enqueue((jObj.NextSiblingBoneOffset, jObj.NextSibling));
      }
    }
  }

  private void ReadNames_(IBinaryReader br) {
    br.Position = this.stringTableOffset_;
    br.PushLocalSpace();

    foreach (var jObj in this.JObjs) {
      var jObjStringOffset = jObj.StringOffset;
      if (jObjStringOffset != 0) {
        br.Position = jObjStringOffset;
        jObj.Name = br.ReadStringNT();
      }

      foreach (var dObj in jObj.DObjs) {
        var dObjStringOffset = dObj.StringOffset;
        if (dObjStringOffset != 0) {
          br.Position = dObjStringOffset;
          dObj.Name = br.ReadStringNT();
        }

        var mObj = dObj.MObj;
        if (mObj != null) {
          var mObjStringOffset = mObj.StringOffset;
          if (mObjStringOffset != 0) {
            br.Position = mObjStringOffset;
            mObj.Name = br.ReadStringNT();
          }

          foreach (var (_, tObj) in mObj.TObjsAndOffsets) {
            var tObjStringOffset = tObj.StringOffset;
            if (tObjStringOffset != 0) {
              br.Position = tObj.StringOffset;
              tObj.Name = br.ReadStringNT();
            }
          }
        }
      }
    }

    br.PopLocalSpace();
  }
}

[BinarySchema]
public partial class FileHeader : IBinaryConvertible {
  public uint FileSize { get; set; }
  public uint DataBlockSize { get; set; }

  public uint RelocationTableCount { get; set; }
  public uint RootNodeCount { get; set; }
  public uint ReferenceNodeCount { get; set; }

  [StringLengthSource(4)]
  public string Version { get; set; }

  public uint Padding1 { get; set; }
  public uint Padding2 { get; set; }


  [Skip]
  public uint DataBlockOffset => 0x20;

  [Skip]
  public uint RelocationTableOffset
    => this.DataBlockOffset + this.DataBlockSize;

  [Skip]
  public uint RootNodeOffset
    => this.RelocationTableOffset + 4 * this.RelocationTableCount;

  [Skip]
  public uint ReferenceNodeOffset
    => this.RootNodeOffset + 8 * this.RootNodeCount;

  [Skip]
  public uint StringTableOffset
    => this.ReferenceNodeOffset + 8 * this.ReferenceNodeCount;
}

[BinarySchema]
public partial class RootNodeData : IBinaryConvertible {
  public uint DataOffset { get; set; }
  public uint StringOffset { get; set; }
}

public enum RootNodeType {
  UNDEFINED,
  JOBJ,
  MATANIM_JOINT,
  FIGATREE,
  IMAGE,
  SCENE_DATA,
  SCENE_MODELSET,
  TLUT,
  TLUT_DESC,
  FIGHTER_DATA
}

public class RootNode {
  private string name_;

  public RootNodeData Data { get; } = new();

  public string Name {
    get => this.name_;
    set => this.Type = GetTypeFromName_(this.name_ = value);
  }

  public override string ToString() => $"[{this.Type}]: {this.Name}";

  public RootNodeType Type { get; private set; }

  private static RootNodeType GetTypeFromName_(string name) {
    // TODO: Use flags for this instead
    if (name.EndsWith("_joint") &&
        !name.Contains("matanim") &&
        !name.Contains("anim_joint")) {
      return RootNodeType.JOBJ;
    }

    if (name.EndsWith("_matanim_joint")) {
      return RootNodeType.MATANIM_JOINT;
    }

    if (name.EndsWith("_figatree")) {
      return RootNodeType.FIGATREE;
    }

    if (name.StartsWith("ftData")) {
      return RootNodeType.FIGHTER_DATA;
    }

    if (name.EndsWith("_image")) {
      return RootNodeType.IMAGE;
    }

    if (name.EndsWith("scene_data")) {
      return RootNodeType.SCENE_DATA;
    }

    if (name.EndsWith("_scene_modelset")) {
      return RootNodeType.SCENE_MODELSET;
    }

    if (name.EndsWith("_tlut")) {
      return RootNodeType.TLUT;
    }

    if (name.EndsWith("_tlut_desc")) {
      return RootNodeType.TLUT_DESC;
    }

    if (name == "") {
      return RootNodeType.UNDEFINED;
    }

    return RootNodeType.UNDEFINED;
  }
}