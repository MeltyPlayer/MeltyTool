using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

using fin.data.fuzzy;
using fin.data.queues;
using fin.io.bundles;
using fin.util.actions;
using fin.util.asserts;


namespace uni.ui.winforms.common.fileTreeView {
  public abstract partial class FileTreeView<TFiles>
      : UserControl, IFileTreeView
      where TFiles : notnull {
    // TODO: Add tests.
    // TODO: Move the fuzzy logic to a separate reusable component.
    // TODO: Add support for different sorting systems.
    // TODO: Add support for different hierarchies.
    // TODO: Clean up the logic here.

    private readonly BetterTreeView<BFileNode> betterTreeView_;

    private readonly IFuzzyFilterTree<BFileNode> filterImpl_ =
        new FuzzyFilterTree<BFileNode>(fileNode => {
          var keywords = new HashSet<string>();

          if (fileNode is LeafFileNode leafFileNode) {
            var file = leafFileNode.File.FileBundle;
            var fileName = file.RawName;
            keywords.Add(fileName);

            var betterFileName = file.HumanReadableName;
            if (!string.IsNullOrEmpty(betterFileName)) {
              keywords.Add(betterFileName);
            }

            var uiPath = "";
            IFileTreeNode? current = fileNode;
            while (current != null) {
              uiPath = $"{current.Text}/{uiPath}";
              current = current.Parent;
            }

            keywords.Add(uiPath);
          }

          return keywords;
        });

    public event IFileTreeView.FileSelectedHandler FileSelected =
        delegate { };


    public event IFileTreeView.DirectorySelectedHandler
        DirectorySelected = delegate { };

    public FileTreeView() {
      this.InitializeComponent();

      var callFilterFromMainThread = () => this.Invoke(this.Filter_);
      this.filterDebounced_ = callFilterFromMainThread.Debounce();
      this.filterTextBox_.TextChanged += (_, _) => this.filterDebounced_();

      this.betterTreeView_ = new BetterTreeView<BFileNode>(this.fileTreeView_);
      this.betterTreeView_.Selected += betterTreeNode => {
        var fileNode = betterTreeNode.Data;
        switch (fileNode) {
          case ParentFileNode parentFileNode: {
              this.DirectorySelected.Invoke(parentFileNode);
              break;
            }
          case LeafFileNode leafFileNode: {
              this.FileSelected.Invoke(leafFileNode);
              break;
            }
          default: throw new NotImplementedException();
        }
      };
      this.betterTreeView_.ContextMenuItemsGenerator =
          this.GenerateContextMenuItems_;
    }

    private IEnumerable<(string, Action)> GenerateContextMenuItems_(
        IBetterTreeNode<BFileNode> betterNode) {
      if (betterNode.Data is LeafFileNode leafFileNode) {
        var fullName = leafFileNode.FullName;
        yield return (
            "Show in explorer",
            () => Process.Start("explorer.exe", $"/select,\"{fullName}\""));
      }
    }

    public void Populate(TFiles files) {
      this.betterTreeView_.BeginUpdate();

      this.PopulateImpl(files, new ParentFileNode(this));

      this.betterTreeView_.ScrollToTop();

      this.InitializeAutocomplete_();

      this.betterTreeView_.EndUpdate();
    }

    protected abstract void PopulateImpl(TFiles files, ParentFileNode root);

    public abstract Image GetImageForFile(IFileBundle file);


    private void InitializeAutocomplete_() {
      var allAutocompleteKeywords = new AutoCompleteStringCollection();

      var queue = new FinQueue<IFuzzyNode<BFileNode>>(this.filterImpl_.Root);
      while (queue.TryDequeue(out var filterNode)) {
        foreach (var keyword in filterNode.Keywords) {
          allAutocompleteKeywords.Add(keyword);
        }

        queue.Enqueue(filterNode.Children);
      }

      this.filterTextBox_.AutoCompleteCustomSource = allAutocompleteKeywords;
    }

    private readonly Action filterDebounced_;

    private void Filter_() {
      var filterText = this.filterTextBox_.Text.ToLower();

      if (string.IsNullOrEmpty(filterText) || filterText.Length <= 2) {
        if (this.betterTreeView_.Comparer != null) {
          this.betterTreeView_.SelectedNode?.EnsureParentIsExpanded();

          this.filterImpl_.Reset();
          this.betterTreeView_.BeginUpdate();

          this.betterTreeView_.Root.ResetChildrenRecursively();
          this.betterTreeView_.Comparer = null;

          this.betterTreeView_.EndUpdate();
        }
      } else {
        this.filterImpl_.Reset();
        this.betterTreeView_.BeginUpdate();

        this.filterImpl_.Filter(filterText, -1);
        this.betterTreeView_.Root.ResetChildrenRecursively(
            betterTreeNode =>
                Asserts.CastNonnull(betterTreeNode.Data).ChangeDistance <= 0);
        this.betterTreeView_.Comparer ??= new FuzzyTreeComparer();

        this.betterTreeView_.EndUpdate();
      }
    }
  }
}