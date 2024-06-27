namespace gx;

public enum GxCc : byte {
  GX_CC_CPREV,
  GX_CC_APREV,
  GX_CC_C0,
  GX_CC_A0,
  GX_CC_C1,
  GX_CC_A1,
  GX_CC_C2,
  GX_CC_A2,
  GX_CC_TEXC,
  GX_CC_TEXA,
  GX_CC_RASC,
  GX_CC_RASA,
  GX_CC_ONE,
  GX_CC_HALF,
  GX_CC_KONST,
  GX_CC_ZERO,
}

public enum GxCa : byte {
  GX_CA_APREV,
  GX_CA_A0,
  GX_CA_A1,
  GX_CA_A2,
  GX_CA_TEXA,
  GX_CA_RASA,
  GX_CA_KONST,
  GX_CA_ZERO,
}

public enum TevOp : byte {
  GX_TEV_ADD = 0,
  GX_TEV_SUB = 1,
  GX_TEV_COMP_R8_GT = 8,
  GX_TEV_COMP_R8_EQ = 9,
  GX_TEV_COMP_GR16_GT = 10,
  GX_TEV_COMP_GR16_EQ = 11,
  GX_TEV_COMP_BGR24_GT = 12,
  GX_TEV_COMP_BGR24_EQ = 13,
  GX_TEV_COMP_RGB8_GT = 14,
  GX_TEV_COMP_RGB8_EQ = 15
}

public enum TevBias : byte {
  GX_TB_ZERO,
  GX_TB_ADDHALF,
  GX_TB_SUBHALF
}

public enum TevScale : byte {
  GX_CS_SCALE_1,
  GX_CS_SCALE_2,
  GX_CS_SCALE_4,
  GX_CS_DIVIDE_2
}

public enum ColorRegister : byte {
  GX_TEVPREV,
  GX_TEVREG0,
  GX_TEVREG1,
  GX_TEVREG2,
}

public interface ITevStageProps {
  GxCc color_a { get; }
  GxCc color_b { get; }
  GxCc color_c { get; }
  GxCc color_d { get; }
  TevOp color_op { get; }
  TevBias color_bias { get; }
  TevScale color_scale { get; }
  bool color_clamp { get; }
  ColorRegister color_regid { get; }

  GxCa alpha_a { get; }
  GxCa alpha_b { get; }
  GxCa alpha_c { get; }
  GxCa alpha_d { get; }
  TevOp alpha_op { get; }
  TevBias alpha_bias { get; }
  TevScale alpha_scale { get; }
  bool alpha_clamp { get; }
  ColorRegister alpha_regid { get; }
}

public class TevStagePropsImpl : ITevStageProps {
  public GxCc color_a { get; set; }
  public GxCc color_b { get; set; }
  public GxCc color_c { get; set; }
  public GxCc color_d { get; set; }
  public TevOp color_op { get; set; }
  public TevBias color_bias { get; set; }
  public TevScale color_scale { get; set; }
  public bool color_clamp { get; set; }
  public ColorRegister color_regid { get; set; }
  public GxCa alpha_a { get; set; }
  public GxCa alpha_b { get; set; }
  public GxCa alpha_c { get; set; }
  public GxCa alpha_d { get; set; }
  public TevOp alpha_op { get; set; }
  public TevBias alpha_bias { get; set; }
  public TevScale alpha_scale { get; set; }
  public bool alpha_clamp { get; set; }
  public ColorRegister alpha_regid { get; set; }
}