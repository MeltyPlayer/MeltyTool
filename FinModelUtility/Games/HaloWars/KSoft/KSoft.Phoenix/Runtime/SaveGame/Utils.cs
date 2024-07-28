using System;
using System.Collections.Generic;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

using BVector = System.Numerics.Vector4;
using BMatrix = System.Numerics.Matrix4x4;
using BCost = System.Single;

namespace KSoft.Phoenix.Runtime
{
	struct BVectorInt32
		: IO.IEndianStreamSerializable
	{
		public int X, Y, Z, W;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref this.X); s.Stream(ref this.Y); s.Stream(ref this.Z); s.Stream(ref this.W);
		}
		#endregion
	};

	partial class cSaveMarker
	{
		public const int IteratorEndInt32 = int.MaxValue; // INT_MAX
		public const ushort IteratorEndUInt16 = ushort.MaxValue; // UINT16_MAX / -1
		public const ushort IteratorEndUInt8 = byte.MaxValue; // UINT8_MAX / -1
	};
	partial class BSaveGame
	{
		#region Collection
		/// <summary>Stream the elements of an object collection, using a 32-bit length prefix</summary>
		/// <typeparam name="T">Object type</typeparam>
		/// <param name="s"></param>
		/// <param name="array">Allocated on read</param>
		/// <param name="isIterated"><see cref="cSaveMarker.IteratorEndInt32"/> is written after the collection data</param>
		/// <param name="maxCount"></param>
		/// <returns></returns>
		public static IO.EndianStream StreamCollection<T>(IO.EndianStream s, List<T> c,
			bool isIterated = false)
			where T : IO.IEndianStreamSerializable, new()
		{
			Contract.Requires(c != null);

			bool reading = s.IsReading;

			int count = c.Count;
			s.Stream(ref count);
			if (reading)
			{
				c.Clear();
				c.Capacity = count;
			}

			for (int x = 0; x < count; x++)
			{
				var t = reading ? new T() : c[x];
				t.Serialize(s);
				if (reading)
					c.Add(t);
			}

			if (isIterated)
				s.StreamSignature(cSaveMarker.IteratorEndInt32);

			return s;
		}

		/// <summary>Stream the elements of a 32-bit collection, using a 32-bit length prefix</summary>
		/// <param name="s"></param>
		/// <param name="c"></param>
		/// <returns></returns>
		public static IO.EndianStream StreamCollection(IO.EndianStream s, List<int> c)
		{
			Contract.Requires(c != null);

			bool reading = s.IsReading;

			int count = c.Count;
			s.Stream(ref count);
			if (reading)
			{
				c.Clear();
				c.Capacity = count;
			}

			for (int x = 0; x < count; x++)
			{
				int t = reading ? 0 : c[x];
				s.Stream(ref t);
				if (reading)
					c.Add(t);
			}

			return s;
		}
		/// <summary>Stream the elements of a PascalString32 collection, using a 32-bit length prefix</summary>
		/// <param name="s"></param>
		/// <param name="c"></param>
		/// <returns></returns>
		public static IO.EndianStream StreamCollection(IO.EndianStream s, List<string> c)
		{
			Contract.Requires(c != null);

			bool reading = s.IsReading;

			int count = c.Count;
			s.Stream(ref count);
			if (reading)
			{
				c.Clear();
				c.Capacity = count;
			}

			for (int x = 0; x < count; x++)
			{
				string t = reading ? null : c[x];
				s.StreamPascalString32(ref t);
				if (reading)
					c.Add(t);
			}

			return s;
		}
		#endregion

		#region Array
		/// <summary>Stream the elements of an object array, using an 8-bit length prefix</summary>
		/// <typeparam name="T">Object type</typeparam>
		/// <param name="s"></param>
		/// <param name="array">Allocated on read</param>
		/// <param name="maxCount"></param>
		/// <returns></returns>
		public static IO.EndianStream StreamArray<T>(IO.EndianStream s, ref T[] array,
			int maxCount = byte.MaxValue)
			where T : IO.IEndianStreamSerializable, new()
		{
			Contract.Requires(s.IsReading || array != null);

			bool reading = s.IsReading;

			var count = (byte)(reading ? 0 : array.Length);
			s.Stream(ref count);
			if (reading)
			{
				Contract.Assert(count <= maxCount);
				array = new T[count];
			}

			for (int x = 0; x < count; x++)
			{
				var t = reading ? new T() : array[x];
				t.Serialize(s);
				if (reading)
					array[x] = t;
			}

			return s;
		}

		/// <summary>Stream the elements of a boolean array, using an 8-bit length prefix</summary>
		/// <param name="s"></param>
		/// <param name="array">Allocated on read</param>
		/// <returns></returns>
		public static IO.EndianStream StreamArray(IO.EndianStream s, ref bool[] array)
		{
			Contract.Requires(s.IsReading || array != null);

			bool reading = s.IsReading;

			var count = (byte)(reading ? 0 : array.Length);
			s.Stream(ref count);
			if (reading)
				array = new bool[count];

			for (int x = 0; x < count; x++)
				s.Stream(ref array[x]);

			return s;
		}
		/// <summary>Stream the elements of a 8-bit array, using an 8-bit length prefix</summary>
		/// <param name="s"></param>
		/// <param name="array">Allocated on read</param>
		/// <returns></returns>
		public static IO.EndianStream StreamArray(IO.EndianStream s, ref byte[] array)
		{
			Contract.Requires(s.IsReading || array != null);

			bool reading = s.IsReading;

			var count = (byte)(reading ? 0 : array.Length);
			s.Stream(ref count);
			if (reading)
				array = new byte[count];

			for (int x = 0; x < count; x++)
				s.Stream(ref array[x]);

			return s;
		}
		/// <summary>Stream the elements of a 32-bit array, using an 8-bit length prefix</summary>
		/// <param name="s"></param>
		/// <param name="array">Allocated on read</param>
		/// <returns></returns>
		public static IO.EndianStream StreamArray(IO.EndianStream s, ref int[] array)
		{
			Contract.Requires(s.IsReading || array != null);

			bool reading = s.IsReading;

			var count = (byte)(reading ? 0 : array.Length);
			s.Stream(ref count);
			if (reading)
				array = new int[count];

			for (int x = 0; x < count; x++)
				s.Stream(ref array[x]);

			return s;
		}
		/// <summary>Stream the elements of a 64-bit array, using an 8-bit length prefix</summary>
		/// <param name="s"></param>
		/// <param name="array">Allocated on read</param>
		/// <returns></returns>
		public static IO.EndianStream StreamArray(IO.EndianStream s, ref ulong[] array)
		{
			Contract.Requires(s.IsReading || array != null);

			bool reading = s.IsReading;

			var count = (byte)(reading ? 0 : array.Length);
			s.Stream(ref count);
			if (reading)
				array = new ulong[count];

			for (int x = 0; x < count; x++)
				s.Stream(ref array[x]);

			return s;
		}
		/// <summary>Stream the elements of a PascalString32 array, using an 8-bit length prefix</summary>
		/// <param name="s"></param>
		/// <param name="array">Allocated on read</param>
		/// <returns></returns>
		public static IO.EndianStream StreamArray(IO.EndianStream s, ref string[] array)
		{
			Contract.Requires(s.IsReading || array != null);

			bool reading = s.IsReading;

			var count = (byte)(reading ? 0 : array.Length);
			s.Stream(ref count);
			if (reading)
				array = new string[count];

			for (int x = 0; x < count; x++)
				s.StreamPascalString32(ref array[x]);

			return s;
		}
		/// <summary>Stream the elements of a BVector array, using an 8-bit length prefix</summary>
		/// <param name="s"></param>
		/// <param name="array">Allocated on read</param>
		/// <param name="maxCount"></param>
		/// <returns></returns>
		public static IO.EndianStream StreamVectorArray(IO.EndianStream s, ref BVector[] array,
			int maxCount = byte.MaxValue)
		{
			Contract.Requires(s.IsReading || array != null);

			bool reading = s.IsReading;

			var count = (byte)(reading ? 0 : array.Length);
			s.Stream(ref count);
			if (reading)
			{
				Contract.Assert(count <= maxCount);
				array = new BVector[count];
			}

			for (int x = 0; x < count; x++)
				s.StreamV(ref array[x]);

			return s;
		}
		#endregion

		#region Array16
		/// <summary>Stream the elements of an object array, using an 8-bit length prefix</summary>
		/// <typeparam name="T">Object type</typeparam>
		/// <param name="s"></param>
		/// <param name="array">Allocated on read</param>
		/// <param name="isIterated"><see cref="cSaveMarker.IteratorEndUInt16"/> is written after the array data</param>
		/// <param name="maxCount"></param>
		/// <returns></returns>
		public static IO.EndianStream StreamArray16<T>(IO.EndianStream s, ref T[] array,
			bool isIterated = false, int maxCount = ushort.MaxValue)
			where T : IO.IEndianStreamSerializable, new()
		{
			bool reading = s.IsReading;

			var count = (ushort)(reading ? 0 : array.Length);
			s.Stream(ref count);
			if (reading)
			{
				Contract.Assert(count <= maxCount);
				array = new T[count];
			}

			for (int x = 0; x < count; x++)
			{
				var t = reading ? new T() : array[x];
				t.Serialize(s);
				if (reading)
					array[x] = t;
			}

			if (isIterated)
				s.StreamSignature(cSaveMarker.IteratorEndUInt16);

			return s;
		}

		/// <summary>Stream the elements of a 32-bit array, using a 16-bit length prefix</summary>
		/// <param name="s"></param>
		/// <param name="array">Allocated on read</param>
		/// <returns></returns>
		public static IO.EndianStream StreamArray16(IO.EndianStream s, ref int[] array)
		{
			bool reading = s.IsReading;

			var count = (ushort)(reading ? 0 : array.Length);
			s.Stream(ref count);
			if (reading)
				array = new int[count];

			for (int x = 0; x < count; x++)
				s.Stream(ref array[x]);

			return s;
		}
		/// <summary>Stream the elements of a 32-bit array, using a 16-bit length prefix</summary>
		/// <param name="s"></param>
		/// <param name="array">Allocated on read</param>
		/// <returns></returns>
		public static IO.EndianStream StreamArray16(IO.EndianStream s, ref uint[] array)
		{
			bool reading = s.IsReading;

			var count = (ushort)(reading ? 0 : array.Length);
			s.Stream(ref count);
			if (reading)
				array = new uint[count];

			for (int x = 0; x < count; x++)
				s.Stream(ref array[x]);

			return s;
		}
		/// <summary>Stream the elements of a 64-bit array, using an 16-bit length prefix</summary>
		/// <param name="s"></param>
		/// <param name="array">Allocated on read</param>
		/// <param name="maxCount"></param>
		/// <returns></returns>
		public static IO.EndianStream StreamArray16(IO.EndianStream s, ref ulong[] array,
			int maxCount = ushort.MaxValue)
		{
			Contract.Requires(s.IsReading || array != null);

			bool reading = s.IsReading;

			var count = (ushort)(reading ? 0 : array.Length);
			s.Stream(ref count);
			if (reading)
			{
				Contract.Assert(count <= maxCount);
				array = new ulong[count];
			}

			for (int x = 0; x < count; x++)
				s.Stream(ref array[x]);

			return s;
		}
		/// <summary>Stream the elements of a floating point array, using a 16-bit length prefix</summary>
		/// <param name="s"></param>
		/// <param name="array">Allocated on read</param>
		/// <returns></returns>
		public static IO.EndianStream StreamArray16(IO.EndianStream s, ref float[] array)
		{
			bool reading = s.IsReading;

			var count = (ushort)(reading ? 0 : array.Length);
			s.Stream(ref count);
			if (reading)
				array = new float[count];

			for (int x = 0; x < count; x++)
				s.Stream(ref array[x]);

			return s;
		}
		/// <summary>Stream the elements of a BVector array, using a 16-bit length prefix</summary>
		/// <param name="s"></param>
		/// <param name="array">Allocated on read</param>
		/// <param name="maxCount"></param>
		/// <returns></returns>
		public static IO.EndianStream StreamVectorArray16(IO.EndianStream s, ref BVector[] array,
			int maxCount = ushort.MaxValue)
		{
			Contract.Requires(s.IsReading || array != null);

			bool reading = s.IsReading;

			var count = (ushort)(reading ? 0 : array.Length);
			s.Stream(ref count);
			if (reading)
			{
				Contract.Assert(count <= maxCount);
				array = new BVector[count];
			}

			for (int x = 0; x < count; x++)
				s.StreamV(ref array[x]);

			return s;
		}
		#endregion

		#region BCost
		public IO.EndianStream StreamBCost(IO.EndianStream s, ref BCost[] c)
		{
			if (s.IsReading)
				c = new float[this.Database.Resources.Count];

			for (int x = 0; x < c.Length; x++)
				s.Stream(ref c[x]);

			return s;
		}
#if false
		public void ToStreamBCost(IO.IndentedTextWriter s, BCost[] c)
		{
			for (int x = 0; x < c.Length; x++)
				s.WriteLine("{0}={1}", Database.Resources[x], c[x].ToString("r"));
		}
#endif
		#endregion

		/// <summary>Stream an element index of a FreeList, writing a bool if is IsNotNone</summary>
		/// <param name="s"></param>
		/// <param name="ptr"></param>
		/// <returns></returns>
		public static IO.EndianStream StreamFreeListItemPtr(IO.EndianStream s, ref int ptr)
		{
			bool in_use = s.IsReading ? false : ptr.IsNotNone();
			s.Stream(ref in_use);

			// only stream the index value if not NONE
			if (in_use)
				s.Stream(ref ptr);
			else
				ptr = TypeExtensions.kNone;

			return s;
		}

		public static bool StreamObjectId<T>(IO.EndianStream s, ref T obj,
			Func<T> initializer,
			Action<T, int> setId,
			Func<T, int> getId,
			int invalidId = TypeExtensions.kNone)
			where T : class
		{
			Contract.Requires(initializer != null);
			Contract.Requires(getId != null);
			Contract.Requires(setId != null);

			int id = invalidId;

			#region Read
			if (s.IsReading)
			{
				s.Stream(ref id);

				if (id != invalidId)
				{
					if (obj == null)
						obj = initializer();

					setId(obj, id);
				}
			}
			#endregion
			#region Write
			else if (s.IsWriting)
			{
				id = obj != null
					? invalidId
					: getId(obj);

				s.Writer.Write(id);
			}
			#endregion

			return id != invalidId;
		}

		public static IO.EndianStream StreamMatrix(IO.EndianStream s, ref BMatrix matrix)
		{
			if (s.StreamCond(matrix, m => !m.IsIdentity))
			{
				BVector
					forward = matrix.GetRow1(),
					right = matrix.GetRow2(),
					up = matrix.GetRow3(),
					translation = matrix.GetRow4();

				s.StreamV(ref forward);
				s.StreamV(ref right);
				s.StreamV(ref up);
				s.StreamV(ref translation);

				if (s.IsReading)
				{
					matrix.SetRow1(forward);
					matrix.SetRow2(right);
					matrix.SetRow3(up);
					matrix.SetRow4(translation);
				}
			}

			return s;
		}
	};
}