namespace sm64.JSON;

class ModelCombo {
  byte modelId = 0;
  uint segmentAddress = 0;

  public byte ModelID {
    get { return modelId; }
  }

  public uint SegmentAddress {
    get { return segmentAddress; }
  }

  public ModelCombo(byte modelId, uint segmentAddress) {
      this.modelId = modelId;
      this.segmentAddress = segmentAddress;
    }

  public override string ToString() {
      return "[0x" + modelId.ToString("X2") + ", 0x" +
             segmentAddress.ToString("X8") + "]";
    }
}