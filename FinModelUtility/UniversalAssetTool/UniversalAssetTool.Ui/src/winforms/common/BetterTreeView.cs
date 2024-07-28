using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace uni.ui.winforms.common;

public static class BetterTreeUtil {
  public static void ForEach<T, S>(TreeNodeCollection collection,
                                   Action<T> callback)
      where T : IBetterTreeNode<S>
      where S : class {
      // TODO: Remove unboxing?
      foreach (var childTreeNodeObj in collection) {
        var childTreeNode = (TreeNode) childTreeNodeObj;
        var childBetterTreeNode =
            GetBetterFrom<T, S>(childTreeNode);

        callback(childBetterTreeNode);
      }
    }

  public static T GetBetterFrom<T, S>(TreeNode node)
      where T : IBetterTreeNode<S>
      where S : class
    => (T) node.Tag;
}

public interface IBetterTreeView<T> where T : class {
  void Clear();

  IBetterTreeNode<T> Root { get; }

  delegate void SelectedHandler(IBetterTreeNode<T> betterTreeNode);

  event SelectedHandler Selected;
  IBetterTreeNode<T>? SelectedNode { get; }

  Func<IBetterTreeNode<T>, IEnumerable<(string, Action)>>
      ContextMenuItemsGenerator { get; set; }
}


public interface IBetterTreeNode<T> where T : class {
  TreeNode? Impl { get; }

  IBetterTreeView<T> Tree { get; }
  IBetterTreeNode<T>? Parent { get; }

  T? Data { get; set; }
  string? Text { get; }

  Image? OpenImage { set; }
  Image? ClosedImage { set; }

  int OpenImageIndex { get; set; }
  int ClosedImageIndex { get; set; }

  IBetterTreeNode<T> Add(string text);
  void Add(IBetterTreeNode<T> node);

  void Remove(IBetterTreeNode<T> node);
  void RemoveChildren();

  void ResetChildrenRecursively(
      Func<IBetterTreeNode<T>, bool>? filter = null);

  bool IsExpanded { get; }
  void Expand();
  void Collapse();
  void ExpandRecursively();

  void EnsureParentIsExpanded();
}

public class BetterTreeView<T> : IBetterTreeView<T> where T : class {
  // TODO: Add tests.
  // TODO: Add support for different hierarchies.

  private readonly TreeView impl_;
  private readonly Dictionary<Image, int> imageToIndex_ = new();

  private BetterTreeViewComparer comparer_ = new();

  public IBetterTreeNode<T> Root { get; }

  public event IBetterTreeView<T>.SelectedHandler Selected = delegate { };

  public IBetterTreeNode<T>? SelectedNode
    => this.impl_.SelectedNode != null
        ? BetterTreeUtil.GetBetterFrom<BetterTreeNode, T>(
            this.impl_.SelectedNode)
        : null;

  public IComparer<IBetterTreeNode<T>>? Comparer {
    get => this.comparer_.Comparer;
    set => this.comparer_.Comparer = value;
  }

  public BetterTreeView(TreeView impl) {
      this.impl_ = impl;
      this.Root = new BetterTreeNode(this, null, null, impl.Nodes);

      this.impl_.AfterSelect += (sender, args)
          => this.Selected.Invoke(this.SelectedNode!);

      this.impl_.TreeViewNodeSorter = this.comparer_;

      this.impl_.AfterExpand += (sender, args) => {
        var node = args.Node!;
        var betterNode =
            BetterTreeUtil.GetBetterFrom<BetterTreeNode, T>(node);

        betterNode.IsExpanded = true;
        if (args.Action is TreeViewAction.ByMouse
                           or TreeViewAction.ByKeyboard) {
          betterNode.IsExpandedManually = true;
        }

        node.ImageIndex = node.SelectedImageIndex = betterNode.OpenImageIndex;
      };
      this.impl_.AfterCollapse += (sender, args) => {
        var node = args.Node!;
        var betterNode =
            BetterTreeUtil.GetBetterFrom<BetterTreeNode, T>(node);

        betterNode.IsExpanded = false;
        if (args.Action is TreeViewAction.ByMouse
                           or TreeViewAction.ByKeyboard) {
          betterNode.IsExpandedManually = false;
        }

        node.ImageIndex = node.SelectedImageIndex = betterNode.ClosedImageIndex;
      };

      this.impl_.NodeMouseClick += (sender, args) => {
        var senderControl = (Control) sender;
        if (senderControl == null || args.Node == null ||
            args.Button != MouseButtons.Right) {
          return;
        }

        if (this.ContextMenuItemsGenerator == null) {
          return;
        }

        this.impl_.SelectedNode = args.Node;

        var betterTreeNode =
            BetterTreeUtil.GetBetterFrom<BetterTreeNode, T>(args.Node);
        var items = this.ContextMenuItemsGenerator(betterTreeNode).ToArray();
        if (items.Length == 0) {
          return;
        }

        var contextMenu = new ContextMenuStrip();
        foreach (var (itemText, itemHandler) in items) {
          contextMenu.Items.Add(itemText, null, (s, e) => itemHandler());
        }

        contextMenu.Show(senderControl, args.Location);
      };
    }

