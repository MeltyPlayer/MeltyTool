using System.Reactive.Subjects;

namespace marioartisttool.services;

public static class EasterEggService {
  public static BehaviorSubject<bool> ΔIsInBallMode { get; } = new(false);
}