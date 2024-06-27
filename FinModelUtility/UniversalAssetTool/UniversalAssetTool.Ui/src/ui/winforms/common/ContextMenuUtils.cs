using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace uni.ui.winforms.common {
  public static class ContextMenuUtils {
    public static bool IsValidRightClickEvent(object? sender,
                                              EventArgs e,
                                              out Control senderControl,
                                              out MouseEventArgs mouseEventArgs) {
      senderControl = (Control) sender;
      mouseEventArgs = (MouseEventArgs) e;
      return senderControl != null && mouseEventArgs != null &&
             mouseEventArgs.Button == MouseButtons.Right;
    }

    public static EventHandler CreateRightClickEventHandler(
        Func<IEnumerable<(string, Action)>> itemGenerator)
      => (sender, e) => {
        if (!IsValidRightClickEvent(sender,
                                    e,
                                    out var senderControl,
                                    out var mouseEventArgs)) {
          return;
        }

        var items = itemGenerator().ToArray();
        if (items.Length == 0) {
          return;
        }

        var contextMenu = new ContextMenuStrip();
        foreach (var (itemText, itemHandler) in items) {
          contextMenu.Items.Add(itemText, null, (s, e) => itemHandler());
        }

        contextMenu.Show(senderControl, mouseEventArgs.Location);
      };
  }
}