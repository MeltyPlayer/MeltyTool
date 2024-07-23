namespace fin.data;

public class Counter(int startingValue = 0) {
  public int Value { get; set; } = startingValue;

  public int GetAndIncrement() => this.Value++;
}