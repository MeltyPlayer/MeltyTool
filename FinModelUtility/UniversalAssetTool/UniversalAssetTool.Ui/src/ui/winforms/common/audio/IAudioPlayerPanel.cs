using System;
using System.Collections.Generic;

using fin.audio.io;

namespace uni.ui.winforms.common.audio;

public interface IAudioPlayerPanel {
  public IReadOnlyList<IAudioFileBundle>? AudioFileBundles { get; set; }

  public event Action<IAudioFileBundle?> OnChange;
}