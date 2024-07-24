using System;
using System.Runtime.InteropServices;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Phoenix.Engine
{
	public static class EditorUtilsDLL
	{
		public enum LibraryMode : uint
		{
			Debug,
			Release,
		};

		const uint kVersion = 3;

		const string kDllName = @"Engine\EditorUtils.dll";
		const CallingConvention kCallConv = CallingConvention.Cdecl;
		const CharSet kCharSet = CharSet.Ansi;

		public static bool Initialized { get; private set; }

		internal static bool gEntryPointsNotFound;
		public static bool EntryPointsNotFound
		{
			get
			{
				if (!Initialized && !gEntryPointsNotFound && !gIsInitializing)
					Initialize();

				return gEntryPointsNotFound;
			}
			private set { gEntryPointsNotFound = value; }
		}

		public static void HandleEntryPointNotFound(EntryPointNotFoundException ex)
		{
			if (EntryPointsNotFound)
				return;

			EntryPointsNotFound = true;
			Phoenix.Debug.Trace.Phoenix.TraceData(System.Diagnostics.TraceEventType.Critical, TypeExtensions.kNone,
				"Failed to find a EditorUtils method",
				ex);
		}

		private static bool gIsInitializing;
		public static void Initialize()
		{
			if (Initialized)
				return;

			try
			{
				gIsInitializing = true;

				var libPointerSize = GetPointerSize();
				if (libPointerSize != IntPtr.Size)
				{
					Phoenix.Debug.Trace.Phoenix.TraceData(System.Diagnostics.TraceEventType.Critical, TypeExtensions.kNone,
						"EditorUtils was built for a platform that doesn't match what we're currently running in",
						libPointerSize,
						IntPtr.Size);
					return;
				}

				var version = GetLibraryVersion();
				if (version != kVersion)
				{
					Phoenix.Debug.Trace.Phoenix.TraceData(System.Diagnostics.TraceEventType.Critical, TypeExtensions.kNone,
						"EditorUtils version doesn't match what we expected",
						version,
						kVersion);
					return;
				}

				var libMode = GetLibraryMode();
				Phoenix.Debug.Trace.Phoenix.TraceData(System.Diagnostics.TraceEventType.Information, TypeExtensions.kNone,
					"Finished loading EditorUtils",
					libMode,
					version);

				Initialized = true;
			}
			catch (EntryPointNotFoundException ex)
			{
				HandleEntryPointNotFound(ex);
			}
			finally
			{
				gIsInitializing = false;
			}
		}

		public static void Dispose()
		{
			if (!Initialized)
				return;

			Initialized = false;
			EntryPointsNotFound = false;
		}

		[DllImport(kDllName, CallingConvention=kCallConv, CharSet=kCharSet)]
		static extern uint GetPointerSize();
		[DllImport(kDllName, CallingConvention=kCallConv, CharSet=kCharSet)]
		static extern LibraryMode GetLibraryMode();
		[DllImport(kDllName, CallingConvention=kCallConv, CharSet=kCharSet)]
		static extern uint GetLibraryVersion();

		#region Tile/Untile and copy data
		public enum TileCopyFormat : int
		{
			R16F = 0,
			R8G8 = 1,
			DXN = 2,
			L8 = 3,
			DXT5A = 4,
			R11G11B10 = 5,
			DXT1 = 6,
			G16R16 = 7,
		};

		[DllImport(kDllName, CallingConvention=kCallConv, SetLastError=true)]
		static extern bool TileCopyData(
			[Out] byte[] dst,
			[In] byte[] src,
			int width,
			int height,
			TileCopyFormat dxtFormat);
		[DllImport(kDllName, CallingConvention=kCallConv, SetLastError=true)]
		static extern bool UntileCopyData(
			[Out] byte[] dst,
			[In] byte[] src,
			int width,
			int height,
			TileCopyFormat dxtFormat);

		[DllImport(kDllName, CallingConvention=kCallConv, SetLastError=true)]
		static extern bool TileCopyData(
			[Out] short[] dst,
			[In] short[] src,
			int width,
			int height,
			TileCopyFormat dxtFormat);
		[DllImport(kDllName, CallingConvention=kCallConv, SetLastError=true)]
		static extern bool UntileCopyData(
			[Out] short[] dst,
			[In] short[] src,
			int width,
			int height,
			TileCopyFormat dxtFormat);

		[DllImport(kDllName, CallingConvention=kCallConv, SetLastError=true)]
		static extern bool TileCopyData(
			[Out] uint[] dst,
			[In] uint[] src,
			int width,
			int height,
			TileCopyFormat dxtFormat = TileCopyFormat.R11G11B10);
		[DllImport(kDllName, CallingConvention=kCallConv, SetLastError=true)]
		static extern bool UntileCopyData(
			[Out] uint[] dst,
			[In] uint[] src,
			int width,
			int height,
			TileCopyFormat dxtFormat = TileCopyFormat.R11G11B10);

		public static bool TileCopyData(
			Array dstArray,
			Array srcArray,
			int width,
			int height,
			TileCopyFormat dxtFormat)
		{
			Contract.Requires(width > 0);
			Contract.Requires(height > 0);

			if (EditorUtilsDLL.EntryPointsNotFound)
				return false;

			if (dstArray == null || srcArray == null)
				return false;

			try
			{
				if (dstArray is byte[] && srcArray is byte[])
					return TileCopyData((byte[])dstArray, (byte[])srcArray, width, height, dxtFormat);
				if (dstArray is short[] && srcArray is short[])
					return TileCopyData((short[])dstArray, (short[])srcArray, width, height, dxtFormat);
				if (dstArray is uint[] && srcArray is uint[])
					return TileCopyData((uint[])dstArray, (uint[])srcArray, width, height, dxtFormat);
			}
			catch (EntryPointNotFoundException ex)
			{
				HandleEntryPointNotFound(ex);
				return false;
			}

			Phoenix.Debug.Trace.Phoenix.TraceData(System.Diagnostics.TraceEventType.Critical, TypeExtensions.kNone,
				"EditorUtils.TileCopyData called with a destination and/or source array type that is not supported",
				dstArray.GetType(),
				srcArray.GetType(),
				dxtFormat);
			return false;
		}

		public static bool UntileCopyData(
			Array dstArray,
			Array srcArray,
			int width,
			int height,
			TileCopyFormat dxtFormat)
		{
			Contract.Requires(width > 0);
			Contract.Requires(height > 0);

			if (EditorUtilsDLL.EntryPointsNotFound)
				return false;

			if (dstArray == null || srcArray == null)
				return false;

			try
			{
				if (dstArray is byte[] && srcArray is byte[])
					return UntileCopyData((byte[])dstArray, (byte[])srcArray, width, height, dxtFormat);
				if (dstArray is short[] && srcArray is short[])
					return UntileCopyData((short[])dstArray, (short[])srcArray, width, height, dxtFormat);
				if (dstArray is uint[] && srcArray is uint[])
					return UntileCopyData((uint[])dstArray, (uint[])srcArray, width, height, dxtFormat);
			}
			catch (EntryPointNotFoundException ex)
			{
				HandleEntryPointNotFound(ex);
				return false;
			}

			Phoenix.Debug.Trace.Phoenix.TraceData(System.Diagnostics.TraceEventType.Critical, TypeExtensions.kNone,
				"EditorUtils.UntileCopyData called with a destination and/or source array type that is not supported",
				dstArray.GetType(),
				srcArray.GetType(),
				dxtFormat);
			return false;
		}
		#endregion
	};
}

