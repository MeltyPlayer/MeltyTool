// Copyright (c) 2019 SceneGate

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
namespace SceneGate.Lemon.Containers.Formats;

using Yarhl.FileSystem;

/// <summary>
/// Nintendo Content Container Header.
/// This is the format for the CXI and CFA specialization.
/// It can contain up to two file systems and several special files.
/// </summary>
public class Ncch : NodeContainerFormat
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Ncch"/> class.
    /// </summary>
    public Ncch()
    {
            Header = new NcchHeader();
        }

    /// <summary>
    /// Gets or sets the header.
    /// </summary>
    /// <value>The header.</value>
    public NcchHeader Header {
        get;
        set;
    }
}
