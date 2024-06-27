namespace fin.data;

public class Counter {
  public Counter(int startingValue = 0) {
    this.Value = startingValue;
  }

  public int Value { get; set; }

  public int GetAndIncrement() => this.Value++;
}