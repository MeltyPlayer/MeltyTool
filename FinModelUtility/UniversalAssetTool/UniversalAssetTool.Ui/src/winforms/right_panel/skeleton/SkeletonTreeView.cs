using System.Linq;
using System.Windows.Forms;

using fin.data.queues;
using fin.model;

using uni.ui.winforms.common;

#pragma warning disable CS8604


namespace uni.ui.winforms.right_panel.skeleton;

public interface ISkeletonTreeView {
  public delegate void BoneSelectedHandler(ISkeletonTreeNode skeletonNode);

  event BoneSelectedHandler BoneSelected;
}

public interface ISkeletonTreeNode {
  string Text { get; }
  IReadOnlyBone Bone { get; }

  ISkeletonTreeNode? Parent { get; }
}


public partial class SkeletonTreeView : UserControl, ISkeletonTreeView {
  private readonly BetterTreeView<SkeletonNode> betterTreeView_;

  public event ISkeletonTreeView.BoneSelectedHandler BoneSelected =
      delegate { };

  // TODO: Clean this up.
  protected class SkeletonNode : ISkeletonTreeNode {
    private readonly common.IBetterTreeNode<SkeletonNode> treeNode_;

    public SkeletonNode(SkeletonTreeView treeView, IReadOnlyBone bone) {
        this.Bone = bone;

        this.treeNode_ = treeView.betterTreeView_.Root;
        this.treeNode_.Data = this;
      }

    private SkeletonNode(SkeletonNode parent, IReadOnlyBone bone) {
        this.Bone = bone;

        this.treeNode_ = parent.treeNode_.Add(bone.Name);
        this.treeNode_.Data = this;
      }

    public string Text => this.treeNode_.Text ?? "n/a";
    public IReadOnlyBone Bone { get; set; }

    public ISkeletonTreeNode? Parent => this.treeNode_.Parent?.Data;

    public SkeletonNode AddChild(IReadOnlyBone bone) => new(this, bone);
  }

  public SkeletonTreeView() {
      this.InitializeComponent();

      this.betterTreeView_ =
          new BetterTreeView<SkeletonNode>(this.skeletonTreeView_);
      this.betterTreeView_.Selected += betterTreeNode => {
        var boneNode = betterTreeNode.Data;
        if (boneNode == null) {
          return;
        }

        this.BoneSelected.Invoke(boneNode);
      };
    }

  public void Populate(IReadOnlySkeleton? skeleton) {
      this.betterTreeView_.BeginUpdate();
      this.betterTreeView_.Clear();

      if (skeleton != null) {
        var boneQueue =
            new FinTuple2Queue<IReadOnlyBone, SkeletonNode>(
                (skeleton.Root, new SkeletonNode(this, skeleton.Root)));
        while (boneQueue.TryDequeue(out var bone, out var node)) {
          boneQueue.Enqueue(
              bone.Children
                  .Select(child => (child, node.AddChild(child))));
        }
      }

      this.betterTreeView_.Root.ExpandRecursively();

      this.betterTreeView_.ScrollToTop();
      this.betterTreeView_.EndUpdate();
    }
}