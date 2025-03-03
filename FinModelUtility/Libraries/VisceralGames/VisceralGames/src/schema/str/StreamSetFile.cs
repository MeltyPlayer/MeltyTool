/* Copyright (c) 2011 Rick (rick 'at' gibbed 'dot' us)
 *
 * This software is provided 'as-is', without any express or implied
 * warranty. In no event will the authors be held liable for any damages
 * arising from the use of this software.
 *
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 *
 * 1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would
 *    be appreciated but is not required.
 *
 * 2. Altered source versions must be plainly marked as such, and must not
 *    be misrepresented as being the original software.
 *
 * 3. This notice may not be removed or altered from any source
 *    distribution.
 */

using fin.schema;
using fin.schema.data;

using schema.binary;
using schema.binary.attributes;

using visceral.schema.str.content;

namespace visceral.schema.str;

[Endianness(Endianness.LittleEndian)]
[BinarySchema]
public partial class StreamSetFile : IBinaryConvertible {
  private readonly BlockType magic_ = BlockType.Options;
  private readonly uint size_ = 12;

  /* Dead Space:
   * unknown00 = 2
   * unknown02 = 259
   *
   * Dead Space 2:
   * unknown00 = 2
   * unknown02 = 259
   *
   * Dante's Inferno:
   * unknown00 = 2
   * unknown02 = 1537
   */
  [Unknown]
  public ushort Unknown00 { get; set; }

  [Unknown]
  public ushort Unknown02 { get; set; }

  [RSequenceUntilEndOfStream]
  public List<BlockWrapper> Contents { get; } = [];

  [BinarySchema]
  public partial class BlockWrapper : IBinaryConvertible {
    public SwitchMagicUInt32SizedSection<BlockType, IBlock> Impl { get; }
      = new(new BlockConfig()) { TweakReadSize = -8 };

    public override string ToString() => this.Impl.ToString();
  }

  private class BlockConfig : ISwitchMagicConfig<BlockType, IBlock> {
    public BlockType ReadMagic(IBinaryReader br)
      => (BlockType) br.ReadUInt32();

    public void WriteMagic(IBinaryWriter bw, BlockType magic)
      => bw.WriteUInt32((uint) magic);

    public BlockType GetMagic(IBlock data) => data.Type;

    public IBlock CreateData(BlockType magic)
      => magic switch {
          BlockType.Options => new NoopBlock(BlockType.Options),
          BlockType.Content => new ContentBlock(),
          BlockType.Padding => new NoopBlock(BlockType.Padding),
      };
  }
}