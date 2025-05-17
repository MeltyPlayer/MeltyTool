namespace MariosPicross;

public interface IPuzzleDefinition {
  string Name { get; set; }

  byte Width { get; }
  byte Height { get; }

  bool this[int x, int y] { get; }
}