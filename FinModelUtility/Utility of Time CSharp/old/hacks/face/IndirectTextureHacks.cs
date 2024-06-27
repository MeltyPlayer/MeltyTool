using System.Collections.Generic;

using UoT.hacks;
using UoT.hacks.fields;

#pragma warning disable CS8603

namespace UoT;

public enum EyeState {
  AUTO = 0,
  OPEN = 1,
  HALF_OPEN = 2,
  CLOSED = 3,
  LOOKING_LEFT = 4,
  LOOKING_RIGHT = 5,
  SHOCK = 6,
  LOOKING_DOWN = 7,
  CLOSED_TIGHT = 8,
}

public enum MouthState {
  AUTO = 0,
  CLOSED = 1,
  SLIGHTLY_OPEN = 2,
  OPEN = 3,
  SMILE = 4,
}

public interface IIndirectTextureHack : IHack {
  EyeState EyeState { get; set; }
  MouthState MouthState { get; set; }

  /// <summary>
  ///   Maps a given texture address to a new address. This is needed to map
  ///   an eye/mouth address from a random spot in RAM to where they're
  ///   actually defined in ROM (as this is basically what would happen
  ///   in-game.)
  /// </summary>
  uint MapTextureAddress(uint originalAddress);
}

public abstract class BIndirectTextureHack : IIndirectTextureHack {
  public abstract IReadOnlyList<IField> Fields { get; }

  public EyeState EyeState { get => default; set { } }
  public MouthState MouthState { get => default; set { } }

  public abstract uint MapTextureAddress(uint originalAddress);
}


public static class IndirectTextureHacks {
  private static IDictionary<string, IIndirectTextureHack> impl_ =
      new Dictionary<string, IIndirectTextureHack>();

  static IndirectTextureHacks() {
      var linkIndirectTextureHack = new LinkIndirectTextureHack();
      IndirectTextureHacks.Add_(
          ("object_link_boy", linkIndirectTextureHack),
          ("object_link_child", linkIndirectTextureHack),
          ("object_tite", new TektiteIndirectTextureHack()),
          ("object_zl1", new ZeldaChildIndirectTextureHack()),
          ("object_zl2", new ZeldaAdultIndirectTextureHack())
      );
    }

  public static IIndirectTextureHack Get(string filename) {
      IndirectTextureHacks.impl_.TryGetValue(filename, out var hack);
      return hack;
    }

  private static void Add_(params (string, IIndirectTextureHack)[] pairs) {
      foreach (var pair in pairs) {
        IndirectTextureHacks.impl_.Add(pair.Item1, pair.Item2);
      }
    }
}