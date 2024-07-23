using System.Collections;
using System.Collections.Generic;
using System.Linq;

using schema.binary;

namespace fin.schema.data;

public class SwitchMagicUInt32SizedSectionMap<TMagic, TData>(
    ISwitchMagicConfig<TMagic, TData> config)
    : IBinaryConvertible, IEnumerable<(TMagic, TData)>
    where TMagic : notnull
    where TData : IBinaryConvertible {
  private readonly List<IMagicSection<TMagic, TData>> impl_ =
      [];

  public void Clear() => this.impl_.Clear();

  public void Add(TData data)
    => this.impl_.Add(new PassThruMagicUInt32SizedSection<TMagic, TData>(
                          config,
                          data));

  public IEnumerable<TData> this[TMagic magic]
    => this.impl_.Where(section => section.Magic.Equals(magic))
           .Select(section => section.Data);

  IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

  public IEnumerator<(TMagic, TData)> GetEnumerator()
    => this.impl_.Select(section => section.Data)
           .Select(data => (config.GetMagic(data), data))
           .GetEnumerator();

  public void Read(IBinaryReader br) {
    while (!br.Eof) {
      var section =
          new SwitchMagicUInt32SizedSection<TMagic, TData>(config);
      section.Read(br);
      this.impl_.Add(section);
    }
  }

  public void Write(IBinaryWriter bw) {
    foreach (var section in this.impl_) {
      section.Write(bw);
    }
  }
}