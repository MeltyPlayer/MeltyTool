using System;

namespace fin.data.disposables;

public interface IFinDisposable : IDisposable {
  bool IsDisposed { get; }
}