using System;
using System.Collections.Generic;
using System.IO;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Phoenix.Resource.ECF
{
	public sealed class EcfFileDefinition
		: IO.ITagElementStringNameStreamable
	{
		public const string kFileExtension = ".ecfdef";

		public string WorkingDirectory { get; set; }

		/// <summary>This should be the source file's name (without extension) or a user defined name</summary>
		public string EcfName { get; private set; }
		public string EcfFileExtension { get; private set; }
		public uint HeaderId { get; private set; }
		public uint ChunkExtraDataSize { get; private set; }

		public List<EcfFileChunkDefinition> Chunks { get; private set; }
			= new List<EcfFileChunkDefinition>();

		#region ITagElementStringNameStreamable
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			using (s.EnterUserDataBookmark(this))
			{
				s.StreamAttributeOpt("name", this, obj => this.EcfName, Predicates.IsNotNullOrEmpty);
				s.StreamAttributeOpt("ext", this, obj => this.EcfFileExtension, Predicates.IsNotNullOrEmpty);

				using (s.EnterCursorBookmark("Header"))
				{
					s.StreamAttribute("id", this, obj => this.HeaderId, NumeralBase.Hex);
					s.StreamAttributeOpt("ChunkExtraDataSize", this, obj => this.ChunkExtraDataSize, Predicates.IsNotZero, NumeralBase.Hex);
				}

				using (var bm = s.EnterCursorBookmarkOpt("Chunks", this.Chunks, Predicates.HasItems))
					s.StreamableElements("C", this.Chunks, obj => obj.HasPossibleFileData);
			}

			// #NOTE leaving this as an exercise for the caller instead, so they can yell when something is culled
			#if false
			if (s.IsReading)
			{
				CullChunksPossiblyWithoutFileData();
			}
			#endif
		}

		internal void CullChunksPossiblyWithoutFileData(
			Action<int, EcfFileChunkDefinition> cullCallback = null)
		{
			for (int x = this.Chunks.Count - 1; x >= 0; x--)
			{
				var chunk = this.Chunks[x];
				if (!chunk.HasPossibleFileData)
				{
					if (cullCallback != null)
					{
						cullCallback(x, chunk);
					}

					this.Chunks.RemoveAt(x);
					continue;
				}
			}
		}
		#endregion

		public string GetChunkAbsolutePath(EcfFileChunkDefinition chunk)
		{
			Contract.Requires(chunk != null && chunk.Parent == this);

			Contract.Assert(this.WorkingDirectory.IsNotNullOrEmpty());
			string abs_path = Path.Combine(this.WorkingDirectory, chunk.FilePath);

			abs_path = Path.GetFullPath(abs_path);
			return abs_path;
		}

		public void Initialize(string ecfFileName)
		{
			this.EcfName = ecfFileName;
			if (this.EcfName.IsNotNullOrEmpty())
			{
				this.EcfFileExtension = Path.GetExtension(this.EcfName);
				// We don't use GetFileNameWithoutExtension because there are cases where
				// files only differ by their extensions (like Terrain data XTD, XSD, etc)
				this.EcfName = Path.GetFileName(this.EcfName);
			}
		}

		public void CopyHeaderData(EcfHeader header)
		{
			this.HeaderId = header.Id;
			this.ChunkExtraDataSize = header.ExtraDataSize;
		}

		public void UpdateHeader(ref EcfHeader header)
		{
			Contract.Requires<InvalidOperationException>(this.HeaderId != 0);

			header.InitializeChunkInfo(this.HeaderId, this.ChunkExtraDataSize);
		}

		public EcfFileChunkDefinition Add(EcfChunk rawChunk, int rawChunkIndex)
		{
			Contract.Requires(rawChunk != null);

			var chunk = new EcfFileChunkDefinition();
			chunk.Initialize(this, rawChunk, rawChunkIndex);
			this.Chunks.Add(chunk);

			return chunk;
		}

		internal MemoryStream GetChunkFileDataStream(EcfFileChunkDefinition chunk)
		{
			Contract.Assume(chunk != null && chunk.Parent == this);

			MemoryStream ms;

			if (chunk.FileBytes != null)
			{
				ms = new MemoryStream(chunk.FileBytes, writable: false);
			}
			else
			{
				var source_file = this.GetChunkAbsolutePath(chunk);
				using (var fs = File.OpenRead(source_file))
				{
					ms = new MemoryStream((int)fs.Length);
					fs.CopyTo(ms);
				}
			}

			ms.Position = 0;
			return ms;
		}
	};
}