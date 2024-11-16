using System;
using System.Collections.Generic;
using System.Windows.Forms;

using fin.audio.io;

namespace uni.ui.winforms.common.audio;

public partial class AudioPlayerPanel : UserControl, IAudioPlayerPanel {
  public AudioPlayerPanel() {
      this.InitializeComponent();

      this.impl_.OnChange += audioFileBundle => {
        this.Invoke(() => {
          if (audioFileBundle != null) {
            this.groupBox_.Text = audioFileBundle.DisplayFullPath.ToString();
          } else {
            this.groupBox_.Text = "(Select audio)";
          }
        });

        this.OnChange(audioFileBundle);
      };
    }

  public IReadOnlyList<IAudioFileBundle>? AudioFileBundles {
    get => this.impl_.AudioFileBundles;
    set => this.impl_.AudioFileBundles = value;
  }

  public event Action<IAudioFileBundle?> OnChange = delegate { };
}