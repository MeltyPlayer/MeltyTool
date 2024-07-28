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

using fin.io;
using fin.log;
using fin.util.asserts;
using fin.util.linq;

using visceral.decompression;
using visceral.schema.str;
using visceral.schema.str.content;

using FileInfo = visceral.schema.str.content.FileInfo;

namespace visceral.api;

public class StrExtractor {
  private readonly ILogger logger_ = Logging.Create<StrExtractor>();

  public void Extract(IReadOnlyGenericFile strFile, ISystemDirectory outputDir) {
      var task = this.ExtractAsync(strFile, outputDir);
      task.Wait();
    }

  public async Task ExtractAsync(IReadOnlyGenericFile strFile,
                                 ISystemDirectory outputDir) {
      this.logger_.LogInformation($"Extracting {strFile.DisplayFullPath}...");

      ContentBlock[] contentBlocks;
      var headerBlocks = new LinkedList<(FileInfo fileInfo, int index)>();
      {
        var set = strFile.ReadNew<StreamSetFile>();
        contentBlocks =
            set.Contents
               .Select(block => block.Impl.Data)
               .WhereIs<IBlock, ContentBlock>()
               .ToArray();

        for (var i = 0; i < contentBlocks.Length; ++i) {
          var block = contentBlocks[i];
          if (block.Impl.Magic == ContentType.Header) {
            headerBlocks.AddLast(((FileInfo) block.Impl.Data, i));
          }
        }
      }

      var refPackDecompressor = new RefPackArrayDecompressor();
      await Parallel.ForEachAsync(
                        headerBlocks,
                        new ParallelOptions { MaxDegreeOfParallelism = -1, },
                        async (tuple, cancellationToken) => {
                          var (fileInfo, initialIndex) = tuple;

                          var i = initialIndex + 1;
                          ISystemFile outputFile =
                              new FinFile(
                                  Path.Join(outputDir.FullPath, fileInfo.FileName));
                          if (outputFile.Exists) {
                            return;
                          }

                          outputFile.AssertGetParent().Create();

                          await using var output = FinFileSystem.File.Open(
                              outputFile.FullPath,
                              new FileStreamOptions {
                                  Mode = FileMode.Create,
                                  Access = FileAccess.Write,
                                  BufferSize = 0,
                                  PreallocationSize = fileInfo.TotalSize,
                                  Options = FileOptions.SequentialScan |
                                            FileOptions.Asynchronous |
                                            FileOptions.WriteThrough,
                              });

                          var readSize = 0L;
                          while (readSize < fileInfo.TotalSize) {
                            var leftSize = fileInfo.TotalSize - readSize;

                            var dataInfo = contentBlocks[i];
                            switch (dataInfo.Impl.Data) {
                              case UncompressedData uncompressedData: {
                                var data = uncompressedData.Bytes;
                                var writeSize = Math.Min(leftSize, data.Length);
                                await output.WriteAsync(data,
                                  0,
                                  (int) writeSize,
                                  cancellationToken);
                                readSize += writeSize;

                                break;
                              }
                              case RefPackCompressedData compressedData: {
                                Asserts.True(
                                    refPackDecompressor.TryDecompress(
                                        compressedData.RawBytes,
                                        out var data));
                                var writeSize = Math.Min(leftSize,
                                  (uint) data.Length);
                                await output.WriteAsync(data,
                                  0,
                                  (int) writeSize,
                                  cancellationToken);
                                readSize += writeSize;

                                break;
                              }
                              default:
                                throw new InvalidOperationException();
                            }

                            ++i;
                          }

                          await output.FlushAsync(cancellationToken);
                        })
                    .ConfigureAwait(false);
    }
}