namespace sm64.JSON {
  class ModelCombo {
    byte modelId = 0;
    uint segmentAddress = 0;

    public byte ModelID {
      get { return this.modelId; }
    }

    public uint SegmentAddress {
      get { return this.segmentAddress; }
    }

    public ModelCombo(byte modelId, uint segmentAddress) {
      this.modelId = modelId;
      this.segmentAddress = segmentAddress;
    }

    public override string ToString() {
      return "[0x" +
             this.modelId.ToString("X2") + ", 0x" +
             this.segmentAddress.ToString("X8") + "]";
    }
  }
}