namespace gx;

public enum SwapTableId : byte {
  GX_TEV_SWAP0,
  GX_TEV_SWAP1,
  GX_TEV_SWAP2,
  GX_TEV_SWAP3
}

public interface ITevSwapMode {
  SwapTableId RasSel { get; }
  SwapTableId TexSel { get; }
}