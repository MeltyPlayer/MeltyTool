using System.Collections.Generic;

using fin.util.asserts;

namespace uni.ui.winforms.common.fileTreeView {
  public abstract partial class FileTreeView<TFiles> {
    private class FuzzyTreeComparer : IComparer<IBetterTreeNode<BFileNode>> {
      public int Compare(IBetterTreeNode<BFileNode> lhs,
                         IBetterTreeNode<BFileNode> rhs)
        => -Asserts.CastNonnull(lhs.Data)
                   .Similarity.CompareTo(
                       Asserts.CastNonnull(rhs.Data).Similarity);
    }
  }
}