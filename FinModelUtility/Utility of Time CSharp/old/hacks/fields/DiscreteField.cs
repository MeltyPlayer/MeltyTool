using System.Collections.Generic;

using UoT.util;

namespace UoT.hacks.fields {
  public interface IDiscreteValue {
    string Name { get; }
  }

  public interface IDiscreteValue<out T> : IDiscreteValue {
    T Value { get; }
  }


  public interface IDiscreteField : IField {
    /// <summary>
    ///   List of possible values for this field. This should NOT ever change.
    /// </summary>
    IReadOnlyList<IDiscreteValue> PossibleValues { get; }

    int SelectedValueIndex { get; }

    void Set(IDiscreteValue value);
  }

  public interface IDiscreteField<T> : IDiscreteField, IField<T> {
    /// <summary>
    ///   List of possible values for this field. This should NOT ever change.
    /// </summary>
    new IReadOnlyList<IDiscreteValue<T>> PossibleValues { get; }

    void Set(IDiscreteValue<T> value);
  }


  public class DiscreteField<T> : IDiscreteField<T> {
    public class Builder(string name) {
      private IDiscreteValue<T>? defaultValue_;

      private readonly List<IDiscreteValue<T>> possibleValues_ = [];

      public Builder AddPossibleValue(
          string name,
          T value,
          bool isDefault = false) {
        var newValue = new DiscreteValue(name, value);

        this.possibleValues_.Add(newValue);
        if (isDefault) {
          this.defaultValue_ = newValue;
        }

        return this;
      }

      public DiscreteField<T> Build() {
        var defaultValue = this.defaultValue_ ?? this.possibleValues_[0];

        return new DiscreteField<T>(
            name,
            defaultValue.Value,
            this.possibleValues_.AsReadOnly()
        );
      }

      private class DiscreteValue(string name, T value) : IDiscreteValue<T> {
        public string Name { get; } = name;
        public T Value { get; } = value;
      }
    }

    private DiscreteField(
        string name,
        T value,
        IReadOnlyList<IDiscreteValue<T>> possibleValues) {
      this.Name = name;
      this.Value = value;
      this.PossibleValues = possibleValues;
    }


    public string Name { get; }
    public T Value { get; set; }


    IReadOnlyList<IDiscreteValue> IDiscreteField.PossibleValues
      => this.PossibleValues;

    public IReadOnlyList<IDiscreteValue<T>> PossibleValues { get; }


    public int SelectedValueIndex {
      get {
        for (var i = 0; i < this.PossibleValues.Count; ++i) {
          var possibleValue = this.PossibleValues[i].Value;
          if (this.Value?.Equals(possibleValue) ??
              false || (this.Value == null && possibleValue == null)) {
            return i;
          }
        }
        return -1;
      }
    }


    void IDiscreteField.Set(IDiscreteValue value)
      => this.Set(Asserts.Assert(value as IDiscreteValue<T>));

    public void Set(IDiscreteValue<T> value) {
      this.Value = value.Value;
    }
  }
}