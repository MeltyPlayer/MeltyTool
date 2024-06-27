using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace uni.ui.common {
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
}