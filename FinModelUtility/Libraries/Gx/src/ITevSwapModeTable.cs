namespace gx;

public enum ChannelId : byte {
  GX_CH_RED,
  GX_CH_GREEN,
  GX_CH_BLUE,
  GX_CH_ALPHA
}

public interface ITevSwapModeTable {
  ChannelId R { get; }
  ChannelId G { get; }
  ChannelId B { get; }
  ChannelId A { get; }
}