using System.Reactive.Subjects;

namespace marioartisttool.services;

public static class EasterEggService {
  public static Subject<bool> ΔIsInBallMode { get; } = new();
}