  public Func<IBetterTreeNode<T>, IEnumerable<(string, Action)>>
      ContextMenuItemsGenerator { get; set; }

  public void BeginUpdate() {
      this.impl_.BeginUpdate();
      this.impl_.Hide();
      this.impl_.SuspendLayout();
      this.comparer_.Enabled = false;
      this.impl_.Sorted = false;
      this.impl_.Enabled = false;
      this.impl_.Visible = false;
    }

  public void EndUpdate() {
      this.comparer_.Enabled = true;
      this.impl_.Sorted = true;
      this.impl_.EndUpdate();
      this.impl_.ResumeLayout();
      this.impl_.Show();
      this.impl_.Visible = true;
      this.impl_.Enabled = true;
    }

  public void ScrollToTop() {
      var nodes = this.impl_.Nodes;
      if (nodes.Count > 0) {
        nodes[0].EnsureVisible();
      }
    }

  public void Clear() {
      this.Root.RemoveChildren();
    }

  // TODO: Slow
  public int GetOrAddIndexOfImage(Image? image) {
      if (image == null) {
        return -1;
      }

      if (this.imageToIndex_.TryGetValue(image, out var index)) {
        return index;
      }

      this.impl_.ImageList ??= new ImageList();
      var imageList = this.impl_.ImageList.Images;

      index = imageList.Count;
      imageList.Add(image);

      this.imageToIndex_[image] = index;

      return index;
    }


  private class BetterTreeNode : IBetterTreeNode<T> {
    private readonly BetterTreeNode? parent_;
    private readonly TreeNodeCollection collection_;

    private int openImageIndex_ = -1;
    private int closedImageIndex_ = -1;

    public TreeNode? Impl { get; }

    public IBetterTreeView<T> Tree { get; }
    public IBetterTreeNode<T>? Parent => this.parent_;

    public int AbsoluteIndex { get; }

    private readonly List<BetterTreeNode> absoluteChildren_ = [];

    // TODO: Possible to remove unboxing?
    public T? Data { get; set; }
    public string? Text => this.Impl?.Text;

    public BetterTreeNode(IBetterTreeView<T> tree,
                          BetterTreeNode? parent,
                          TreeNode? impl,
                          TreeNodeCollection collection) {
        this.Tree = tree;
        this.parent_ = parent;
        this.Impl = impl;

        if (impl != null) {
          impl.Tag = this;
        }

        this.collection_ = collection;

        this.AbsoluteIndex = impl?.Index ?? 0;
      }

    public Image? OpenImage {
      set => this.OpenImageIndex =
          ((BetterTreeView<T>) this.Tree).GetOrAddIndexOfImage(value);
    }

    public Image? ClosedImage {
      set => this.ClosedImageIndex =
          ((BetterTreeView<T>) this.Tree).GetOrAddIndexOfImage(value);
    }

    public int OpenImageIndex {
      get => this.openImageIndex_;
      set {
          this.openImageIndex_ = value;
          if (this.Impl != null && this.IsExpanded) {
            this.Impl.ImageIndex = this.Impl.SelectedImageIndex = value;
          }
        }
    }

