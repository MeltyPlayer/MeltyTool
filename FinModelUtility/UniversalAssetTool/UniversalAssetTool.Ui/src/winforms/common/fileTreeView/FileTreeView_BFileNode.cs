using System.Collections.Generic;

using fin.data.fuzzy;


namespace uni.ui.winforms.common.fileTreeView;

public abstract partial class FileTreeView<TFiles> {
  protected abstract class BFileNode : IFileTreeNode {
    protected readonly IFileTreeView treeView;
    protected readonly IBetterTreeNode<BFileNode> treeNode;
    protected IFuzzyNode<BFileNode> filterNode;

    protected BFileNode(FileTreeView<TFiles> treeView) {
        this.treeView = treeView;
        this.treeNode = treeView.betterTreeView_.Root;
        this.treeNode.Data = this;
      }

    protected BFileNode(ParentFileNode parent, string text) {
        this.treeView = parent.treeView;
        this.treeNode = parent.treeNode.Add(text);
        this.treeNode.Data = this;
      }


    protected void InitializeFilterNode(ParentFileNode parent)
      => this.InitializeFilterNode(parent.filterNode);

    protected void InitializeFilterNode(IFuzzyNode<BFileNode> parent)
      => this.filterNode = parent.AddChild(this);


    public string Text => this.treeNode.Text ?? "n/a";
    public abstract string? FullName { get; }

    public IFileTreeParentNode? Parent
      => this.treeNode.Parent?.Data as ParentFileNode;

    public float Similarity => this.filterNode.Similarity;
    public float ChangeDistance => this.filterNode.ChangeDistance;
    public IReadOnlySet<string> Keywords => this.filterNode.Keywords;
  }
}