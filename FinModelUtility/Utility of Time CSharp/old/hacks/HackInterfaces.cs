using System.Collections.Generic;

using UoT.hacks.fields;

namespace UoT.hacks;

public interface IHack {
  IReadOnlyList<IField> Fields { get; }
}