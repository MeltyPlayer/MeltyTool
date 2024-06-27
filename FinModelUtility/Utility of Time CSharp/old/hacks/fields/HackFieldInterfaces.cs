namespace UoT.hacks.fields {
  public interface IField {
    public string Name { get; }
  }

  public interface IField<T> : IField {
    public T Value { get; set; }
  }
}
