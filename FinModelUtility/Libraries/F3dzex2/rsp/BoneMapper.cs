using System.Collections.Generic;

using fin.model;

namespace f3dzex2.rsp;

public interface IBoneMapper {
  bool TryToGetBoneAtSegmentedAddress(uint ramAddress, out IBone? bone);
  void SetBoneAtSegmentedAddress(uint ramAddress, IBone bone);
}

public class BoneMapper : IBoneMapper {
  private readonly Dictionary<uint, IBone> impl_ = new();

  public bool TryToGetBoneAtSegmentedAddress(
      uint ramAddress,
      out IBone? bone)
    => this.impl_.TryGetValue(ramAddress, out bone);

  public void SetBoneAtSegmentedAddress(uint ramAddress, IBone bone)
    => this.impl_[ramAddress] = bone;
}