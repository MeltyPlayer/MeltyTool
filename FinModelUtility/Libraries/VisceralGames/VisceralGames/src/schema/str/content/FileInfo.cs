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

using schema.binary;
using schema.binary.attributes;

namespace visceral.schema.str.content;

[BinarySchema]
public partial class FileInfo : IContent {
  public FileBuild Build { get; set; }
  public ushort Alignment { get; set; }
  public ushort Flags { get; set; }

  public uint Type { get; set; }

  [Unknown]
  public uint Unknown0C { get; set; }

  public uint Type2 { get; set; }

  [Unknown]
  public uint Unknown14 { get; set; }

  // seems to be some kind of hash of the file name
  [Unknown]
  public uint Unknown18 { get; set; }

  public uint TotalSize { get; set; }

  [NullTerminatedString]
  public string BaseName { get; set; }

  [NullTerminatedString]
  public string FileName { get; set; }

  [NullTerminatedString]
  public string TypeName { get; set; }

  public override string ToString() => this.FileName;
}