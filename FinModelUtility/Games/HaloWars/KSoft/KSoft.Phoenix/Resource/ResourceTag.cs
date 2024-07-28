using System;
using System.IO;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Phoenix.Resource
{
	public sealed class ResourceTag
	{
		public DateTime TimeStamp { get; set; } = DateTime.MinValue;
		public Values.KGuid Guid { get; set; }
		public string MachineName { get; set; }
		public string UserName { get; set; }

		public string SourceFileName { get; private set; }
		public byte[] SourceDigest { get; private set; } = new byte[Security.Cryptography.PhxHash.kSha1SizeOf];
		public long SourceFileSize { get; private set; }
		public DateTime SourceFileTimeStamp { get; private set; }

		public int CreatorToolVersion { get; set; }
		public string CreatorToolCommandLine { get; set; }

		public ResourceTagPlatformId PlatformId { get; set; }

		public bool SetSourceFile(string fileName)
		{
			try
			{
				if (!File.Exists(fileName))
					return false;

				var fileInfo = new FileInfo(fileName);
				var fileSize = fileInfo.Length;
				var createTime = fileInfo.CreationTimeUtc;
				var lastWriteTime = fileInfo.LastWriteTimeUtc;

				this.SourceFileName = fileName;
				Array.Clear(this.SourceDigest, 0, this.SourceDigest.Length);
				this.SourceFileSize = fileSize;
				this.SourceFileTimeStamp = lastWriteTime > createTime
					? lastWriteTime
					: createTime;
			}
			catch (Exception ex)
			{
				Debug.Trace.Resource.TraceInformation(ex.ToString());
				return false;
			}

			return true;
		}

		public bool ComputeSourceFileDigest()
		{
			bool result = false;
			try
			{
				long fileLength;
				result = Security.Cryptography.PhxHash.Sha1HashFile(this.SourceFileName, this.SourceDigest, out fileLength);

				if (result)
				{
					this.SourceFileSize = fileLength;
				}
			}
			catch (Exception ex)
			{
				Debug.Trace.Resource.TraceInformation(ex.ToString());
				return false;
			}
			return result;
		}

		public void SetCreatorToolInfo(int version, string cmdLine)
		{
			Contract.Requires(version >= 0 && version <= byte.MaxValue);

			this.CreatorToolVersion = version;
			this.CreatorToolCommandLine = cmdLine;
		}

		public void ComputeMetadata()
		{
			if (this.Guid.IsNotEmpty)
				return;

			this.TimeStamp = DateTime.UtcNow;
			this.Guid = Values.KGuid.NewGuid();
			this.MachineName = Environment.MachineName;
			this.UserName = Environment.UserName;
		}

		public void PopulateFromStream(IO.EndianStream s, ResourceTagHeader header)
		{
			Contract.Requires(s != null);
			Contract.Requires(s.IsReading);
			Contract.Requires(header != null);

			string streamedString = null;

			this.TimeStamp = DateTime.FromFileTimeUtc((long)header.TagTimeStamp);
			this.Guid = header.TagGuid;
			if (header.StreamTagMachineName(s, ref streamedString))
				this.MachineName = streamedString;
			if (header.StreamTagUserName(s, ref streamedString))
				this.UserName = streamedString;

			if (header.StreamSourceFileNamee(s, ref streamedString))
				this.SourceFileName = streamedString;
			Array.Copy(header.SourceDigest, this.SourceDigest, header.SourceDigest.Length);
			this.SourceFileSize = (long)header.SourceFileSize;
			this.SourceFileTimeStamp = DateTime.FromFileTimeUtc((long)header.SourceFileTimeStamp);

			if (header.StreamCreatorToolCommandLine(s, ref streamedString))
				this.CreatorToolCommandLine = streamedString;
			this.CreatorToolVersion = header.CreatorToolVersion;

			this.PlatformId = header.PlatformId;
		}
	};
}
