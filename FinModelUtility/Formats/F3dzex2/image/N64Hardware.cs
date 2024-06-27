using f3dzex2.io;

namespace f3dzex2.image;

public interface IN64Hardware {
  IN64Memory Memory { get; }
  IRsp Rsp { get; }
  IRdp Rdp { get; }
}

public interface IN64Hardware<TMemory> : IN64Hardware
    where TMemory : IN64Memory {
  new TMemory Memory { get; }
  IRsp Rsp { get; }
  IRdp Rdp { get; }
}


public class N64Hardware<TMemory> : IN64Hardware<TMemory>
    where TMemory : IN64Memory {
  IN64Memory IN64Hardware.Memory => this.Memory;
  public TMemory Memory { get; set; }
  public IRsp Rsp { get; set; }
  public IRdp Rdp { get; set; }
}