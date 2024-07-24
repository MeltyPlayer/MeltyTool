using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

using fin.color;
using fin.model;
using fin.util.linq;
using fin.util.strings;

using uni.ui.winforms.common;

namespace uni.ui.winforms.right_panel.registers;

public partial class RegistersTab : UserControl {
  public RegistersTab() {
      InitializeComponent();
    }

  public IReadOnlyModel? Model {
    set {
        this.SuspendLayout();

        this.flowLayoutPanel_.Controls.Clear();

        var materialManager = value?.MaterialManager;
        var registers = materialManager?.Registers;
        if (materialManager == null || registers == null) {
          this.ResumeLayout(true);
          return;
        }

        Func<string, TableLayoutPanel> createSection = text => {
          var panel = new GroupBox {
              Text = text,
              AutoSize = true,
              AutoSizeMode = AutoSizeMode.GrowAndShrink,
              Anchor = AnchorStyles.Left | AnchorStyles.Right,
              TabIndex = 0,
          };
          this.flowLayoutPanel_.Controls.Add(panel);

          panel.SuspendLayout();

          var tableLayoutPanel =
              new TableLayoutPanel {
                  AutoSize = true,
                  AutoSizeMode = AutoSizeMode.GrowAndShrink,
                  Dock = DockStyle.Fill,
                  ColumnCount = 2,
                  RowCount = 1,
                  TabIndex = 0,
              };
          panel.Controls.Add(tableLayoutPanel);

          var columnStyles = tableLayoutPanel.ColumnStyles;
          columnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
          columnStyles.Add(new ColumnStyle(SizeType.AutoSize));

          var rowStyles = tableLayoutPanel.RowStyles;
          rowStyles.Add(new ColumnStyle(SizeType.AutoSize));

          panel.ResumeLayout(true);

          return tableLayoutPanel;
        };

        var allEquations =
            materialManager
                .All
                .WhereIs<IReadOnlyMaterial, IReadOnlyFixedFunctionMaterial>()
                .Select(mat => mat.Equations)
                .ToArray();

        var comparer = new StringNumberSuffixComparer();
        var colorRegisters = registers.ColorRegisters
                                      .OrderBy(reg => reg.Name, comparer)
                                      .ToArray();
        var scalarRegisters = registers
                              .ScalarRegisters
                              .OrderBy(reg => reg.Name, comparer)
                              .ToArray();

        var colorRegistersSection = createSection("Color registers");
        {
          var row = 0;
          foreach (var colorRegister in colorRegisters) {
            if (!allEquations.Any(
                    equations => equations.DoOutputsDependOn(colorRegister))) {
              continue;
            }

            colorRegistersSection.Controls.Add(
                new Label { Text = colorRegister.Name, Dock = DockStyle.Fill, },
                0,
                row);

            var finColorValue = colorRegister.Value;
            var sysColorValue = Color.FromArgb(
                finColorValue.Rb,
                finColorValue.Gb,
                finColorValue.Bb);
            var colorPicker = new ColorPicker {
                Value = sysColorValue, Dock = DockStyle.Fill,
            };
            colorPicker.ValueChanged += newColor => {
              colorRegister.Value = FinColor.FromSystemColor(newColor);
            };

            colorRegistersSection.Controls.Add(colorPicker, 1, row);

            row++;
          }
        }

        var scalarRegistersSection = createSection("Scalar registers");
        {
          var row = 0;
          foreach (var scalarRegister in scalarRegisters) {
            if (!allEquations.Any(
                    equations => equations.DoOutputsDependOn(scalarRegister))) {
              continue;
            }

            scalarRegistersSection.Controls.Add(
                new Label { Text = scalarRegister.Name, Dock = DockStyle.Fill },
                0,
                row);

            var scale = 100f;
            var trackBar = new TrackBar {
                Minimum = 0,
                Maximum = (int) scale,
                TickFrequency = (int) (scale / 10),
                Value = (int) (scalarRegister.Value * scale),
                Dock = DockStyle.Fill,
            };
            trackBar.ValueChanged += (_, _) => {
              scalarRegister.Value = trackBar.Value / scale;
            };

            scalarRegistersSection.Controls.Add(trackBar, 1, row);

            row++;
          }
        }

        this.ResumeLayout(true);
      }
  }
}