    public int ClosedImageIndex {
      get => this.closedImageIndex_;
      set {
          this.closedImageIndex_ = value;
          if (this.Impl != null && !this.IsExpanded) {
            this.Impl.ImageIndex = this.Impl.SelectedImageIndex = value;
          }
        }
    }

    public IBetterTreeNode<T> Add(string text) {
        var childTreeNode = this.collection_.Add(text);

        var childBetterTreeNode =
            new BetterTreeNode(this.Tree,
                               this,
                               childTreeNode,
                               childTreeNode.Nodes);

        this.absoluteChildren_.Add(childBetterTreeNode);

        return childBetterTreeNode;
      }

    public void Add(IBetterTreeNode<T> node) {
        if (node.Impl.Parent == this.Impl) {
          if (this.Impl != null || this.collection_.Contains(node.Impl)) {
            return;
          }
        }

        if (node.Impl.Parent != null) {
          node.Parent?.Remove(node);
        }

        this.collection_.Add(node.Impl);
      }

    public void Remove(IBetterTreeNode<T> node) {
        if (node.Impl.Parent != this.Impl) {
          return;
        }

        this.collection_.Remove(node.Impl);
      }

    public void RemoveChildren() {
        foreach (var child in this.absoluteChildren_) {
          child.Impl?.Remove();
        }

        this.collection_.Clear();
        this.absoluteChildren_.Clear();
      }

    public void ResetChildrenRecursively(
        Func<IBetterTreeNode<T>, bool>? filter = null) {
        foreach (var childBetterTreeNode in this.absoluteChildren_) {
          if (filter != null && !filter(childBetterTreeNode)) {
            this.Remove(childBetterTreeNode);
            continue;
          }

          this.Add(childBetterTreeNode);

          if (filter == null &&
              childBetterTreeNode.IsExpandedManually != true) {
            childBetterTreeNode.Collapse();
          }

          childBetterTreeNode.ResetChildrenRecursively(filter);

          if (filter != null ||
              childBetterTreeNode.IsExpandedManually == true) {
            childBetterTreeNode.Expand();
          }

          var node = childBetterTreeNode.Impl;
          node.ImageIndex =
              node.SelectedImageIndex =
                  childBetterTreeNode.IsExpanded
                      ? childBetterTreeNode.OpenImageIndex
                      : childBetterTreeNode.ClosedImageIndex;
        }
      }

    public bool IsExpanded { get; set; }
    public bool? IsExpandedManually { get; set; }

    public void Expand() {
        if (!this.IsExpanded) {
          this.IsExpanded = true;
          this.Impl?.Expand();
        }
      }

    public void ExpandRecursively() {
        this.Expand();
        BetterTreeUtil.ForEach<IBetterTreeNode<T>, T>(
            this.collection_,
            childBetterTreeNode =>
                childBetterTreeNode.ExpandRecursively());
      }

    public void Collapse() {
        if (this.IsExpanded) {
          this.IsExpanded = false;
          this.Impl?.Collapse(true);
        }
      }

    public void EnsureParentIsExpanded() {
        if (this.parent_ == null) {
          return;
        }

        this.parent_.IsExpandedManually = true;
        this.parent_.Expand();
        this.parent_.EnsureParentIsExpanded();
      }
  }


  private class BetterTreeViewComparer : IComparer {
    private readonly IComparer<IBetterTreeNode<T>> defaultComparer_ =
        new DefaultBetterTreeViewComparer();

    public bool Enabled { get; set; }

    public IComparer<IBetterTreeNode<T>>? Comparer { get; set; }

    public int Compare(object lhs, object rhs) {
        if (!this.Enabled) {
          return 0;
        }

        var comparer = this.Comparer ?? this.defaultComparer_;
        return comparer.Compare((BetterTreeNode) (((TreeNode) lhs).Tag),
                                (BetterTreeNode) (((TreeNode) rhs).Tag));
      }

    private class DefaultBetterTreeViewComparer :
        IComparer<IBetterTreeNode<T>> {
      public int Compare(IBetterTreeNode<T> lhs, IBetterTreeNode<T> rhs)
        => ((BetterTreeNode) lhs).AbsoluteIndex.CompareTo(
            ((BetterTreeNode) rhs).AbsoluteIndex);
    }
  }
}